using CommunityToolkit.Mvvm.ComponentModel;

namespace LibraryApp.UI.ViewModels;

public sealed partial class SeriesGroupVm : ObservableObject
{
    public string                    Header { get; }
    public IReadOnlyList<BookTreeItemVm> Books  { get; }

    /// <summary>
    /// Tri-state: true = all selected, false = none, null = indeterminate.
    /// Setting true/false propagates to all books; null is set only programmatically.
    /// </summary>
    [ObservableProperty]
    private bool? _isChecked = false;

    private bool _isSyncing;

    public SeriesGroupVm(string header, IEnumerable<BookTreeItemVm> books)
    {
        Header = header;
        Books  = [.. books];

        foreach (var book in Books)
            book.SelectionChanged = OnBookSelectionChanged;
    }

    // Called when user clicks the series checkbox (never receives null from UI).
    partial void OnIsCheckedChanged(bool? value)
    {
        if (_isSyncing || value is null) return;

        _isSyncing = true;
        foreach (var book in Books)
            book.IsSelected = value.Value;
        _isSyncing = false;
    }

    // Called when any child book changes its IsSelected.
    private void OnBookSelectionChanged()
    {
        if (_isSyncing) return;

        _isSyncing = true;
        int selected = Books.Count(b => b.IsSelected);
        IsChecked = selected == 0           ? false
            : selected == Books.Count  ? true
            : null;
        _isSyncing = false;

        // Notify VM that selection changed
        SelectionChanged?.Invoke();
    }

    /// <summary>Raised when any book's selection changes; wired by the VM.</summary>
    public Action? SelectionChanged { get; set; }
}