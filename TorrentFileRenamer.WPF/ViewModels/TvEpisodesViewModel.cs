using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TorrentFileRenamer.WPF.Models;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels.Base;
using TorrentFileRenamer.WPF.Views;
using WpfApplication = System.Windows.Application;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for TV Episodes tab
/// </summary>
public class TvEpisodesViewModel : ViewModelBase
{
  private readonly IScanningService _scanningService;
 private readonly IFileProcessingService _fileProcessingService;
    private readonly IDialogService _dialogService;
    private readonly IWindowStateService _windowStateService;
    private string _statusMessage = "No episodes scanned";
    private bool _isProcessing;
    private bool _isCardViewSelected = true;
    private bool _isCompactViewSelected;
    private bool _isGridViewSelected;
    private string _searchText = string.Empty;
    private ProcessingStatus? _statusFilter;
    private ObservableCollection<FileEpisodeModel> _allEpisodes = new();

    public TvEpisodesViewModel(
   IScanningService scanningService,
  IFileProcessingService fileProcessingService,
        IDialogService dialogService,
      IWindowStateService windowStateService)
    {
        _scanningService = scanningService;
        _fileProcessingService = fileProcessingService;
     _dialogService = dialogService;
        _windowStateService = windowStateService;

        Episodes = new ObservableCollection<FileEpisodeModel>();

        // Initialize commands
        ScanCommand = new AsyncRelayCommand(ScanAsync, () => !IsProcessing);
   ProcessCommand = new AsyncRelayCommand(ProcessAsync, CanProcess);
   RemoveSelectedCommand = new RelayCommand(RemoveSelected, _ => !IsProcessing);
  ClearAllCommand = new RelayCommand(ClearAll, _ => Episodes.Count > 0 && !IsProcessing);
     RemoveUnparsedCommand = new RelayCommand(RemoveUnparsed, _ => Episodes.Any(e => e.Status == ProcessingStatus.Unparsed) && !IsProcessing);
     SelectAllCommand = new RelayCommand(_ => { /* Handled by view */ });
    
 // Card action commands
        ViewDetailsCommand = new RelayCommand(ViewDetails);
     OpenFolderCommand = new RelayCommand(OpenFolder);
        RetryCommand = new AsyncRelayCommand<object>(RetryAsync);
        
        // Enhanced context menu commands
        OpenSourceFolderCommand = new RelayCommand(OpenSourceFolder);
        OpenDestinationFolderCommand = new RelayCommand(OpenDestinationFolder);
        CopySourcePathCommand = new RelayCommand(CopySourcePath);
     CopyDestinationPathCommand = new RelayCommand(CopyDestinationPath);
 RemoveAllFailedCommand = new RelayCommand(RemoveAllFailed, _ => Episodes.Any(e => e.Status == ProcessingStatus.Failed) && !IsProcessing);
        RemoveAllCompletedCommand = new RelayCommand(RemoveAllCompleted, _ => Episodes.Any(e => e.Status == ProcessingStatus.Completed) && !IsProcessing);
    }

  /// <summary>
    /// Collection of episodes (filtered based on search and status filter)
    /// </summary>
    public ObservableCollection<FileEpisodeModel> Episodes { get; }

    /// <summary>
    /// Window state service for column width persistence
    /// </summary>
    public IWindowStateService WindowStateService => _windowStateService;

    /// <summary>
    /// Status message
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Whether an operation is currently in progress
    /// </summary>
    public bool IsProcessing
    {
      get => _isProcessing;
 set
  {
      if (SetProperty(ref _isProcessing, value))
         {
   // Refresh command states
   ((AsyncRelayCommand)ScanCommand).RaiseCanExecuteChanged();
    ((AsyncRelayCommand)ProcessCommand).RaiseCanExecuteChanged();
     ((RelayCommand)RemoveSelectedCommand).RaiseCanExecuteChanged();
          ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
 ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
           ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
              ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
}
  }
    }

    /// <summary>
    /// Whether card view is selected
    /// </summary>
    public bool IsCardViewSelected
    {
        get => _isCardViewSelected;
   set
        {
            if (SetProperty(ref _isCardViewSelected, value) && value)
        {
              IsCompactViewSelected = false;
        IsGridViewSelected = false;
    // TODO: Save view preference
   }
        }
    }

    /// <summary>
    /// Whether compact view is selected
  /// </summary>
    public bool IsCompactViewSelected
    {
        get => _isCompactViewSelected;
        set
        {
            if (SetProperty(ref _isCompactViewSelected, value) && value)
 {
   IsCardViewSelected = false;
      IsGridViewSelected = false;
              // TODO: Save view preference
            }
      }
    }

    /// <summary>
    /// Whether grid view is selected
    /// </summary>
    public bool IsGridViewSelected
    {
      get => _isGridViewSelected;
        set
        {
     if (SetProperty(ref _isGridViewSelected, value) && value)
            {
    IsCardViewSelected = false;
        IsCompactViewSelected = false;
        // TODO: Save view preference
            }
 }
    }

/// <summary>
    /// Search text for filtering episodes
    /// </summary>
    public string SearchText
    {
 get => _searchText;
   set
        {
            if (SetProperty(ref _searchText, value))
       {
       ApplyFilters();
 }
}
    }

    /// <summary>
    /// Status filter for filtering episodes
    /// </summary>
    public ProcessingStatus? StatusFilter
    {
 get => _statusFilter;
        set
        {
        if (SetProperty(ref _statusFilter, value))
            {
      ApplyFilters();
            }
        }
    }

    /// <summary>
    /// Apply search and status filters to the episodes collection
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = _allEpisodes.AsEnumerable();

   // Apply search filter
   if (!string.IsNullOrWhiteSpace(SearchText))
        {
 filtered = filtered.Where(e =>
  (e.ShowName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
      (e.NewFileName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
   (e.SourcePath?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
    }

        // Apply status filter
     if (StatusFilter.HasValue)
        {
       filtered = filtered.Where(e => e.Status == StatusFilter.Value);
}

  // Update the Episodes collection
Episodes.Clear();
     foreach (var episode in filtered)
 {
 Episodes.Add(episode);
 }
        
   // Refresh command states
    ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
      ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Command to scan for episodes
  /// </summary>
public ICommand ScanCommand { get; }

    /// <summary>
    /// Command to process episodes
    /// </summary>
    public ICommand ProcessCommand { get; }

    /// <summary>
    /// Command to remove selected episode
    /// </summary>
    public ICommand RemoveSelectedCommand { get; }

    /// <summary>
    /// Command to clear all episodes
    /// </summary>
    public ICommand ClearAllCommand { get; }

    /// <summary>
    /// Command to remove unparsed episodes
    /// </summary>
    public ICommand RemoveUnparsedCommand { get; }

/// <summary>
    /// Command to select all episodes
    /// </summary>
    public ICommand SelectAllCommand { get; }

    /// <summary>
    /// Command to view episode details
    /// </summary>
    public ICommand ViewDetailsCommand { get; }

    /// <summary>
    /// Command to open folder containing the file
    /// </summary>
    public ICommand OpenFolderCommand { get; }

    /// <summary>
    /// Command to retry a failed episode
    /// </summary>
    public ICommand RetryCommand { get; }

    /// <summary>
    /// Command to open source folder
    /// </summary>
    public ICommand OpenSourceFolderCommand { get; }

    /// <summary>
    /// Command to open destination folder
  /// </summary>
    public ICommand OpenDestinationFolderCommand { get; }

    /// <summary>
    /// Command to copy source path to clipboard
    /// </summary>
    public ICommand CopySourcePathCommand { get; }

  /// <summary>
    /// Command to copy destination path to clipboard
    /// </summary>
    public ICommand CopyDestinationPathCommand { get; }

    /// <summary>
    /// Command to remove all failed episodes
    /// </summary>
    public ICommand RemoveAllFailedCommand { get; }

    /// <summary>
    /// Command to remove all completed episodes
    /// </summary>
    public ICommand RemoveAllCompletedCommand { get; }

    private async Task ScanAsync()
    {
        // Show scan options dialog
        var dialog = new ScanOptionsDialog();
        if (WpfApplication.Current.MainWindow != null)
        {
            dialog.Owner = WpfApplication.Current.MainWindow;
        }

        if (dialog.ShowDialog() != true)
        {
  return;
        }

        var options = dialog.ViewModel;

        try
 {
            IsProcessing = true;
     StatusMessage = "Scanning...";

          // Create progress dialog
      var progressDialog = new ProgressDialog();
   if (WpfApplication.Current.MainWindow != null)
            {
       progressDialog.Owner = WpfApplication.Current.MainWindow;
            }

var cts = new CancellationTokenSource();
       progressDialog.Initialize(cts);
        progressDialog.UpdateTitle("Scanning for TV Episodes");

       // Start scanning in background
            var scanTask = Task.Run(async () =>
            {
     var progress = new Progress<ScanProgress>(p =>
       {
    progressDialog.UpdateCurrentFile(p.CurrentFile);
        progressDialog.UpdateProgress(p.PercentComplete);
     });

 return await _scanningService.ScanForTvEpisodesAsync(
        options.SourcePath,
       options.DestinationPath,
        options.FileExtensionsArray,
   progress,
    cts.Token);
    });

            // Show progress dialog
       progressDialog.ShowDialog();

        // Wait for scan to complete
            var results = await scanTask;

  // Store all episodes and update filtered collection
 _allEpisodes.Clear();
 foreach (var episode in results)
   {
     _allEpisodes.Add(episode);
         }
        
   ApplyFilters();

    var parsedCount = results.Count(e => e.IsParsed);
            var unparsedCount = results.Count - parsedCount;

  StatusMessage = $"Scanned {results.Count} files: {parsedCount} parsed, {unparsedCount} unparsed";

            if (unparsedCount > 0)
            {
      var result = await _dialogService.ShowConfirmationAsync(
         "Unparsed Files",
$"Found {unparsedCount} unparsed files. Remove them from the list?");

                if (result)
     {
     RemoveUnparsed(null);
    }
            }
        }
    catch (OperationCanceledException)
        {
   StatusMessage = "Scan cancelled";
        }
        catch (Exception ex)
        {
 StatusMessage = $"Scan failed: {ex.Message}";
            await _dialogService.ShowErrorAsync("Scan Error", $"Failed to scan directory: {ex.Message}");
        }
        finally
{
 IsProcessing = false;
            ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
        }
    }

    private async Task ProcessAsync()
    {
        var episodesToProcess = Episodes
            .Where(e => e.Status == ProcessingStatus.Pending)
            .ToList();

        if (episodesToProcess.Count == 0)
        {
            await _dialogService.ShowMessageAsync("Process", "No pending episodes to process.");
            return;
        }

        var result = await _dialogService.ShowConfirmationAsync(
            "Confirm Process",
            $"Process {episodesToProcess.Count} episodes?");

        if (!result)
        {
            return;
        }

        try
        {
            IsProcessing = true;
            StatusMessage = $"Processing {episodesToProcess.Count} files...";

            // Create progress dialog
            var progressDialog = new ProgressDialog();
            if (WpfApplication.Current.MainWindow != null)
            {
                progressDialog.Owner = WpfApplication.Current.MainWindow;
            }

            var cts = new CancellationTokenSource();
            progressDialog.Initialize(cts);
            progressDialog.UpdateTitle("Processing TV Episodes");

            // Start processing in background
            var processTask = Task.Run(async () =>
            {
                var fileProgress = new Progress<(string fileName, int percentage)>(p =>
                {
                    progressDialog.UpdateCurrentFile(p.fileName);
                });

                var overallProgress = new Progress<(int current, int total)>(p =>
                {
                    int percentage = (p.current * 100) / p.total;
                    progressDialog.UpdateProgress(percentage, $"{p.current} of {p.total} files ({percentage}%)");
                });

                return await _fileProcessingService.ProcessFilesAsync(
                    episodesToProcess,
                    fileProgress,
                    overallProgress,
                    cts.Token);
            });

            // Show progress dialog
            progressDialog.ShowDialog();

            // Wait for processing to complete
            var successCount = await processTask;

            var failedCount = episodesToProcess.Count - successCount;
            StatusMessage = $"Processed {successCount} files successfully, {failedCount} failed";

            if (successCount > 0)
            {
                await _dialogService.ShowMessageAsync(
                    "Process Complete",
                    $"Successfully processed {successCount} of {episodesToProcess.Count} files.");
            }

            if (failedCount > 0)
            {
                var failedEpisodes = episodesToProcess.Where(e => e.Status == ProcessingStatus.Failed).ToList();
                var errorMessages = string.Join("\n", failedEpisodes.Select(e => $"{e.NewFileName}: {e.ErrorMessage}"));

                await _dialogService.ShowErrorAsync(
                    "Process Errors",
                    $"Failed to process {failedCount} files:\n\n{errorMessages}");
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Processing cancelled";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Processing failed: {ex.Message}";
            await _dialogService.ShowErrorAsync("Process Error", $"Failed to process files: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private bool CanProcess()
    {
        return !IsProcessing && Episodes.Any(e => e.Status == ProcessingStatus.Pending);
    }

    private void RemoveSelected(object? parameter)
    {
 if (parameter is FileEpisodeModel episode)
  {
    _allEpisodes.Remove(episode);
            Episodes.Remove(episode);
     StatusMessage = $"{_allEpisodes.Count} episodes in list";
      ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
  ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
        }
    }

    private async void ClearAll(object? parameter)
    {
        var result = await _dialogService.ShowConfirmationAsync(
         "Clear All",
    $"Clear all {_allEpisodes.Count} episodes from the list?");

        if (result)
        {
            _allEpisodes.Clear();
        Episodes.Clear();
            StatusMessage = "No episodes scanned";
       ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
       ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
     }
    }

    private void RemoveUnparsed(object? parameter)
    {
        var unparsed = _allEpisodes.Where(e => e.Status == ProcessingStatus.Unparsed).ToList();
 foreach (var episode in unparsed)
 {
 _allEpisodes.Remove(episode);
    }

        ApplyFilters();
      StatusMessage = $"Removed {unparsed.Count} unparsed episodes. {_allEpisodes.Count} remaining.";
   ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
   ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
    }

    private void ViewDetails(object? parameter)
    {
        if (parameter is FileEpisodeModel episode)
        {
     // TODO: Implement details dialog
      _dialogService.ShowMessageAsync("View Details", 
       $"Show: {episode.ShowName}\n" +
     $"Season: {episode.SeasonNumber}\n" +
           $"Episode: {episode.EpisodeNumbers}\n" +
          $"New Filename: {episode.NewFileName}\n" +
          $"Source: {episode.SourcePath}\n" +
         $"Destination: {episode.DestinationPath}\n" +
                $"Status: {episode.StatusText}\n" +
         $"Plex: {episode.PlexValidation}");
        }
    }

    private void OpenFolder(object? parameter)
    {
        if (parameter is FileEpisodeModel episode)
        {
 try
            {
        var folderPath = System.IO.Path.GetDirectoryName(episode.SourcePath);
    if (!string.IsNullOrEmpty(folderPath) && System.IO.Directory.Exists(folderPath))
                {
       System.Diagnostics.Process.Start("explorer.exe", folderPath);
     }
            }
   catch (Exception ex)
            {
   _dialogService.ShowErrorAsync("Open Folder Error", $"Failed to open folder: {ex.Message}");
       }
        }
    }

    private async Task RetryAsync(object? parameter)
    {
        if (parameter is FileEpisodeModel episode && episode.Status == ProcessingStatus.Failed)
     {
            try
         {
     IsProcessing = true;
     episode.Status = ProcessingStatus.Pending;
        episode.ErrorMessage = string.Empty;

    var progress = new Progress<(string fileName, int percentage)>(_ => { });
      var overallProgress = new Progress<(int current, int total)>(_ => { });

   await _fileProcessingService.ProcessFilesAsync(
     new List<FileEpisodeModel> { episode },
         progress,
    overallProgress,
 CancellationToken.None);

        if (episode.Status == ProcessingStatus.Completed)
                {
     await _dialogService.ShowMessageAsync("Retry Success", "Episode processed successfully!");
  }
          else
     {
           await _dialogService.ShowErrorAsync("Retry Failed", $"Failed to process episode: {episode.ErrorMessage}");
      }
      }
       catch (Exception ex)
      {
           episode.Status = ProcessingStatus.Failed;
       episode.ErrorMessage = ex.Message;
 await _dialogService.ShowErrorAsync("Retry Error", $"Error during retry: {ex.Message}");
}
       finally
   {
   IsProcessing = false;
        }
   }
    }

    private void OpenSourceFolder(object? parameter)
 {
        if (parameter is FileEpisodeModel episode && !string.IsNullOrEmpty(episode.SourcePath))
        {
            try
          {
          var folderPath = System.IO.Path.GetDirectoryName(episode.SourcePath);
 if (!string.IsNullOrEmpty(folderPath) && System.IO.Directory.Exists(folderPath))
                {
              System.Diagnostics.Process.Start("explorer.exe", folderPath);
     }
            }
      catch (Exception ex)
      {
      _dialogService.ShowErrorAsync("Open Folder Error", $"Failed to open source folder: {ex.Message}");
            }
        }
    }

    private void OpenDestinationFolder(object? parameter)
    {
    if (parameter is FileEpisodeModel episode && !string.IsNullOrEmpty(episode.DestinationPath))
        {
   try
          {
 var folderPath = System.IO.Path.GetDirectoryName(episode.DestinationPath);
                if (!string.IsNullOrEmpty(folderPath))
                {
            // Create directory if it doesn't exist yet
 if (!System.IO.Directory.Exists(folderPath))
          {
        _dialogService.ShowMessageAsync("Folder Not Found", 
        "Destination folder does not exist yet. It will be created during processing.");
return;
            }
   System.Diagnostics.Process.Start("explorer.exe", folderPath);
       }
            }
   catch (Exception ex)
            {
   _dialogService.ShowErrorAsync("Open Folder Error", $"Failed to open destination folder: {ex.Message}");
    }
        }
    }

private void CopySourcePath(object? parameter)
    {
     if (parameter is FileEpisodeModel episode && !string.IsNullOrEmpty(episode.SourcePath))
        {
     try
         {
     System.Windows.Clipboard.SetText(episode.SourcePath);
 StatusMessage = "Source path copied to clipboard";
         }
  catch (Exception ex)
            {
 _dialogService.ShowErrorAsync("Copy Error", $"Failed to copy source path: {ex.Message}");
   }
}
    }

private void CopyDestinationPath(object? parameter)
    {
      if (parameter is FileEpisodeModel episode && !string.IsNullOrEmpty(episode.DestinationPath))
{
          try
    {
      System.Windows.Clipboard.SetText(episode.DestinationPath);
    StatusMessage = "Destination path copied to clipboard";
 }
       catch (Exception ex)
   {
        _dialogService.ShowErrorAsync("Copy Error", $"Failed to copy destination path: {ex.Message}");
      }
        }
    }

    private async void RemoveAllFailed(object? parameter)
    {
        var failedEpisodes = _allEpisodes.Where(e => e.Status == ProcessingStatus.Failed).ToList();
        
        if (failedEpisodes.Count == 0)
        {
            await _dialogService.ShowMessageAsync("Remove Failed", "No failed episodes to remove.");
  return;
        }

        var result = await _dialogService.ShowConfirmationAsync(
 "Remove All Failed",
    $"Remove all {failedEpisodes.Count} failed episodes from the list?");

        if (result)
        {
       foreach (var episode in failedEpisodes)
  {
    _allEpisodes.Remove(episode);
 }

     ApplyFilters();
        StatusMessage = $"Removed {failedEpisodes.Count} failed episodes. {_allEpisodes.Count} remaining.";
            ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
  ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
    ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
       ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
        }
    }

    private async void RemoveAllCompleted(object? parameter)
    {
        var completedEpisodes = _allEpisodes.Where(e => e.Status == ProcessingStatus.Completed).ToList();

        if (completedEpisodes.Count == 0)
        {
   await _dialogService.ShowMessageAsync("Remove Completed", "No completed episodes to remove.");
            return;
        }

        var result = await _dialogService.ShowConfirmationAsync(
   "Remove All Completed",
     $"Remove all {completedEpisodes.Count} completed episodes from the list?");

        if (result)
{
     foreach (var episode in completedEpisodes)
            {
 _allEpisodes.Remove(episode);
  }

    ApplyFilters();
   StatusMessage = $"Removed {completedEpisodes.Count} completed episodes. {_allEpisodes.Count} remaining.";
   ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
        ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
     }
    }
}
