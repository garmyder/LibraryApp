using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces;
using LibraryApp.UI.Models;
using LibraryApp.UI.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace LibraryApp.UI.ViewModels;

public enum ImportPhase { Idle, Counting, Importing, Done, Cancelled, Failed }

public sealed partial class ImportWindowViewModel(
    IBookScanner scanner,
    IImportService importService,
    ILogger<ImportWindowViewModel> logger)
    : ObservableObject
{
    private CancellationTokenSource?               _cts;

    // ── Observable properties ─────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartImportCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelImportCommand))]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(IsCountingPhase))]
    [NotifyPropertyChangedFor(nameof(ShowProgressBar))]
    [NotifyPropertyChangedFor(nameof(IsFinished))]
    private ImportPhase _phase = ImportPhase.Idle;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartImportCommand))]
    private string _folderPath = string.Empty;

    [ObservableProperty] private bool          _recursive     = true;
    [ObservableProperty] private bool          _removeMissing;
    [ObservableProperty] private int           _totalBooks;
    [ObservableProperty] private int           _processedBooks;
    [ObservableProperty] private int           _countedSoFar;
    [ObservableProperty] private string        _statusText    = "Select a folder to start import.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReport))]
    private ImportReport? _report;

    // ── Computed properties (for XAML visibility bindings) ────────────────

    public bool IsBusy          => Phase is ImportPhase.Counting or ImportPhase.Importing;
    public bool IsNotBusy       => !IsBusy;
    public bool IsCountingPhase => Phase == ImportPhase.Counting;
    public bool ShowProgressBar => Phase is ImportPhase.Importing or ImportPhase.Done
                                         or ImportPhase.Cancelled or ImportPhase.Failed;
    public bool IsFinished      => Phase is ImportPhase.Done or ImportPhase.Cancelled or ImportPhase.Failed;
    public bool HasReport       => Report is not null;

    /// <summary>Segments added one by one during import — bound to <see cref="SegmentedProgressBar"/>.</summary>
    public ObservableCollection<ImportSegment> Segments { get; } = [];

    // ── Constructor ───────────────────────────────────────────────────────

    // ── Commands ──────────────────────────────────────────────────────────

    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new OpenFolderDialog { Title = "Select Library Folder" };
        if (dialog.ShowDialog() == true)
            FolderPath = dialog.FolderName;
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartImportAsync()
    {
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        Segments.Clear();
        TotalBooks     = 0;
        ProcessedBooks = 0;
        CountedSoFar   = 0;
        Report         = null;

        try
        {
            var options = new ScanOptions(Recursive, RemoveMissing);

            // ── Phase 1: Counting ─────────────────────────────────────────
            Phase      = ImportPhase.Counting;
            StatusText = "Scanning files…";

            var countProgress = new Progress<int>(n =>
            {
                CountedSoFar = n;
                StatusText   = $"Scanning… {n} files found";
            });

            TotalBooks = await scanner.CountAsync(FolderPath, options, countProgress, ct);

            if (TotalBooks == 0)
            {
                StatusText = "No supported books found in the selected folder.";
                Phase      = ImportPhase.Done;
                return;
            }

            // ── Phase 2: Importing ────────────────────────────────────────
            Phase      = ImportPhase.Importing;
            StatusText = $"Importing 0 / {TotalBooks}…";

            var importProgress = new Progress<ImportProgressEvent>(evt =>
            {
                ProcessedBooks++;
                StatusText = $"Importing {ProcessedBooks} / {TotalBooks}…";
                Segments.Add(new ImportSegment
                {
                    Status   = evt.Status,
                    Title    = evt.Title,
                    Author   = evt.Author,
                    FilePath = evt.FilePath,
                    Error    = evt.Error
                });
            });

            Report     = await importService.ImportAsync(FolderPath, options, importProgress, ct);
            Phase      = ImportPhase.Done;
            StatusText = BuildSummary(Report);
            logger.LogInformation("Import finished: {Summary}", StatusText);
        }
        catch (OperationCanceledException)
        {
            Phase      = ImportPhase.Cancelled;
            StatusText = "Import cancelled — all changes rolled back.";
            logger.LogWarning("Import cancelled by user.");
        }
        catch (Exception ex)
        {
            Phase      = ImportPhase.Failed;
            StatusText = $"Error: {ex.Message}";
            logger.LogError(ex, "Import failed unexpectedly.");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanStart() => !string.IsNullOrWhiteSpace(FolderPath) && !IsBusy;

    [RelayCommand(CanExecute = nameof(IsBusy))]
    private void CancelImport() => _cts?.Cancel();

    // ── Helpers ───────────────────────────────────────────────────────────

    private static string BuildSummary(ImportReport r)
    {
        var parts = new List<string>
        {
            $"added: {r.Added}",
            $"updated: {r.Updated}",
            $"skipped: {r.Skipped}"
        };
        if (r.Removed > 0) parts.Add($"removed: {r.Removed}");
        if (r.Failed  > 0) parts.Add($"failed: {r.Failed}");
        return $"Done — {string.Join(", ", parts)}.";
    }
}
