// src/Infrastructure/Services/ImportService.cs

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
        string directoryPath,
        ScanOptions options,
        IProgress<ImportProgressEvent>? progress = null,
        CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            if (options.Mode == ImportMode.Recreate)
                await ClearLibraryAsync(ct);

            var report = await ExecuteImportAsync(directoryPath, options, progress, ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation(
                "Import completed [{Mode}] - added: {Added}, updated: {Updated}, skipped: {Skipped}, " +
                "removed: {Removed}, failed: {Failed}",
                options.Mode, report.Added, report.Updated, report.Skipped, report.Removed, report.Failed);

            return report;
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            logger.LogWarning("Import cancelled - all changes rolled back.");
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            logger.LogError(ex, "Import failed - all changes rolled back.");
            throw;
        }
    }

    // ── Recreate helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Removes all book and author content in dependency order.
    /// Reference/lookup tables (Genres, Languages, Tags) are intentionally preserved.
    /// Foreign key checks are suspended for the duration of the clear to avoid
    /// constraint errors caused by deletion order edge cases.
    /// </summary>
    private async Task ClearLibraryAsync(CancellationToken ct)
    {
        await ExecuteSqlAsync("PRAGMA foreign_keys = OFF", ct);
        try
        {
            // Junction tables first, then dependents, then root entities
            await ExecuteSqlAsync("DELETE FROM BookAuthors", ct);
            await ExecuteSqlAsync("DELETE FROM BookTags",    ct);
            await ExecuteSqlAsync("DELETE FROM Books",       ct);
            await ExecuteSqlAsync("DELETE FROM Authors",     ct);
            await ExecuteSqlAsync("DELETE FROM Series",      ct);
        }
        finally
        {
            await ExecuteSqlAsync("PRAGMA foreign_keys = ON", ct);
        }

        logger.LogInformation("Library content cleared for Recreate import.");
    }

    private Task ExecuteSqlAsync(string sql, CancellationToken ct) =>
        db.Database.ExecuteSqlRawAsync(sql, ct);

    // ── Core logic ────────────────────────────────────────────────────────

    private async Task<ImportReport> ExecuteImportAsync(
        string directoryPath,
        ScanOptions options,
        IProgress<ImportProgressEvent>? progress,
        CancellationToken ct)
    {
        int added = 0, updated = 0, skipped = 0, failed = 0, removed = 0;
        var errors = new List<string>();

        var hashIndex   = await db.Books.Where(b => b.FileHash != null)
                                        .ToDictionaryAsync(b => b.FileHash!, ct);
        var pathIndex   = await db.Books.ToDictionaryAsync(b => b.FilePath, ct);
        var authorCache = new AuthorCache(db);
        var lookupCache = new LookupCache(db);

        await foreach (var file in scanner.ScanAsync(directoryPath, options, ct))
        {
            if (file.Error is not null)
            {
                failed++;
                errors.Add($"{file.FilePath}: {file.Error}");
                logger.LogWarning("Parse error for {File}: {Error}", file.FilePath, file.Error);
                progress?.Report(new ImportProgressEvent(
                    ImportStatus.Failed, file.FilePath, null, null, file.Error));
                continue;
            }

            var meta       = file.Metadata!;
            var authorName = FormatAuthor(meta.Authors.FirstOrDefault());

            if (hashIndex.TryGetValue(file.FileHash, out var existing))
            {
                skipped++;
                progress?.Report(new ImportProgressEvent(
                    ImportStatus.Skipped, file.FilePath, existing.Title, null));
                continue;
            }

            if (pathIndex.TryGetValue(file.FilePath, out var byPath))
            {
                byPath.UpdateFromScan(meta.Title, meta.Annotation, meta.Published, file.FileHash);
                updated++;
                progress?.Report(new ImportProgressEvent(
                    ImportStatus.Updated, file.FilePath, meta.Title, authorName));
            }
            else
            {
                var book = await CreateBookAsync(file, meta, lookupCache, ct);
                await db.Books.AddAsync(book, ct);
                await AssignAuthorsAsync(book, meta.Authors, authorCache, ct);
                added++;
                progress?.Report(new ImportProgressEvent(
                    ImportStatus.Added, file.FilePath, meta.Title, authorName));
            }
        }

        // RemoveMissing is only applicable in Update mode
        if (options.Mode == ImportMode.Update && options.RemoveMissing)
            removed += await RemoveMissingBooksAsync(directoryPath, options.Recursive, ct);

        await db.SaveChangesAsync(ct);
        return new ImportReport(added, updated, skipped, removed, failed, errors);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static async Task<Book> CreateBookAsync(
        ScannedFile file, BookMetadata meta, LookupCache cache, CancellationToken ct)
    {
        var genreId  = await cache.GetOrCreateGenreIdAsync(meta.Genre, ct);
        var langId   = await cache.GetOrCreateLanguageIdAsync(meta.Language, ct);
        var seriesId = await cache.GetOrCreateSeriesIdAsync(meta.SeriesName, ct);
        var format   = Book.DetectFormat(file.FilePath);

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

        var missing = booksInDir
            .Where(b => !IsPresent(b.FilePath, presentPaths))
            .ToList();

        db.Books.RemoveRange(missing);
        return missing.Count;
    }

    private static bool IsPresent(string filePath, HashSet<string> presentPaths)
    {
        var sep      = filePath.IndexOf("::", StringComparison.Ordinal);
        var realPath = sep >= 0 ? filePath[..sep] : filePath;
        return presentPaths.Contains(realPath);
    }

    /// <summary>Formats author name as "LastName FirstName", trimming whitespace.</summary>
    private static string? FormatAuthor(AuthorMetadata? a) =>
        a is null ? null : $"{a.LastName} {a.FirstName}".Trim();
}
