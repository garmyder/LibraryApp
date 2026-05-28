namespace LibraryApp.Core.Interfaces;

public interface IBookScanner
{
    /// <summary>Scans a directory and streams metadata for each supported file.</summary>
    IAsyncEnumerable<ScannedFile> ScanAsync(
        string      directoryPath,
        ScanOptions options,
        CancellationToken ct = default);

    /// <summary>
    /// Pre-counts all supported book files in the directory, including entries inside archives.
    /// Reports a running total via <paramref name="progress"/> after each file is counted.
    /// </summary>
    Task<int> CountAsync(
        string           directoryPath,
        ScanOptions      options,
        IProgress<int>?  progress = null,
        CancellationToken ct = default);
}
