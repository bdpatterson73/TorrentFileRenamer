using System.Windows;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.WPF.ViewModels;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for ScanOptionsDialog.xaml
/// </summary>
public partial class ScanOptionsDialog : Window
{
    public ScanOptionsViewModel ViewModel { get; }

    public ScanOptionsDialog(AppSettings appSettings)
  {
     InitializeComponent();
    ViewModel = new ScanOptionsViewModel(appSettings);
  DataContext = ViewModel;
    }

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
