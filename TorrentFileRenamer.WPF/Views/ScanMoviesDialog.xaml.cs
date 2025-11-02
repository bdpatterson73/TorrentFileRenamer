using System.Windows;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.WPF.ViewModels;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for ScanMoviesDialog.xaml
/// </summary>
public partial class ScanMoviesDialog : Window
{
    public ScanMoviesDialog(AppSettings appSettings)
    {
        InitializeComponent();
        ViewModel = new ScanMoviesViewModel(appSettings);
        DataContext = ViewModel;
    }

    public ScanMoviesViewModel ViewModel { get; }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // Save the paths before closing
        ViewModel.SavePaths();

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}