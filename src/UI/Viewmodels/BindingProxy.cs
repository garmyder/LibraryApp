using System.Windows;

namespace LibraryApp.UI.ViewModels;
///summary  A helper class to enable binding to the MainWindowViewModel from XAML when the DataContext is not inherited (e.g. ContextMenu).
public class MainWindowViewModelProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new MainWindowViewModelProxy();

    // We explicitly specify the MainWindowViewModel type instead of object!
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainWindowViewModel),
            typeof(MainWindowViewModelProxy),
            new PropertyMetadata(null));

    public MainWindowViewModel ViewModel
    {
        get => (MainWindowViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
}
