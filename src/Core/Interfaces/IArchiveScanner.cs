// Core/Interfaces/IArchiveScanner.cs

using System.IO;

namespace LibraryApp.Core.Interfaces;

public interface IArchiveScanner
{
    bool CanHandle(string filePath);

    /// <summary>Yields each supported book entry from the archive.</summary>
    IAsyncEnumerable<ArchivedBook> ExtractBooksAsync(
        string archivePath, CancellationToken ct);
}

public sealed record ArchivedBook(
    string OriginalName,   // relative entry path, e.g. "Author/Book.fb2"
    Stream Content,        // caller disposes
    long   SizeBytes) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => Content.DisposeAsync();
}