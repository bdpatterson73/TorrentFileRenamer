using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TorrentFileRenamer.Core.Configuration;
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
    private readonly IExportService _exportService;
    private readonly AppSettings _appSettings;
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
        IWindowStateService windowStateService,
      IExportService exportService,
        SearchViewModel searchViewModel,
        FilterViewModel filterViewModel,
        StatsViewModel statsViewModel,
        AppSettings appSettings)
    {
        _scanningService = scanningService;
   _fileProcessingService = fileProcessingService;
        _dialogService = dialogService;
      _windowStateService = windowStateService;
        _exportService = exportService;
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));

        // Phase 6: Initialize ViewModels
   SearchViewModel = searchViewModel ?? throw new ArgumentNullException(nameof(searchViewModel));
        FilterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
 StatsViewModel = statsViewModel ?? throw new ArgumentNullException(nameof(statsViewModel));

        Episodes = new ObservableCollection<FileEpisodeModel>();

     // Phase 6: Wire up event handlers
        SearchViewModel.SearchChanged += OnSearchChanged;
        FilterViewModel.FiltersApplied += OnFiltersApplied;
        FilterViewModel.FiltersReset += OnFiltersReset;
        StatsViewModel.RefreshRequested += OnStatsRefreshRequested;

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
        
        // Phase 7: Export command
  ExportCommand = new AsyncRelayCommand(ExecuteExportAsync, () => Episodes.Count > 0 && !IsProcessing);
    }

#region Phase 6: ViewModels

    public SearchViewModel SearchViewModel { get; }
    public FilterViewModel FilterViewModel { get; }
    public StatsViewModel StatsViewModel { get; }

    #endregion

    #region Properties

    public ObservableCollection<FileEpisodeModel> Episodes { get; }
    public IWindowStateService WindowStateService => _windowStateService;

    public string StatusMessage
    {
    get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsProcessing
    {
 get => _isProcessing;
        set
        {
  if (SetProperty(ref _isProcessing, value))
   {
  ((AsyncRelayCommand)ScanCommand).RaiseCanExecuteChanged();
   ((AsyncRelayCommand)ProcessCommand).RaiseCanExecuteChanged();
     ((RelayCommand)RemoveSelectedCommand).RaiseCanExecuteChanged();
    ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
     ((RelayCommand)RemoveUnparsedCommand).RaiseCanExecuteChanged();
     ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
    ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
       ((AsyncRelayCommand)ExportCommand).RaiseCanExecuteChanged();
   }
        }
    }

    public bool IsCardViewSelected
    {
        get => _isCardViewSelected;
      set
    {
        if (SetProperty(ref _isCardViewSelected, value) && value)
   {
         IsCompactViewSelected = false;
           IsGridViewSelected = false;
            }
   }
    }

    public bool IsCompactViewSelected
    {
  get => _isCompactViewSelected;
        set
    {
            if (SetProperty(ref _isCompactViewSelected, value) && value)
      {
       IsCardViewSelected = false;
    IsGridViewSelected = false;
            }
        }
    }

    public bool IsGridViewSelected
    {
 get => _isGridViewSelected;
        set
        {
            if (SetProperty(ref _isGridViewSelected, value) && value)
  {
        IsCardViewSelected = false;
              IsCompactViewSelected = false;
  }
}
    }

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

    #endregion

    #region Commands

    public ICommand ScanCommand { get; }
    public ICommand ProcessCommand { get; }
    public ICommand RemoveSelectedCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand RemoveUnparsedCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand ViewDetailsCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand RetryCommand { get; }
    public ICommand OpenSourceFolderCommand { get; }
    public ICommand OpenDestinationFolderCommand { get; }
    public ICommand CopySourcePathCommand { get; }
  public ICommand CopyDestinationPathCommand { get; }
    public ICommand RemoveAllFailedCommand { get; }
    public ICommand RemoveAllCompletedCommand { get; }
    public ICommand ExportCommand { get; }

    #endregion

    #region Filtering

    private void ApplyFilters()
    {
        var filtered = _allEpisodes.AsEnumerable();

  // Phase 6: Apply advanced filters if available
     if (FilterViewModel.HasActiveFilters)
        {
       var criteria = FilterViewModel.GetCurrentCriteria();
  
      if (!string.IsNullOrWhiteSpace(criteria.SearchText))
      {
 filtered = filtered.Where(e =>
         (e.ShowName?.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
              (e.NewFileName?.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
    (e.SourcePath?.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }
      
            if (criteria.SelectedStatuses.Count > 0)
       {
      filtered = filtered.Where(e => criteria.SelectedStatuses.Contains(e.Status));
          }
        }
      else
  {
    if (!string.IsNullOrWhiteSpace(SearchText))
         {
 filtered = filtered.Where(e =>
  (e.ShowName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
    (e.NewFileName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
          (e.SourcePath?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
       }

     if (StatusFilter.HasValue)
  {
           filtered = filtered.Where(e => e.Status == StatusFilter.Value);
        }
        }

    Episodes.Clear();
        foreach (var episode in filtered)
        {
            Episodes.Add(episode);
   }
    
        // Phase 6: Update search result count and statistics
    SearchViewModel.UpdateResultCount(Episodes.Count);
   StatsViewModel.UpdateEpisodeStatistics(Episodes);
   
   ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
        ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
     ((AsyncRelayCommand)ExportCommand).RaiseCanExecuteChanged();
    }

    #endregion

    #region Phase 6: Event Handlers

    private void OnSearchChanged(object? sender, EventArgs e)
    {
        FilterViewModel.ApplySearchText(SearchViewModel.SearchText);
        ApplyFilters();
    }

    private void OnFiltersApplied(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void OnFiltersReset(object? sender, EventArgs e)
    {
     SearchText = string.Empty;
        StatusFilter = null;
        ApplyFilters();
    }

    private void OnStatsRefreshRequested(object? sender, EventArgs e)
    {
        StatsViewModel.UpdateEpisodeStatistics(_allEpisodes);
    }

    #endregion

    #region Command Implementations

    private async Task ScanAsync()
    {
        var dialog = new ScanOptionsDialog(_appSettings);
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

            var progressDialog = new ProgressDialog();
       if (WpfApplication.Current.MainWindow != null)
            {
           progressDialog.Owner = WpfApplication.Current.MainWindow;
 }

            var cts = new CancellationTokenSource();
            progressDialog.Initialize(cts);
 progressDialog.UpdateTitle("Scanning for TV Episodes");

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

    progressDialog.ShowDialog();

            var results = await scanTask;

          _allEpisodes.Clear();
         foreach (var episode in results)
   {
                _allEpisodes.Add(episode);
        }
   
          ApplyFilters();

            // Phase 6: Update available extensions for filter
    var extensions = _allEpisodes
   .Select(e => Path.GetExtension(e.SourcePath ?? "").ToLowerInvariant())
           .Where(ext => !string.IsNullOrWhiteSpace(ext))
      .Distinct()
           .ToList();
            FilterViewModel.UpdateAvailableExtensions(extensions);
          
            // Update statistics
     StatsViewModel.UpdateEpisodeStatistics(_allEpisodes);

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

   var progressDialog = new ProgressDialog();
 if (WpfApplication.Current.MainWindow != null)
  {
      progressDialog.Owner = WpfApplication.Current.MainWindow;
    }

var cts = new CancellationTokenSource();
      progressDialog.Initialize(cts, autoClose: true);
 progressDialog.UpdateTitle("Processing TV Episodes");

     var processTask = Task.Run(async () =>
     {
      int currentFileIndex = 0;

      var fileProgress = new Progress<(string fileName, int percentage)>(p =>
         {
  progressDialog.UpdateCurrentFile(p.fileName);
   progressDialog.UpdateProgress(p.percentage);
 });

  var overallProgress = new Progress<(int current, int total)>(p =>
 {
     currentFileIndex = p.current;
  int percentage = (p.current * 100) / p.total;
         progressDialog.UpdateProgress(percentage, $"File {p.current} of {p.total} ({percentage}%)");
  });

         var successCount = await _fileProcessingService.ProcessFilesAsync(
     episodesToProcess,
      fileProgress,
    overallProgress,
      cts.Token);
         
            // Mark dialog as complete from within the task
          progressDialog.Complete($"Completed: {successCount} of {episodesToProcess.Count} files processed");
      
            return successCount;
     });

            progressDialog.ShowDialog();

     var successCount = await processTask;

      var failedCount = episodesToProcess.Count - successCount;
     StatusMessage = $"Processed {successCount} files successfully, {failedCount} failed";
         
          // Update statistics after processing
   ApplyFilters();
StatsViewModel.UpdateEpisodeStatistics(_allEpisodes);

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

    /// <summary>
  /// Phase 7: Export episodes to file
    /// </summary>
private async Task ExecuteExportAsync()
    {
        try
 {
            // Create export view model
      var exportViewModel = new ExportViewModel(_exportService, _dialogService);
         
         // Show export dialog
   var dialog = new Views.ExportDialog
     {
     Owner = System.Windows.Application.Current.MainWindow,
        DataContext = exportViewModel
       };

            if (dialog.ShowDialog() != true)
         {
     StatusMessage = "Export cancelled";
     return;
            }

 // Check if output path was selected
    if (string.IsNullOrWhiteSpace(exportViewModel.Options.OutputPath))
     {
          StatusMessage = "Export cancelled - no output path selected";
         return;
   }

  IsProcessing = true;
  StatusMessage = "Exporting episodes...";

         // Export episodes (export all visible episodes in current filter)
            var episodesToExport = Episodes.ToList();

   if (episodesToExport.Count == 0)
      {
          await _dialogService.ShowMessageAsync("Export", "No episodes to export.");
            return;
   }

            var success = await exportViewModel.ExportEpisodesAsync(episodesToExport);

            if (success)
   {
    StatusMessage = $"Successfully exported {episodesToExport.Count} episodes to {Path.GetFileName(exportViewModel.Options.OutputPath)}";
       
       var result = await _dialogService.ShowConfirmationAsync(
        "Export Complete",
      $"Successfully exported {episodesToExport.Count} episodes.\n\nOpen the export file?");

  if (result)
      {
         try
    {
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
  {
        FileName = exportViewModel.Options.OutputPath,
   UseShellExecute = true
          });
      }
        catch (Exception ex)
      {
   await _dialogService.ShowErrorAsync("Open File Error", $"Failed to open export file: {ex.Message}");
  }
    }
  }
            else
      {
     StatusMessage = "Export failed";
      await _dialogService.ShowErrorAsync("Export Error", "Failed to export episodes. Please check the file path and try again.");
  }
        }
        catch (Exception ex)
        {
     StatusMessage = $"Export error: {ex.Message}";
            await _dialogService.ShowErrorAsync("Export Error", $"An error occurred during export:\n\n{ex.Message}");
        }
        finally
        {
       IsProcessing = false;
((AsyncRelayCommand)ExportCommand).RaiseCanExecuteChanged();
        }
    }

    #endregion
}
