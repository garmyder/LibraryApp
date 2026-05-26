// LibraryApp.Core/Domain/Series.cs
namespace LibraryApp.Core.Domain;

public sealed class Series : ILibraryItem
{
    public long SeriesId { get; private set; }
    public string SeriesName { get; private set; } = null!;

    // Navigation
    public ICollection<Book> Books { get; private set; } = [];

    public int BooksCount => Books.Count;
    public string Name => SeriesName;
    public LibraryItemType Type => LibraryItemType.Series;

    /// <summary>Required by EF Core.</summary>
    private Series() { }

    public Series(string seriesName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seriesName);
        SeriesName = seriesName;
    }
}