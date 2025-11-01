using System.Windows;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for LogViewerDialog.xaml
/// </summary>
public partial class LogViewerDialog : Window
{
    public LogViewerDialog()
    {
     InitializeComponent();
    }

  protected override void OnContentRendered(EventArgs e)
    {
  base.OnContentRendered(e);

      // Subscribe to DialogResult changes if ViewModel implements it
 if (DataContext is ViewModels.LogViewerViewModel viewModel)
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
