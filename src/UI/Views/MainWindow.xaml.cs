using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LibraryApp.UI.ViewModels;

namespace LibraryApp.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel vm)
    {
        InitializeComponent();
        var vm1 = vm;
        DataContext = vm;

        Loaded += async (_, _) => await vm1.LoadAuthorsCommand.ExecuteAsync(null);
    }

    private void MenuItem_Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGridRow row)
        {
            row.IsSelected = true;
            row.Focus();
        }
    }

    private void BooksDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            // Force commands to check the HasSelection() method
            vm.DeleteSelectedCommand.NotifyCanExecuteChanged();
            vm.ToggleReadCommand.NotifyCanExecuteChanged();
        }
    }
}
