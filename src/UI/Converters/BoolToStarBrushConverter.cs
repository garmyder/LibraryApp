using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LibraryApp.UI.Converters;

/// <summary>Converts a boolean star state to gold (filled) or light-gray (empty) brush.</summary>
[ValueConversion(typeof(bool), typeof(Brush))]
public sealed class BoolToStarBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Filled = new(Color.FromRgb(255, 193,   7));
    private static readonly SolidColorBrush Empty  = new(Color.FromRgb(220, 220, 220));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Filled : Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}