using System.Windows;
using System.Windows.Threading;

namespace TorrentFileRenamer.WPF.Views;

/// <summary>
/// Interaction logic for ProgressDialog.xaml
/// </summary>
public partial class ProgressDialog : Window
{
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _autoClose = false;
    private bool _operationComplete = false;

    public ProgressDialog()
    {
 InitializeComponent();
 }

    /// <summary>
 /// Gets the cancellation token for this operation
 /// </summary>
    public CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

    /// <summary>
    /// Initializes the dialog with a cancellation token source
  /// </summary>
    public void Initialize(CancellationTokenSource cancellationTokenSource, bool autoClose = true)
  {
        _cancellationTokenSource = cancellationTokenSource;
        _autoClose = autoClose;
    }

 /// <summary>
    /// Updates the dialog title
    /// </summary>
public void UpdateTitle(string title)
  {
  Dispatcher.Invoke(() =>
     {
  TitleTextBlock.Text = title;
 });
    }

    /// <summary>
 /// Updates the current file being processed
 /// </summary>
    public void UpdateCurrentFile(string fileName)
    {
     Dispatcher.Invoke(() =>
 {
            CurrentFileTextBlock.Text = fileName;
  });
  }

    /// <summary>
    /// Updates the progress percentage (0-100)
 /// </summary>
    public void UpdateProgress(int percentage)
  {
   Dispatcher.Invoke(() =>
        {
   ProgressBar.Value = percentage;
    ProgressTextBlock.Text = $"{percentage}%";
      });
    }

    /// <summary>
    /// Updates progress with custom text
    /// </summary>
public void UpdateProgress(int percentage, string text)
    {
   Dispatcher.Invoke(() =>
        {
  ProgressBar.Value = percentage;
   ProgressTextBlock.Text = text;
});
    }

    /// <summary>
    /// Updates transfer details (bytes copied and speed)
    /// </summary>
 public void UpdateTransferDetails(string details)
  {
        Dispatcher.Invoke(() =>
        {
      TransferDetailsTextBlock.Text = details;
 });
    }

 /// <summary>
    /// Marks the operation as complete
    /// </summary>
  public void Complete(string message = "Complete!")
    {
      Dispatcher.Invoke(() =>
    {
   ProgressBar.Value = 100;
 ProgressTextBlock.Text = message;
    TransferDetailsTextBlock.Text = "";
  CancelButton.Content = "Close";
         _operationComplete = true;
         
// Auto-close after a brief delay if enabled
 if (_autoClose)
      {
          var timer = new DispatcherTimer
{
           Interval = TimeSpan.FromMilliseconds(800)
    };
   timer.Tick += (s, e) =>
   {
 timer.Stop();
      try
            {
           DialogResult = true;
          Close();
        }
            catch
   {
          // Dialog may already be closed
  }
  };
  timer.Start();
         }
});
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      if (_operationComplete || CancelButton.Content.ToString() == "Close")
 {
  DialogResult = true;
         Close();
     }
        else
    {
    // Cancel the operation
   _cancellationTokenSource?.Cancel();
 CancelButton.IsEnabled = false;
   CancelButton.Content = "Cancelling...";
 }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
   // If still processing and not complete, cancel the operation
   if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested && !_operationComplete)
        {
  _cancellationTokenSource.Cancel();
   }
 base.OnClosing(e);
    }
}
