using CommunityToolkit.Mvvm.ComponentModel;
using LibraryApp.Application.Common.DTOs;
using LibraryApp.Core.Domain;

namespace LibraryApp.UI.ViewModels;

public sealed partial class BookFlatItemVm : ObservableObject
{
    public long       BookId            { get; }
    public string     Title             { get; }
    public bool       Read              { get; }
    public int?       Rate              { get; }
    public string?    Genre             { get; }
    public string?    Language          { get; }
    public BookFormat Format            { get; }
    public string?    Annotation        { get; }
    public string?    SeriesNumber      { get; }

    public string AuthorName { get; init; } = string.Empty;

    /// <summary>Used as the DataGrid group key.</summary>
    public string SeriesDisplayName { get; }

    [ObservableProperty]
    private bool _isSelected;

    internal Action? SelectionChanged { get; set; }
    partial void OnIsSelectedChanged(bool value) => SelectionChanged?.Invoke();

    public BookFlatItemVm(BookDto dto)
    {
        BookId            = dto.BookId;
        Title             = dto.Title;
        Read              = dto.Read;
        Rate              = dto.Rate;
        Genre             = dto.Genre;
        Language          = dto.Language;
        Format            = dto.Format;
        Annotation        = dto.Annotation;
        SeriesNumber      = dto.SeriesNumber;
        SeriesDisplayName = dto.SeriesName ?? "[No series]";
    }
}