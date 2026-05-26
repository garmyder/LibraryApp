using LibraryApp.Core.Domain;

namespace LibraryApp.Core.Interfaces;

public interface IMetadataParser
{
    /// <summary>Returns true if this parser handles the given file extension (e.g. ".fb2").</summary>
    bool CanParse(string extension);

    /// <summary>Parses book metadata from the file at <paramref name="filePath"/>.</summary>
    Task<BookMetadata> ParseAsync(string filePath, CancellationToken ct = default);
}