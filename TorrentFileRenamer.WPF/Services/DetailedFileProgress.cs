using TorrentFileRenamer.Core.Utilities;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Enhanced progress reporting that includes ETA and transfer speed information
/// </summary>
public class DetailedFileProgress : IProgress<int>
{
    private readonly Action<string, string>? _updateCallback;
    private FileOperationProgress? _currentFileOperation;

    public DetailedFileProgress(Action<string, string>? updateCallback = null)
    {
        _updateCallback = updateCallback;
    }

    public void AttachFileOperation(FileOperationProgress fileOperation)
    {
        _currentFileOperation = fileOperation;

        if (_currentFileOperation != null)
        {
            _currentFileOperation.ProgressChanged += OnFileProgressChanged;
        }
    }

    public void DetachFileOperation()
    {
        if (_currentFileOperation != null)
        {
            _currentFileOperation.ProgressChanged -= OnFileProgressChanged;
            _currentFileOperation = null;
        }
    }

    private void OnFileProgressChanged(object? sender, FileProgressEventArgs e)
    {
        // Report detailed progress with speed and ETA
        string details = $"{e.FormattedProgress} at {e.FormattedSpeed}";
        string eta = e.FormattedTimeRemaining;

        _updateCallback?.Invoke(details, eta);
    }

    public void Report(int value)
    {
        // Basic progress reporting (for compatibility)
    }
}