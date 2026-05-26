using System.IO;
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces;
using UglyToad.PdfPig;

namespace LibraryApp.Infrastructure.Scanning.Parsers;

internal sealed class PdfMetadataParser : IMetadataParser
{
    public bool CanParse(string extension)
        => extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<BookMetadata> ParseAsync(string filePath, CancellationToken ct = default)
    {
        // PdfPig is synchronous; offload to thread pool to not block async context
        return Task.Run(() => Parse(filePath), ct);
    }

    private static BookMetadata Parse(string filePath)
    {
        using var pdf  = PdfDocument.Open(filePath);
        var info = pdf.Information;

        var title  = info.Title?.Trim();
        var author = info.Author?.Trim();

        if (string.IsNullOrWhiteSpace(title))
            title = Path.GetFileNameWithoutExtension(filePath);

        var authors = string.IsNullOrWhiteSpace(author)
            ? (IReadOnlyList<AuthorMetadata>)[]
            : [new AuthorMetadata(author, null, null)];

        return new BookMetadata(
            Title:        title,
            Authors:      authors,
            Genre:        info.Subject?.Trim(),
            Language:     null,
            SeriesName:   null,
            SeriesNumber: null,
            Annotation:   null,
            Published:    info.CreationDate,
            Isbn:         null,
            CoverBytes:   null,
            CoverMimeType: null);
    }
}