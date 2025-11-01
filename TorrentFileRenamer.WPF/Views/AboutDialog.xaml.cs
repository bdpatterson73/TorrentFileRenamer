using System.Windows;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for AboutDialog.xaml
/// </summary>
public partial class AboutDialog : Window
{
    public AboutDialog()
{
        InitializeComponent();
 }

private void CloseButton_Click(object sender, RoutedEventArgs e)
  {
        DialogResult = true;
Close();
    }
}
