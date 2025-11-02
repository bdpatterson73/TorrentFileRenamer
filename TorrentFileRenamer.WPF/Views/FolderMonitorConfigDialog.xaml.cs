using System.Windows;
using TorrentFileRenamer.WPF.ViewModels;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for FolderMonitorConfigDialog.xaml
/// </summary>
public partial class FolderMonitorConfigDialog : Window
{
    public FolderMonitorConfigDialog()
    {
        InitializeComponent();
        DataContext = new FolderMonitorConfigViewModel();
    }

    public FolderMonitorConfigViewModel ViewModel => (FolderMonitorConfigViewModel)DataContext;

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