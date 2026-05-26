using CommunityToolkit.Mvvm.ComponentModel;
using LibraryApp.Application.Common.DTOs;
using LibraryApp.Core.Domain;

namespace LibraryApp.UI.ViewModels;

public sealed partial class BookTreeItemVm : ObservableObject
{
    public long       BookId      { get; }
    public string     Title       { get; }
    public bool       Read        { get; }
    public int?       Rate        { get; }
    public string?    Genre       { get; }
    public string?    Language    { get; }
    public string?    Annotation  { get; }
    public BookFormat Format      { get; }
    public string     Meta        { get; }

    [ObservableProperty]
    private bool _isSelected;

    /// <summary>Invoked by the parent SeriesGroupVm to track selection changes.</summary>
    internal Action? SelectionChanged { get; set; }

    partial void OnIsSelectedChanged(bool value) => SelectionChanged?.Invoke();

    public BookTreeItemVm(BookDto dto)
    {
        BookId     = dto.BookId;
        Title      = dto.Title;
        Read       = dto.Read;
        Rate       = dto.Rate;
        Genre      = dto.Genre;
        Language   = dto.Language;
        Annotation = dto.Annotation;
        Format     = dto.Format;

        var parts = new List<string>(3);
        if (Genre    is not null) parts.Add(Genre);
        if (Language is not null) parts.Add(Language);
        parts.Add(Format.ToString().ToUpperInvariant());
        Meta = $"  |  {string.Join("  |  ", parts)}";
    }
}