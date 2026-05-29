using System.Globalization;
using System.Windows.Data;

namespace LibraryApp.UI.Converters;

/// <summary>
/// Converts an enum value to <c>bool</c> for use with RadioButton.IsChecked.
/// ConverterParameter must match the enum member name (e.g. "Recreate", "Update").
/// </summary>
[ValueConversion(typeof(Enum), typeof(bool))]
public sealed class EnumToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value?.ToString() == parameter?.ToString();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is string paramStr)
            return Enum.Parse(targetType, paramStr);

        return Binding.DoNothing;
    }
}
