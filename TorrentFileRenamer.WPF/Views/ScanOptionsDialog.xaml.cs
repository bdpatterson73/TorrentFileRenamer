using System.Windows;
using TorrentFileRenamer.WPF.ViewModels;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for ScanOptionsDialog.xaml
/// </summary>
public partial class ScanOptionsDialog : Window
{
    public ScanOptionsViewModel ViewModel { get; }

    public ScanOptionsDialog()
  {
     InitializeComponent();
        ViewModel = new ScanOptionsViewModel();
        DataContext = ViewModel;
    }

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
