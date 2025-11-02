using System.Windows;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for KeyboardShortcutsDialog.xaml
/// </summary>
public partial class KeyboardShortcutsDialog : Window
{
    public KeyboardShortcutsDialog()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}