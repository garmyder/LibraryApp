using System.Windows;

namespace LibraryApp.UI.ViewModels;
///summary  A helper class to enable binding to the MainWindowVm from XAML when the DataContext is not inherited (e.g. ContextMenu).
public class MainWindowViewModelProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new MainWindowViewModelProxy();

    // We explicitly specify the MainWindowVm type instead of object!
    public static readonly DependencyProperty VmProperty =
        DependencyProperty.Register(
            nameof(Vm),
            typeof(MainWindowVm),
            typeof(MainWindowViewModelProxy),
            new PropertyMetadata(null));

    public MainWindowVm Vm
    {
        get => (MainWindowVm)GetValue(VmProperty);
        set => SetValue(VmProperty, value);
    }
}
