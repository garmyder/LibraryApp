// LibraryApp.Core/Domain/Book.cs

using System.IO;

namespace LibraryApp.Core.Domain;

public sealed class Book : ILibraryItem
{
    public long BookId { get; private set; }
    public string Title { get; private set; } = null!;
    public string FilePath { get; private set; } = null!;
    public long AddedDate { get; private set; }
    public bool Read { get; private set; }
    public BookFormat Format { get; private set; }
    public int? Rate { get; private set; }

    public long? SeriesId { get; private set; }
    public string? SeriesNumber { get; private set; }
    public long? GenreId { get; private set; }
    public long? LanguageId { get; private set; }
    public string? Annotation { get; private set; }
    public string? CoverPath { get; private set; }
    public string? Published { get; private set; }

    // Navigation properties
    public Genre? Genre { get; private set; }
    public Language? Language { get; private set; }
    public Series? Series { get; private set; }
    public ICollection<Author> Authors { get; private set; } = [];
    public ICollection<Tag> Tags { get; private set; } = [];
    public string Name => Title;
    public LibraryItemType Type => LibraryItemType.Book;
    public string? FileHash { get; private set; }
    
    /// <summary>Required by EF Core.</summary>
    private Book() { }

    public Book(string title, string filePath, long addedDate, BookFormat format,
        bool read = false, long? seriesId = null, string? seriesNumber = null,
        long? genreId = null, long? languageId = null, string? annotation = null,
        string? coverPath = null, string? published = null, int? rate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (rate is not null and (< 1 or > 5))
            throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be between 1 and 5.");

        Title = title;
        FilePath = filePath;
        AddedDate = addedDate;
        Format = format;
        Read = read;
        SeriesId = seriesId;
        SeriesNumber = seriesNumber;
        GenreId = genreId;
        LanguageId = languageId;
        Annotation = annotation;
        CoverPath = coverPath;
        Published = published;
        Rate = rate;
    }

    /// <summary>Detects the format from the file extension.</summary>
    public static BookFormat DetectFormat(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".fb2"  => BookFormat.Fb2,
            ".epub" => BookFormat.Epub,
            ".pdf"  => BookFormat.Pdf,
            ".mobi" => BookFormat.Mobi,
            var ext => throw new NotSupportedException($"Format '{ext}' is not supported.")
        };

    public void MarkAsRead() => Read = true;
    public void MarkAsUnread() => Read = false;

    public void SetRate(int rate)
    {
        if (rate is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(rate));
        Rate = rate;
    }
    
    public void ClearRate()
    {
        Rate = null;
    }
    
    /// <summary>Updates mutable metadata after a re-scan detects file changes.</summary>
    public void UpdateFromScan(
        string title, string? annotation, string? published, string fileHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title      = title;
        Annotation = annotation;
        Published  = published;
        FileHash   = fileHash;
    }

    /// <summary>Sets the file hash on initial import.</summary>
    public void SetFileHash(string hash) => FileHash = hash;
}