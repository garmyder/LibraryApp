namespace LibraryApp.Core.Interfaces;

public interface IImportService
{
    /// <summary>Scans a directory and persists new/updated books to the database.</summary>
    Task<ImportReport> ImportAsync(
        string directoryPath,
        ScanOptions options,
        CancellationToken ct = default);
}