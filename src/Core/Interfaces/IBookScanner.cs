namespace LibraryApp.Core.Interfaces;

public interface IBookScanner
{
    /// <summary>Scans a directory and streams metadata for each supported file.</summary>
    IAsyncEnumerable<ScannedFile> ScanAsync(
        string directoryPath,
        ScanOptions options,
        CancellationToken ct = default);
}