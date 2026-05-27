using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryApp.Application.Authors.Queries.GetAllAuthors;
using LibraryApp.Application.Books.Commands.DeleteBooks;
using LibraryApp.Application.Books.Commands.ToggleRead;
using LibraryApp.Application.Books.Commands.SetRate;
using LibraryApp.Application.Books.Queries.GetBooksByAuthor;
using LibraryApp.Core.Interfaces;
using MediatR;
using Microsoft.Win32;

namespace LibraryApp.UI.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IMediator       _mediator;
    private readonly IImportService  _importService;
    [ObservableProperty]
    private bool? _allBooksChecked = false;

    // ── Authors ──────────────────────────────────────────────────────────
    private readonly ObservableCollection<AuthorListItemVm> _authors = [];

    /// <summary>Filtered view over _authors, bound to AuthorsListView.</summary>
    public ICollectionView AuthorsView { get; }

    [ObservableProperty]
    private string _authorFilter = string.Empty;

    partial void OnAuthorFilterChanged(string value) => AuthorsView.Refresh();

    // ── Books ─────────────────────────────────────────────────────────────
    private readonly ObservableCollection<BookFlatItemVm> _books = [];

    /// <summary>Grouped and sorted view bound to the DataGrid.</summary>
    public ICollectionView BooksView { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BookDetail))]
    private BookFlatItemVm? _selectedBook;

    /// <summary>Incremented on any selection change to trigger group checkbox recompute.</summary>
    [ObservableProperty]
    private int _selectionVersion;

    private bool _isSyncing;

    [ObservableProperty]
    private AuthorListItemVm? _selectedAuthor;

    partial void OnSelectedAuthorChanged(AuthorListItemVm? value) =>
        _ = LoadBooksByAuthorAsync(value);

    partial void OnSelectedBookChanged(BookFlatItemVm? value)
    {
        OnPropertyChanged(nameof(BookDetail));
        SetRateCommand.NotifyCanExecuteChanged();
        DeleteSelectedCommand.NotifyCanExecuteChanged();
        ToggleReadCommand.NotifyCanExecuteChanged();
    }
    private void OnBookSelectionChanged()
    {
        if (_isSyncing) return;
        NotifySelectionChanged();
    }
    private void NotifySelectionChanged()
    {
        UpdateAllBooksChecked();
        SelectionVersion++;
        DeleteSelectedCommand.NotifyCanExecuteChanged();
        ToggleReadCommand.NotifyCanExecuteChanged();
    }

    // ── Status bar ────────────────────────────────────────────────────────
    [ObservableProperty] private string _statusText   = "Ready";
    [ObservableProperty] private string _itemCountText = "0 books";
    [ObservableProperty] private bool   _isImporting;

    // ── Book detail ───────────────────────────────────────────────────────
    public string BookDetail => SelectedBook is null
        ? "Select a book to see details..."
        : BuildDetail(SelectedBook);

    // ─────────────────────────────────────────────────────────────────────
    public MainWindowViewModel(IMediator mediator, IImportService importService)
    {
        _mediator      = mediator;
        _importService = importService;

        AuthorsView = CollectionViewSource.GetDefaultView(_authors);
        AuthorsView.Filter = o =>
            o is AuthorListItemVm a &&
            (string.IsNullOrWhiteSpace(AuthorFilter) ||
             a.Name.Contains(AuthorFilter, StringComparison.OrdinalIgnoreCase));

        BooksView = CollectionViewSource.GetDefaultView(_books);

        // Sort order must match group order
        BooksView.SortDescriptions.Add(new SortDescription(nameof(BookFlatItemVm.AuthorName), ListSortDirection.Ascending));
        BooksView.SortDescriptions.Add(new SortDescription(nameof(BookFlatItemVm.SeriesDisplayName), ListSortDirection.Ascending));
        BooksView.SortDescriptions.Add(new SortDescription(nameof(BookFlatItemVm.SeriesNumber), ListSortDirection.Ascending));
        BooksView.SortDescriptions.Add(new SortDescription(nameof(BookFlatItemVm.Title), ListSortDirection.Ascending));

        // Outer group → inner group (order matters)
        BooksView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BookFlatItemVm.AuthorName)));
        BooksView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BookFlatItemVm.SeriesDisplayName)));
    }

    // ── Selection helpers ─────────────────────────────────────────────────

    /// <summary>Returns IDs of all checked books across all series groups.</summary>
    private IReadOnlyList<long> GetSelectedBookIds() =>
        _books.Where(b => b.IsSelected).Select(b => b.BookId).ToList();

    private void UpdateAllBooksChecked()
    {
        if (!_books.Any()) { AllBooksChecked = false; return; }
        AllBooksChecked = _books.All(b => b.IsSelected)  ? true
            : _books.All(b => !b.IsSelected)  ? false
            : null;
    }

    // ── Commands ──────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadAuthorsAsync(CancellationToken ct = default)
    {
        var authors = await _mediator.Send(new GetAllAuthorsQuery(), ct);

        _authors.Clear();
        foreach (var a in authors)
            _authors.Add(new AuthorListItemVm(a.AuthorId, a.FullName, a.BookCount));

        int total = authors.Sum(a => a.BookCount);
        ItemCountText = $"{total} books";
        StatusText    = $"Loaded {authors.Count} authors";
    }

    [RelayCommand]
    private async Task OpenLibraryAsync(CancellationToken ct = default)
    {
        var dialog = new OpenFolderDialog { Title = "Select Library Folder" };
        if (dialog.ShowDialog() != true) return;

        IsImporting = true;
        StatusText  = "Importing…";

        try
        {
            var report = await _importService.ImportAsync(
                dialog.FolderName, new ScanOptions(), ct);

            StatusText = $"Done: {report.Added} added, {report.Updated} updated, " +
                         $"{report.Skipped} skipped, {report.Removed} removed" +
                         (report.Failed > 0 ? $", {report.Failed} failed" : string.Empty);

            await LoadAuthorsAsync(ct);
        }
        catch (OperationCanceledException)
        {
            StatusText = "Import cancelled.";
        }
        catch (Exception ex)
        {
            StatusText = $"Import failed: {ex.Message}";
            MessageBox.Show(ex.Message, "Import Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSelectedAsync(BookFlatItemVm? targetBook, CancellationToken ct)
    {
        List<long> ids;

        // If the command was called from the context menu of a specific book
        if (targetBook is not null)
        {
            ids = [targetBook.BookId];
        }
        else // If called in general (for example, with a button when checkboxes are checked)
        {
            ids = GetSelectedBookIds().ToList();
        }

        if (ids.Count == 0) return;

        var confirm = MessageBox.Show(
            $"Delete {ids.Count} book(s)?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        await _mediator.Send(new DeleteBooksCommand(ids), ct);
        StatusText = $"Deleted {ids.Count} book(s).";

        await LoadBooksByAuthorAsync(SelectedAuthor);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task ToggleReadAsync(BookFlatItemVm? targetBook, CancellationToken ct)
    {
        List<BookFlatItemVm> targetBooks;

        if (targetBook is not null)
        {
            targetBooks = [targetBook];
        }
        else
        {
            targetBooks = _books.Where(b => b.IsSelected).ToList();
        }

        if (targetBooks.Count == 0) return;

        var ids = targetBooks.Select(b => b.BookId).ToList();

        // If all the target books are read → make them unread, otherwise → read
        bool markAsRead = !targetBooks.All(b => b.Read);

        if (markAsRead)
            await _mediator.Send(new MarkAsReadCommand(ids), ct);
        else
            await _mediator.Send(new MarkAsUnreadCommand(ids), ct);

        StatusText = $"Marked {ids.Count} book(s) as {(markAsRead ? "read" : "unread")}.";
        await LoadBooksByAuthorAsync(SelectedAuthor);
    }

    /// <summary>
    /// Sets rating for the currently selected book.
    /// </summary>
    /// <param name="rate">Rating value (1-5).</param>
    /// <param name="ct">Cancellation token.</param>
    [RelayCommand(CanExecute = nameof(HasSingleSelection))]
    private async Task SetRateAsync(int rate, CancellationToken ct = default)
    {
        if (SelectedBook is null) return;

        await _mediator.Send(new SetRateCommand(SelectedBook.BookId, rate), ct);
        StatusText = $"Rating set to {rate}.";

        await LoadBooksByAuthorAsync(SelectedAuthor);
    }
    /// <summary>
    /// Overload for XAML bindings where CommandParameter comes as string.
    /// </summary>
    [RelayCommand]
    private async Task SetRateFromStringAsync(string? rateStr, CancellationToken ct = default)
    {
        if (int.TryParse(rateStr, out int rate) && rate is >= 1 and <= 5)
        {
            await SetRateAsync(rate, ct);
        }
        else
        {
            StatusText = $"Invalid rating value: {rateStr}";
        }
    }

    /// <summary>
    /// Clear rating.
    /// </summary>
    [RelayCommand]
    private async Task ClearRateAsync(CancellationToken ct)
    {
        if (SelectedBook is null) return;
        await _mediator.Send(new ClearRateCommand(SelectedBook.BookId), ct);
        StatusText = "Rating cleared.";
        await LoadBooksByAuthorAsync(SelectedAuthor);
    }

    private bool HasSelection() => GetSelectedBookIds().Count > 0 || SelectedBook is not null;
    private bool HasSingleSelection() => SelectedBook is not null;

    /// <summary>Toggles selection for all books in a DataGrid group.</summary>
    [RelayCommand]
    private void ToggleGroup(CollectionViewGroup? group)
    {
        if (group is null) return;

        var books = group.Items.OfType<BookFlatItemVm>().ToList();
        bool check = !books.All(b => b.IsSelected);

        _isSyncing = true;
        foreach (var book in books)
            book.IsSelected = check;
        _isSyncing = false;

        NotifySelectionChanged();
    }

    [RelayCommand]
    private void ToggleAllBooks()
    {
        bool check = AllBooksChecked != true;
        _isSyncing = true;
        foreach (var book in _books)
            book.IsSelected = check;
        _isSyncing = false;
        AllBooksChecked = check;
        NotifySelectionChanged();
    }
    // ── Private helpers ───────────────────────────────────────────────────

    private async Task LoadBooksByAuthorAsync(AuthorListItemVm? author)
    {
        if (author is null)
        {
            _books.Clear();
            SelectedBook    = null;
            AllBooksChecked = false;
            ItemCountText   = "0 books";
            return;
        }

        var dtos     = await _mediator.Send(new GetBooksByAuthorQuery(author.AuthorId));
        var incoming = dtos.ToDictionary(d => d.BookId);
        var existing = _books.ToDictionary(b => b.BookId);

        // Remove books no longer present
        foreach (var id in existing.Keys.Except(incoming.Keys).ToList())
            _books.Remove(existing[id]);

        // Update existing or add new — preserves expander/cursor state
        foreach (var dto in dtos)
        {
            if (existing.TryGetValue(dto.BookId, out var vm))
                vm.UpdateFrom(dto);
            else
                _books.Add(new BookFlatItemVm(dto)
                {
                    AuthorName       = author.Name,
                    SelectionChanged = OnBookSelectionChanged
                });
        }

        UpdateAllBooksChecked();
        ItemCountText = $"{dtos.Count} books";
        StatusText    = $"{dtos.Count} books by {author.Name}";

        OnPropertyChanged(nameof(BookDetail));
    }

    private static string BuildDetail(BookFlatItemVm vm)
    {
        var sb = new StringBuilder();
        sb.AppendLine(vm.Title);
        if (vm.Genre      is not null) sb.AppendLine($"Genre:      {vm.Genre}");
        if (vm.Language   is not null) sb.AppendLine($"Language:   {vm.Language}");
        sb.AppendLine($"Format:     {vm.Format.ToString().ToLower()}");
        sb.AppendLine(vm.Read ? "✓ Read" : "○ Unread");
        if (vm.Rate.HasValue)
            sb.AppendLine($"Rating:     {"★".PadRight(vm.Rate.Value, '★').PadRight(5, '☆')}");
        if (vm.Annotation is not null)
        {
            sb.AppendLine();
            sb.Append(vm.Annotation);
        }
        return sb.ToString().TrimEnd();
    }
}
