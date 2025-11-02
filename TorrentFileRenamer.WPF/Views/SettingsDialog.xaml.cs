using System.Windows;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for SettingsDialog.xaml
/// </summary>
public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();
        Loaded += SettingsDialog_Loaded;
    }

    private void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
    {
        // Subscribe to DialogResult changes if ViewModel implements it
        if (DataContext is ViewModels.SettingsViewModel viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is ViewModels.SettingsViewModel viewModel &&
            e.PropertyName == nameof(viewModel.DialogResult))
        {
            if (viewModel.DialogResult.HasValue)
            {
                DialogResult = viewModel.DialogResult;
                Close();
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Unsubscribe from events
        if (DataContext is ViewModels.SettingsViewModel viewModel)
        {
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        base.OnClosed(e);
    }
}
