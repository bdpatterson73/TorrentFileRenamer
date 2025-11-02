using System.Windows;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for ExportDialog.xaml
/// </summary>
public partial class ExportDialog : Window
{
    public ExportDialog()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}