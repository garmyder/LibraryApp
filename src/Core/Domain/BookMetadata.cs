namespace LibraryApp.Core.Domain;

public sealed record BookMetadata(
    string Title,
    IReadOnlyList<AuthorMetadata> Authors,
    string? Genre,
    string? Language,
    string? SeriesName,
    string? SeriesNumber,
    string? Annotation,
    string? Published,
    string? Isbn,
    byte[]? CoverBytes,
    string? CoverMimeType);