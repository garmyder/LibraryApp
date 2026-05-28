using System.IO;

namespace LibraryApp.Core.Interfaces;

public interface IArchiveScanner
{
    bool CanHandle(string filePath);

    /// <summary>Yields each supported book entry from the archive.</summary>
    IAsyncEnumerable<ArchivedBook> ExtractBooksAsync(string archivePath, CancellationToken ct);

    /// <summary>Returns the count of supported book entries inside the archive.</summary>
    Task<int> CountEntriesAsync(string archivePath, CancellationToken ct = default);
}

public sealed record ArchivedBook(
    string OriginalName,
    Stream Content,
    long   SizeBytes) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}
