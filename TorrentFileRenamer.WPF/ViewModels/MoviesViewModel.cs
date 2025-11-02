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
/// ViewModel for Movies tab
/// </summary>
public class MoviesViewModel : ViewModelBase
{
    private readonly IScanningService _scanningService;
    private readonly IFileProcessingService _fileProcessingService;
    private readonly IDialogService _dialogService;
    private readonly IWindowStateService _windowStateService;
    private readonly IExportService _exportService;
    private readonly AppSettings _appSettings;
    private string _statusMessage = "No movies scanned";
    private bool _isProcessing;
    private int _minimumConfidence = 40;
    private bool _isCardViewSelected = true;
    private bool _isCompactViewSelected;
    private bool _isGridViewSelected;
    private string _searchText = string.Empty;
    private ProcessingStatus? _statusFilter;
    private ObservableCollection<MovieFileModel> _allMovies = new();

    public MoviesViewModel(
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

        Movies = new ObservableCollection<MovieFileModel>();

        // Phase 6: Wire up event handlers
        SearchViewModel.SearchChanged += OnSearchChanged;
        FilterViewModel.FiltersApplied += OnFiltersApplied;
        FilterViewModel.FiltersReset += OnFiltersReset;
        StatsViewModel.RefreshRequested += OnStatsRefreshRequested;

        // Initialize commands
        ScanCommand = new AsyncRelayCommand(ScanAsync, () => !IsProcessing);
        ProcessCommand = new AsyncRelayCommand(ProcessAsync, CanProcess);
        RemoveSelectedCommand = new RelayCommand(RemoveSelected, _ => !IsProcessing);
        ClearAllCommand = new RelayCommand(ClearAll, _ => Movies.Count > 0 && !IsProcessing);
        RemoveLowConfidenceCommand = new RelayCommand(RemoveLowConfidence, _ => Movies.Any(m => m.Confidence < MinimumConfidence) && !IsProcessing);
        SelectAllCommand = new RelayCommand(_ => { /* Handled by view */ });
        ViewDetailsCommand = new RelayCommand(ViewDetails);
        OpenFolderCommand = new RelayCommand(OpenFolder);
        RetryCommand = new AsyncRelayCommand<MovieFileModel>(RetryAsync);
        
        // Enhanced context menu commands
        OpenSourceFolderCommand = new RelayCommand(OpenSourceFolder);
        OpenDestinationFolderCommand = new RelayCommand(OpenDestinationFolder);
        CopySourcePathCommand = new RelayCommand(CopySourcePath);
        CopyDestinationPathCommand = new RelayCommand(CopyDestinationPath);
        RemoveAllFailedCommand = new RelayCommand(RemoveAllFailed, _ => Movies.Any(m => m.Status == ProcessingStatus.Failed) && !IsProcessing);
        RemoveAllCompletedCommand = new RelayCommand(RemoveAllCompleted, _ => Movies.Any(m => m.Status == ProcessingStatus.Completed) && !IsProcessing);
        
        // Phase 7: Export command
        ExportCommand = new AsyncRelayCommand(ExecuteExportAsync, () => Movies.Count > 0 && !IsProcessing);
    }

    #region Phase 6: ViewModels

    /// <summary>
    /// Search ViewModel (Phase 6)
    /// </summary>
    public SearchViewModel SearchViewModel { get; }

    /// <summary>
    /// Filter ViewModel (Phase 6)
    /// </summary>
    public FilterViewModel FilterViewModel { get; }

    /// <summary>
    /// Stats ViewModel (Phase 6)
    /// </summary>
    public StatsViewModel StatsViewModel { get; }

    #endregion

    #region Properties

    /// <summary>
    /// Collection of movies (filtered based on search and status filter)
    /// </summary>
    public ObservableCollection<MovieFileModel> Movies { get; }

    /// <summary>
    /// Window state service for column width persistence
    /// </summary>
    public IWindowStateService WindowStateService => _windowStateService;
    
    #endregion
    
    #region Commands

    public ICommand ScanCommand { get; }
    public ICommand ProcessCommand { get; }
    public ICommand RemoveSelectedCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand RemoveLowConfidenceCommand { get; }
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
                ((RelayCommand)RemoveLowConfidenceCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)ExportCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Minimum confidence threshold for filtering
    /// </summary>
    public int MinimumConfidence
    {
        get => _minimumConfidence;
        set
        {
            if (SetProperty(ref _minimumConfidence, value))
            {
                ((RelayCommand)RemoveLowConfidenceCommand).RaiseCanExecuteChanged();
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
    /// Search text for filtering movies
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
    /// Status filter for filtering movies
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
    /// Apply search and status filters to the movies collection
    /// </summary>
  private void ApplyFilters()
 {
   var filtered = _allMovies.AsEnumerable();

        // Phase 6: Apply advanced filters if available
 if (FilterViewModel.HasActiveFilters)
        {
       var criteria = FilterViewModel.GetCurrentCriteria();
            
            // Apply search text
            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
            filtered = filtered.Where(m =>
    (m.MovieName?.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
  (m.FileName?.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
      (m.SourcePath?.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
       }
        
      // Apply confidence range
            if (criteria.MinConfidence > 0 || criteria.MaxConfidence < 100)
          {
    filtered = filtered.Where(m => m.Confidence >= criteria.MinConfidence && m.Confidence <= criteria.MaxConfidence);
         }
            
  // Apply status filters
         if (criteria.SelectedStatuses.Count > 0)
   {
    filtered = filtered.Where(m => criteria.SelectedStatuses.Contains(m.Status));
            }

   // Apply file size filters
       if (criteria.MinFileSize > 0)
     {
    filtered = filtered.Where(m => m.FileSize >= criteria.MinFileSize);
    }
 if (criteria.MaxFileSize < long.MaxValue)
       {
   filtered = filtered.Where(m => m.FileSize <= criteria.MaxFileSize);
}
        }
 else
        {
        // Apply basic search filter
      if (!string.IsNullOrWhiteSpace(SearchText))
         {
        filtered = filtered.Where(m =>
    (m.MovieName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (m.FileName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (m.SourcePath?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
  }

            // Apply status filter
        if (StatusFilter.HasValue)
        {
           filtered = filtered.Where(m => m.Status == StatusFilter.Value);
 }
        }

        // Update the Movies collection
        Movies.Clear();
        foreach (var movie in filtered)
        {
     Movies.Add(movie);
        }
  
        // Phase 6: Update search result count and statistics
     SearchViewModel.UpdateResultCount(Movies.Count);
        StatsViewModel.UpdateMovieStatistics(Movies);
   
        // Refresh command states
        ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
        ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
        ((AsyncRelayCommand)ExportCommand).RaiseCanExecuteChanged();
    }

    #region Phase 6: Event Handlers

    private void OnSearchChanged(object? sender, EventArgs e)
    {
        // Sync search text from SearchViewModel to FilterViewModel
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
        StatsViewModel.UpdateMovieStatistics(_allMovies);
    }

    #endregion

    private async Task ScanAsync()
    {
        // Show scan movies dialog
        var dialog = new ScanMoviesDialog(_appSettings);
        if (WpfApplication.Current.MainWindow != null)
        {
     dialog.Owner = WpfApplication.Current.MainWindow;
        }

        if (dialog.ShowDialog() != true)
        {
   return;
        }

var options = dialog.ViewModel;
     MinimumConfidence = options.MinimumConfidence;

    try
        {
            IsProcessing = true;
            StatusMessage = "Scanning for movies...";

       // Create progress dialog
            var progressDialog = new ProgressDialog();
    if (WpfApplication.Current.MainWindow != null)
   {
    progressDialog.Owner = WpfApplication.Current.MainWindow;
 }
 
      var cts = new CancellationTokenSource();
       progressDialog.Initialize(cts);
     progressDialog.UpdateTitle("Scanning for Movies");

         // Start scanning in background
         var scanTask = Task.Run(async () =>
            {
 var progress = new Progress<ScanProgress>(p =>
          {
        progressDialog.UpdateCurrentFile(p.CurrentFile);
progressDialog.UpdateProgress((int)p.PercentComplete);
         });

            return await _scanningService.ScanForMoviesAsync(
    options.SourcePath,
    options.DestinationPath,
      options.FileExtensionsArray,
    progress,
        cts.Token);
         });

       // Show progress dialog (non-blocking)
            progressDialog.Show();

            // Wait for scan to complete
     var results = await scanTask;

     // Close progress dialog
       progressDialog.Close();

            // Store all movies and update filtered collection
            _allMovies.Clear();
            foreach (var movie in results)
   {
         _allMovies.Add(movie);
}
          
 ApplyFilters();

      var parsedCount = results.Count(m => m.IsParsed);
            var unparsedCount = results.Count - parsedCount;
            var highConfidence = results.Count(m => m.Confidence >= 70);
         var mediumConfidence = results.Count(m => m.Confidence >= 40 && m.Confidence < 70);
            var lowConfidence = results.Count(m => m.Confidence < 40);

            StatusMessage = $"Scanned {results.Count} files: {parsedCount} parsed, {unparsedCount} unparsed | Confidence: {highConfidence} high, {mediumConfidence} medium, {lowConfidence} low";

      if (lowConfidence > 0 && MinimumConfidence > 0)
            {
          var result = await _dialogService.ShowConfirmationAsync(
            "Low Confidence Files",
                 $"Found {lowConfidence} files with low confidence (< {MinimumConfidence}%). Remove them from the list?");

  if (result)
                {
     RemoveLowConfidence(null);
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
            ((RelayCommand)RemoveLowConfidenceCommand).RaiseCanExecuteChanged();
      }
 }

    private async Task ProcessAsync()
    {
     var moviesToProcess = Movies
     .Where(m => m.Status == ProcessingStatus.Pending)
  .ToList();

    if (moviesToProcess.Count == 0)
        {
       await _dialogService.ShowMessageAsync("Process", "No pending movies to process.");
  return;
  }

        var result = await _dialogService.ShowConfirmationAsync(
    "Confirm Process",
     $"Process {moviesToProcess.Count} movies?");

   if (!result)
      {
    return;
 }

      try
     {
        IsProcessing = true;
 StatusMessage = $"Processing {moviesToProcess.Count} files...";

    // Create progress dialog
   var progressDialog = new ProgressDialog();
      if (WpfApplication.Current.MainWindow != null)
 {
     progressDialog.Owner = WpfApplication.Current.MainWindow;
       }
       
    var cts = new CancellationTokenSource();
      progressDialog.Initialize(cts, autoClose: true);
  progressDialog.UpdateTitle("Processing Movies");

 // Start processing in background
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

     var successCount = await _fileProcessingService.ProcessMoviesAsync(
  moviesToProcess,
 fileProgress,
 overallProgress,
cts.Token);
  
         // Mark dialog as complete from within the task
            progressDialog.Complete($"Completed: {successCount} of {moviesToProcess.Count} movies processed");
      
return successCount;
        });

   // Show progress dialog
      progressDialog.ShowDialog();

 // Wait for processing to complete
   var successCount = await processTask;

    var failedCount = moviesToProcess.Count - successCount;
    StatusMessage = $"Processed {successCount} files successfully, {failedCount} failed";
   
          // Update statistics after processing
   ApplyFilters();
   StatsViewModel.UpdateMovieStatistics(_allMovies);

      if (successCount > 0)
    {
       await _dialogService.ShowMessageAsync(
 "Process Complete",
$"Successfully processed {successCount} of {moviesToProcess.Count} files.");
 }

   if (failedCount > 0)
     {
 var failedMovies = moviesToProcess.Where(m => m.Status == ProcessingStatus.Failed).ToList();
       var errorMessages = string.Join("\n", failedMovies.Select(m => $"{m.FileName}: {m.ErrorMessage}"));

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
        return !IsProcessing && Movies.Any(m => m.Status == ProcessingStatus.Pending);
    }

    private void RemoveSelected(object? parameter)
    {
        if (parameter is MovieFileModel movie)
      {
          _allMovies.Remove(movie);
  Movies.Remove(movie);
         StatusMessage = $"{_allMovies.Count} movies in list";
            ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
 ((RelayCommand)RemoveLowConfidenceCommand).RaiseCanExecuteChanged();
        }
    }

    private async void ClearAll(object? parameter)
    {
        var result = await _dialogService.ShowConfirmationAsync(
            "Clear All",
         $"Clear all {_allMovies.Count} movies from the list?");

    if (result)
        {
            _allMovies.Clear();
    Movies.Clear();
StatusMessage = "No movies scanned";
            ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveLowConfidenceCommand).RaiseCanExecuteChanged();
        }
    }

    private void RemoveLowConfidence(object? parameter)
    {
        var lowConfidence = _allMovies.Where(m => m.Confidence < MinimumConfidence).ToList();
    foreach (var movie in lowConfidence)
      {
    _allMovies.Remove(movie);
}

  ApplyFilters();
        StatusMessage = $"Removed {lowConfidence.Count} low confidence movies. {_allMovies.Count} remaining.";
        ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
     ((RelayCommand)RemoveLowConfidenceCommand).RaiseCanExecuteChanged();
    }

    private void ViewDetails(object? parameter)
    {
        if (parameter is MovieFileModel movie)
  {
      // Extract new filename from NewDestDirectory path
          string newFileName = !string.IsNullOrEmpty(movie.CoreMovie.NewDestDirectory) 
  ? Path.GetFileName(movie.CoreMovie.NewDestDirectory) 
          : "N/A";

       var message = $"Movie: {movie.DisplayName}\n" +
     $"Confidence: {movie.Confidence}% ({movie.ConfidenceLevel})\n" +
      $"Status: {movie.StatusText}\n\n" +
     $"Original: {movie.FileName}\n" +
   $"New: {newFileName}\n\n" +
   $"Source: {movie.SourcePath}\n" +
      $"Destination: {movie.DestinationPath}";

          if (!string.IsNullOrEmpty(movie.ErrorMessage))
            {
    message += $"\n\nError: {movie.ErrorMessage}";
         }

        _dialogService.ShowMessageAsync("Movie Details", message);
        }
    }

    private void OpenFolder(object? parameter)
    {
        if (parameter is MovieFileModel movie && !string.IsNullOrEmpty(movie.SourcePath))
        {
            try
       {
          var directory = System.IO.Path.GetDirectoryName(movie.SourcePath);
         if (!string.IsNullOrEmpty(directory) && System.IO.Directory.Exists(directory))
    {
     System.Diagnostics.Process.Start("explorer.exe", directory);
          }
    }
          catch (Exception ex)
         {
_dialogService.ShowErrorAsync("Open Folder", $"Failed to open folder: {ex.Message}");
   }
        }
    }

    private async Task RetryAsync(MovieFileModel? movie)
    {
     if (movie == null || movie.Status != ProcessingStatus.Failed)
     {
     return;
        }

        try
     {
      movie.Status = ProcessingStatus.Processing;
       movie.ErrorMessage = string.Empty;

       // Create progress for single file
  var fileProgress = new Progress<(string fileName, int percentage)>();
        var overallProgress = new Progress<(int current, int total)>();

// Process single movie
         var result = await _fileProcessingService.ProcessMoviesAsync(
   new[] { movie },
         fileProgress,
      overallProgress,
  CancellationToken.None);

      if (result > 0)
   {
     StatusMessage = $"Successfully reprocessed {movie.MovieName}";
  }
        }
      catch (Exception ex)
        {
   movie.Status = ProcessingStatus.Failed;
         movie.ErrorMessage = ex.Message;
       await _dialogService.ShowErrorAsync("Retry Failed", $"Failed to reprocess movie: {ex.Message}");
        }
  }

    private void OpenSourceFolder(object? parameter)
    {
        if (parameter is MovieFileModel movie && !string.IsNullOrEmpty(movie.SourcePath))
        {
            try
   {
                var folderPath = System.IO.Path.GetDirectoryName(movie.SourcePath);
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
        if (parameter is MovieFileModel movie && !string.IsNullOrEmpty(movie.DestinationPath))
   {
        try
          {
   var folderPath = System.IO.Path.GetDirectoryName(movie.DestinationPath);
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
        if (parameter is MovieFileModel movie && !string.IsNullOrEmpty(movie.SourcePath))
   {
       try
   {
    System.Windows.Clipboard.SetText(movie.SourcePath);
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
     if (parameter is MovieFileModel movie && !string.IsNullOrEmpty(movie.DestinationPath))
    {
      try
   {
   System.Windows.Clipboard.SetText(movie.DestinationPath);
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
        var failedMovies = _allMovies.Where(m => m.Status == ProcessingStatus.Failed).ToList();
    
   if (failedMovies.Count == 0)
        {
   await _dialogService.ShowMessageAsync("Remove Failed", "No failed movies to remove.");
        return;
        }

      var result = await _dialogService.ShowConfirmationAsync(
         "Remove All Failed",
      $"Remove all {failedMovies.Count} failed movies from the list?");

   if (result)
     {
      foreach (var movie in failedMovies)
        {
     _allMovies.Remove(movie);
            }

          ApplyFilters();
      StatusMessage = $"Removed {failedMovies.Count} failed movies. {_allMovies.Count} remaining.";
            ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
   ((RelayCommand)RemoveLowConfidenceCommand).RaiseCanExecuteChanged();
       ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
      ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
      }
}

    private async void RemoveAllCompleted(object? parameter)
    {
        var completedMovies = _allMovies.Where(m => m.Status == ProcessingStatus.Completed).ToList();

 if (completedMovies.Count == 0)
        {
   await _dialogService.ShowMessageAsync("Remove Completed", "No completed movies to remove.");
            return;
        }

var result = await _dialogService.ShowConfirmationAsync(
 "Remove All Completed",
            $"Remove all {completedMovies.Count} completed movies from the list?");

        if (result)
        {
            foreach (var movie in completedMovies)
   {
 _allMovies.Remove(movie);
  }

         ApplyFilters();
    StatusMessage = $"removed {completedMovies.Count} completed movies. {_allMovies.Count} remaining.";
            ((RelayCommand)ClearAllCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveLowConfidenceCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveAllFailedCommand).RaiseCanExecuteChanged();
 ((RelayCommand)RemoveAllCompletedCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Phase 7: Export movies to file
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
     StatusMessage = "Exporting movies...";

            // Export movies (export all visible movies in current filter)
            var moviesToExport = Movies.ToList();

    if (moviesToExport.Count == 0)
            {
       await _dialogService.ShowMessageAsync("Export", "No movies to export.");
   return;
      }

            var success = await exportViewModel.ExportMoviesAsync(moviesToExport);

            if (success)
            {
         StatusMessage = $"Successfully exported {moviesToExport.Count} movies to {Path.GetFileName(exportViewModel.Options.OutputPath)}";
      
       var result = await _dialogService.ShowConfirmationAsync(
     "Export Complete",
  $"Successfully exported {moviesToExport.Count} movies.\n\nOpen the export file?");

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
    await _dialogService.ShowErrorAsync("Export Error", "Failed to export movies. Please check the file path and try again.");
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
}
