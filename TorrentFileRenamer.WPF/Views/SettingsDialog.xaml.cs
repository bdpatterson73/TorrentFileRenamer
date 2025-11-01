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
    }

    protected override void OnContentRendered(EventArgs e)
    {
   base.OnContentRendered(e);

        // Subscribe to DialogResult changes if ViewModel implements it
    if (DataContext is ViewModels.SettingsViewModel viewModel)
        {
  viewModel.PropertyChanged += (s, args) =>
{
       if (args.PropertyName == nameof(viewModel.DialogResult))
   {
      DialogResult = viewModel.DialogResult;
   }
     };
        }
    }
}
