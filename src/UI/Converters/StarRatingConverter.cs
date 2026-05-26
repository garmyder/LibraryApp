using System.Globalization;
using System.Windows.Data;

namespace LibraryApp.UI.Converters;

/// <summary>Converts a nullable int rating (1–5) to a list of boolean star states.</summary>
[ValueConversion(typeof(int?), typeof(IReadOnlyList<bool>))]
public sealed class StarRatingConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int rating = value is int r ? r : 0;
        return Enumerable.Range(1, 5).Select(i => i <= rating).ToList();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}