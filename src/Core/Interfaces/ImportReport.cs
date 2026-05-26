namespace LibraryApp.Core.Interfaces;

public sealed record ImportReport(
    int Added,
    int Updated,
    int Skipped,
    int Removed,
    int Failed,
    IReadOnlyList<string> Errors);