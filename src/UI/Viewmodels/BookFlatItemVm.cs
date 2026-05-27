using CommunityToolkit.Mvvm.ComponentModel;
using LibraryApp.Application.Common.DTOs;
using LibraryApp.Core.Domain;

namespace LibraryApp.UI.ViewModels;

public sealed partial class BookFlatItemVm : ObservableObject
{
    // Backing field to store the original enum value
    private readonly BookFormat _formatEnum;

    // Immutable identity
    public long BookId { get; }

    // Computed property for UI binding that automatically returns lowercase string via extension method
    public string Format => _formatEnum.ToCode();

    // Mutable properties — updated in-place on reload
    [ObservableProperty] private string  _title             = string.Empty;
    [ObservableProperty] private bool    _read;
    [ObservableProperty] private int?    _rate;
    [ObservableProperty] private string? _genre;
    [ObservableProperty] private string? _language;
    [ObservableProperty] private string? _annotation;
    [ObservableProperty] private string? _seriesNumber;
    [ObservableProperty] private string  _seriesDisplayName = string.Empty;
    [ObservableProperty] private string  _authorName        = string.Empty;
    [ObservableProperty] private bool    _isSelected;

    internal Action? SelectionChanged { get; set; }
    partial void OnIsSelectedChanged(bool value) => SelectionChanged?.Invoke();

    public BookFlatItemVm(BookDto dto)
    {
        BookId      = dto.BookId;
        _formatEnum = dto.Format; // Store the enum received from DTO
        UpdateFrom(dto);
    }

    /// <summary>Updates mutable properties in-place to preserve grid/expander state.</summary>
    public void UpdateFrom(BookDto dto)
    {
        Title             = dto.Title;
        Read              = dto.Read;
        Rate              = dto.Rate;
        Genre             = dto.Genre;
        Language          = dto.Language;
        Annotation        = dto.Annotation;
        SeriesNumber      = dto.SeriesNumber;
        SeriesDisplayName = dto.SeriesName ?? "[No series]";

        // Note: If the book format can ever change during runtime updates,
        // you should remove 'readonly' from _formatEnum and update it here as well:
        // _formatEnum = dto.Format;
    }
}
