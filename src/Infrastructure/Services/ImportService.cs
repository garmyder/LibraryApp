using System.IO;
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces;
using LibraryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Infrastructure.Services;

internal sealed class ImportService(
    IBookScanner scanner,
    LibraryDbContext db,
    ILogger<ImportService> logger) : IImportService
{
    public async Task<ImportReport> ImportAsync(
        string directoryPath, ScanOptions options, CancellationToken ct = default)
    {
        int added = 0, updated = 0, skipped = 0, failed = 0, removed = 0;
        var errors = new List<string>();

        // Pre-load lookup caches to minimise DB round-trips
        var hashIndex  = await db.Books.Where(b => b.FileHash != null)
                                       .ToDictionaryAsync(b => b.FileHash!, ct);
        var pathIndex  = await db.Books.ToDictionaryAsync(b => b.FilePath, ct);
        var authorCache = new AuthorCache(db);
        var lookupCache = new LookupCache(db);

        await foreach (var file in scanner.ScanAsync(directoryPath, options, ct))
        {
            if (file.Error is not null)
            {
                failed++;
                errors.Add($"{file.FilePath}: {file.Error}");
                logger.LogWarning("Parse error for {File}: {Error}", file.FilePath, file.Error);
                continue;
            }

            var meta = file.Metadata!;

            if (hashIndex.TryGetValue(file.FileHash, out var existing))
            {
                // Exact same content — skip
                skipped++;
                continue;
            }

            if (pathIndex.TryGetValue(file.FilePath, out var byPath))
            {
                // File at same path but content changed → update metadata
                byPath.UpdateFromScan(meta.Title, meta.Annotation, meta.Published, file.FileHash);
                updated++;
            }
            else
            {
                // New book
                var book = await CreateBookAsync(file, meta, lookupCache, ct);
                await db.Books.AddAsync(book, ct);
                await AssignAuthorsAsync(book, meta.Authors, authorCache, ct);
                added++;
            }
        }

        if (options.RemoveMissing)
            removed += await RemoveMissingBooksAsync(directoryPath, options.Recursive, ct);

        await db.SaveChangesAsync(ct);

        return new ImportReport(added, updated, skipped, removed, failed, errors);
    }

    private async Task<Book> CreateBookAsync(
        ScannedFile file, BookMetadata meta, LookupCache cache, CancellationToken ct)
    {
        var genreId    = await cache.GetOrCreateGenreIdAsync(meta.Genre, ct);
        var langId     = await cache.GetOrCreateLanguageIdAsync(meta.Language, ct);
        var seriesId   = await cache.GetOrCreateSeriesIdAsync(meta.SeriesName, ct);
        var format     = Book.DetectFormat(file.FilePath);

        var book = new Book(
            title:        meta.Title,
            filePath:     file.FilePath,
            addedDate:    DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            format:       format,
            seriesId:     seriesId,
            seriesNumber: meta.SeriesNumber,
            genreId:      genreId,
            languageId:   langId,
            annotation:   meta.Annotation,
            published:    meta.Published);

        book.SetFileHash(file.FileHash);
        return book;
    }

    private static async Task AssignAuthorsAsync(
        Book book, IReadOnlyList<AuthorMetadata> authorMeta,
        AuthorCache cache, CancellationToken ct)
    {
        foreach (var am in authorMeta)
        {
            var author = await cache.GetOrCreateAsync(am, ct);
            book.Authors.Add(author);
        }
    }

    private async Task<int> RemoveMissingBooksAsync(
        string directory, bool recursive, CancellationToken ct)
    {
        var option = recursive
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        var presentPaths = Directory
            .EnumerateFiles(directory, "*.*", option)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var booksInDir = await db.Books
            .Where(b => b.FilePath.StartsWith(directory))
            .ToListAsync(ct);

        // For virtual paths (archive::entry), check that the ARCHIVE exists.
        var missing = booksInDir
            .Where(b => !IsPresent(b.FilePath, presentPaths))
            .ToList();

        db.Books.RemoveRange(missing);
        return missing.Count;
    }

    /// <summary>
    /// Returns true if the book's backing file (or archive) still exists on disk.
    /// Virtual paths have the form: "path/to/archive.zip::entry/name.fb2"
    /// </summary>
    private static bool IsPresent(string filePath, HashSet<string> presentPaths)
    {
        var sep = filePath.IndexOf("::", StringComparison.Ordinal);
        var realPath = sep >= 0 ? filePath[..sep] : filePath;
        return presentPaths.Contains(realPath);
    }
}