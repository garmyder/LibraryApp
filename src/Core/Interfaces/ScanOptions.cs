namespace LibraryApp.Core.Interfaces;

public sealed record ScanOptions(
    bool Recursive = true,
    bool RemoveMissing = false);