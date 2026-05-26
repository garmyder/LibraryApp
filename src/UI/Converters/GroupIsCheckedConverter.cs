using System.Collections;
using System.Globalization;
using System.Windows.Data;
using LibraryApp.UI.ViewModels;

namespace LibraryApp.UI.Converters;

/// <summary>
/// Computes tri-state checkbox value from a group's book selection.
/// Binding[0] = CollectionViewGroup.Items
/// Binding[1] = SelectionVersion (acts as update trigger only)
/// </summary>
public sealed class GroupIsCheckedConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not IEnumerable items) return false;

        var books = items.OfType<BookFlatItemVm>().ToList();
        if (books.Count == 0) return false;

        int selected = books.Count(b => b.IsSelected);
        return selected == 0           ? false
            : selected == books.Count ? true
            : (bool?)null;            // indeterminate
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}