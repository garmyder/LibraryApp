using System.IO;
using System.Runtime.CompilerServices;
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces;

namespace LibraryApp.Infrastructure.Scanning;

internal sealed class BookScanner(
    IEnumerable<IMetadataParser>  parsers,
    IEnumerable<IArchiveScanner>  archiveScanners,
    IFileHasher                   hasher) : IBookScanner
{
    private static readonly HashSet<string> BookExtensions = BookFormatExtensions.SupportedExtensions;

    private static readonly HashSet<string> ArchiveExtensions =
        new([".zip"], StringComparer.OrdinalIgnoreCase);

    public async IAsyncEnumerable<ScannedFile> ScanAsync(
        string directoryPath,
        ScanOptions options,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var searchOption = options.Recursive
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        var allFiles = Directory.EnumerateFiles(directoryPath, "*.*", searchOption);

        foreach (var filePath in allFiles)
        {
            ct.ThrowIfCancellationRequested();

            var ext = Path.GetExtension(filePath);

            if (BookExtensions.Contains(ext))
            {
                yield return await ScanFileAsync(filePath, ct);
                continue;
            }

            if (ArchiveExtensions.Contains(ext))
            {
                var scanner = archiveScanners.FirstOrDefault(s => s.CanHandle(filePath));
                if (scanner is null) continue;

                await foreach (var scanned in ScanArchiveAsync(filePath, scanner, ct))
                    yield return scanned;
            }
        }
    }

    // ── Direct file ───────────────────────────────────────────────────────

    private async Task<ScannedFile> ScanFileAsync(string filePath, CancellationToken ct)
    {
        try
        {
            var parser = FindParser(filePath)
                ?? throw new NotSupportedException($"No parser for '{Path.GetExtension(filePath)}'.");

            var hash     = await hasher.ComputeHashAsync(filePath, ct);
            var fileSize = new FileInfo(filePath).Length;
            var metadata = await parser.ParseAsync(filePath, ct);

            return new ScannedFile(filePath, hash, fileSize, metadata, Error: null);
        }
        catch (Exception ex)
        {
            return ErrorFile(filePath, ex.Message);
        }
    }

    // ── Archive ───────────────────────────────────────────────────────────

    private async IAsyncEnumerable<ScannedFile> ScanArchiveAsync(
        string archivePath,
        IArchiveScanner scanner,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var archived in scanner.ExtractBooksAsync(archivePath, ct))
        {
            await using (archived.Content)
                yield return await ScanArchivedBookAsync(archivePath, archived, ct);
        }
    }

    private async Task<ScannedFile> ScanArchivedBookAsync(
        string archivePath, ArchivedBook archived, CancellationToken ct)
    {
        var ext    = Path.GetExtension(archived.OriginalName);
        var parser = FindParser(ext);

        if (parser is null)
            return ErrorFile(archivePath, $"No parser for '{ext}' inside archive.");

        // Write to temp file — parsers expect a real path (XML readers, etc.)
        var tempPath = Path.ChangeExtension(Path.GetTempFileName(), ext);
        try
        {
            await using (var fs = File.Create(tempPath))
                await archived.Content.CopyToAsync(fs, ct);

            var hash        = await hasher.ComputeHashAsync(tempPath, ct);
            var metadata    = await parser.ParseAsync(tempPath, ct);
            var virtualPath = BuildVirtualPath(archivePath, archived.OriginalName);

            return new ScannedFile(virtualPath, hash, archived.SizeBytes, metadata, Error: null);
        }
        catch (Exception ex)
        {
            return ErrorFile(BuildVirtualPath(archivePath, archived.OriginalName), ex.Message);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private IMetadataParser? FindParser(string filePathOrExt)
    {
        var ext = filePathOrExt.StartsWith('.') ? filePathOrExt : Path.GetExtension(filePathOrExt);
        return parsers.FirstOrDefault(p => p.CanParse(ext));
    }

    /// <summary>Builds a virtual path used as the stable book identifier in DB.</summary>
    internal static string BuildVirtualPath(string archivePath, string entryName) =>
        $"{archivePath}::{entryName}";

    private static ScannedFile ErrorFile(string path, string error) =>
        new(path, string.Empty, 0, Metadata: null, error);
}
