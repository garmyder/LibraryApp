// LibraryApp.Core/Domain/BookFormat.cs
namespace LibraryApp.Core.Domain;

public enum BookFormat { Fb2, Epub, Pdf, Mobi, Azw3, Unknown }

public static class BookFormatExtensions
{
    /// <summary>
    /// Converts the BookFormat enum value to a lowercase string representation.
    /// Throws an exception if the format is undefined or invalid.
    /// </summary>
    public static string ToCode(this BookFormat format)
    {
        // 1. Check if the value is defined in the enum (avoids invalid integer casts like (BookFormat)999)
        // 2. Prevent "Unknown" or default fallback from passing if it's considered an invalid format type
        if (!Enum.IsDefined(typeof(BookFormat), format) || format == BookFormat.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(format), format, $"The book format '{format}' is not supported or defined.");
        }

        // Convert enum name (e.g., "Epub", "Pdf") directly to lowercase string ("epub", "pdf")
        return format.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Safely parses a string format into a BookFormat enum value.
    /// </summary>
    public static BookFormat FromString(string? formatStr)
    {
        if (string.IsNullOrWhiteSpace(formatStr))
            return BookFormat.Unknown;

        // Enum.TryParse automatically matches strings to enum values (case-insensitive)
        if (Enum.TryParse<BookFormat>(formatStr, true, out var result))
        {
            return result;
        }

        return BookFormat.Unknown;
    }
}
