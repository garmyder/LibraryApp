using LibraryApp.Core.Domain;

namespace LibraryApp.Core.Interfaces;

public interface IImportService
{
    /// <summary>
    /// Scans a directory and persists new/updated books to the database within a single transaction.
    /// Reports progress for each processed book via <paramref name="progress"/>.
    /// Throws <see cref="OperationCanceledException"/> and rolls back all changes on cancellation.
    /// </summary>
    Task<ImportReport> ImportAsync(
        string                            directoryPath,
        ScanOptions                       options,
        IProgress<ImportProgressEvent>?   progress = null,
        CancellationToken                 ct       = default);
}
