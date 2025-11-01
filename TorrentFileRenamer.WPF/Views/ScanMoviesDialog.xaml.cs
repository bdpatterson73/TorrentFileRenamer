using System.Windows;
using TorrentFileRenamer.WPF.ViewModels;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for ScanMoviesDialog.xaml
/// </summary>
public partial class ScanMoviesDialog : Window
{
    public ScanMoviesDialog()
    {
        InitializeComponent();
        ViewModel = new ScanMoviesViewModel();
        DataContext = ViewModel;
    }

    public ScanMoviesViewModel ViewModel { get; }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
  }
}
