using System.Windows;
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
}