using System.Windows;
using LibraryApp.UI.ViewModels;

namespace LibraryApp.UI.Views;

public partial class ImportWindow : Window
{
    public ImportWindow(ImportWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // true → main window reloads the author/book list after successful import
        DialogResult = DataContext is ImportWindowViewModel { Phase: ImportPhase.Done };
        Close();
    }
}
