namespace LibraryApp.Core.Domain;

public enum ImportStatus { Added, Updated, Skipped, Failed }

/// <summary>Carries progress data for a single processed book entry during import.</summary>
public sealed record ImportProgressEvent(
    ImportStatus Status,
    string       FilePath,
    string?      Title,
    string?      Author,
    string?      Error = null);
