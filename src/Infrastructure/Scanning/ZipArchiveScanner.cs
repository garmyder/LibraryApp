using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using LibraryApp.Core.Interfaces;

namespace LibraryApp.Infrastructure.Scanning;

internal sealed class ZipArchiveScanner(IEncodingDetector encodingDetector) : IArchiveScanner
{
    static ZipArchiveScanner()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static readonly HashSet<string> SupportedEntryExtensions =
        new([".fb2", ".epub", ".pdf", ".mobi"], StringComparer.OrdinalIgnoreCase);

    public bool CanHandle(string filePath) =>
        Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase);

    public async IAsyncEnumerable<ArchivedBook> ExtractBooksAsync(
        string archivePath,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var nameEncoding = encodingDetector.DetectFile(archivePath);

        await using var zip = new ZipArchive(
            File.OpenRead(archivePath),
            ZipArchiveMode.Read,
            leaveOpen: false,
            entryNameEncoding: nameEncoding);

        foreach (var entry in zip.Entries)
        {
            ct.ThrowIfCancellationRequested();
            if (!SupportedEntryExtensions.Contains(Path.GetExtension(entry.Name))) continue;
            if (entry.Length == 0) continue;

            var ms = new MemoryStream((int)Math.Min(entry.Length, int.MaxValue));
            await using (var entryStream = await entry.OpenAsync(ct))
                await entryStream.CopyToAsync(ms, ct);

            ms.Seek(0, SeekOrigin.Begin);
            yield return new ArchivedBook(entry.FullName, ms, entry.Length);
        }
    }

    /// <inheritdoc />
    public Task<int> CountEntriesAsync(string archivePath, CancellationToken ct = default)
    {
        var nameEncoding = encodingDetector.DetectFile(archivePath);

        using var zip = new ZipArchive(
            File.OpenRead(archivePath),
            ZipArchiveMode.Read,
            leaveOpen: false,
            entryNameEncoding: nameEncoding);

        var count = zip.Entries.Count(e =>
        {
            ct.ThrowIfCancellationRequested();
            return SupportedEntryExtensions.Contains(Path.GetExtension(e.Name)) && e.Length > 0;
        });

        return Task.FromResult(count);
    }
}
