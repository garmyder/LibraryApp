using LibraryApp.Core.Domain;

namespace LibraryApp.Core.Interfaces;

public sealed record ScannedFile(
    string FilePath,
    string FileHash,
    long FileSize,
    BookMetadata? Metadata,
    string? Error);