using System.Windows.Input;
using TorrentFileRenamer.WPF.Models;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for export functionality
/// </summary>
public class ExportViewModel : ViewModelBase
{
    private readonly IExportService _exportService;
    private readonly IDialogService _dialogService;
    private ExportOptions _options = ExportOptions.Default;
    private ExportFormat _selectedFormat = ExportFormat.Csv;
    private bool _isExporting;
    private int _exportProgress;
    private string _statusMessage = string.Empty;

    public ExportViewModel(IExportService exportService, IDialogService dialogService)
    {
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
  _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Commands
        ExportCommand = new AsyncRelayCommand(ExecuteExportAsync, () => !IsExporting);
     BrowseOutputPathCommand = new RelayCommand(ExecuteBrowseOutputPath);
    SelectDefaultOptionsCommand = new RelayCommand(_ => Options = ExportOptions.Default);
   SelectMinimalOptionsCommand = new RelayCommand(_ => Options = ExportOptions.Minimal);
   SelectDetailedOptionsCommand = new RelayCommand(_ => Options = ExportOptions.Detailed);
    }

#region Properties

    /// <summary>
    /// Export options
    /// </summary>
    public ExportOptions Options
    {
        get => _options;
        set => SetProperty(ref _options, value);
    }

    /// <summary>
    /// Selected export format
    /// </summary>
    public ExportFormat SelectedFormat
    {
   get => _selectedFormat;
   set
 {
       if (SetProperty(ref _selectedFormat, value))
       {
     Options.Format = value;
         OnPropertyChanged(nameof(FileExtension));
OnPropertyChanged(nameof(FileFilter));
        }
  }
    }

    /// <summary>
    /// Whether export is in progress
    /// </summary>
    public bool IsExporting
    {
    get => _isExporting;
        set
        {
   if (SetProperty(ref _isExporting, value))
  {
    ((AsyncRelayCommand)ExportCommand).RaiseCanExecuteChanged();
  }
        }
    }

  /// <summary>
    /// Export progress (0-100)
    /// </summary>
    public int ExportProgress
    {
     get => _exportProgress;
        set => SetProperty(ref _exportProgress, value);
    }

    /// <summary>
    /// Status message
    /// </summary>
    public string StatusMessage
 {
    get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// File extension for selected format
    /// </summary>
    public string FileExtension => _exportService.GetFileExtension(SelectedFormat);

    /// <summary>
    /// File filter for save dialog
    /// </summary>
    public string FileFilter => _exportService.GetFileFilter(SelectedFormat);

    #endregion

    #region Commands

    public ICommand ExportCommand { get; }
    public ICommand BrowseOutputPathCommand { get; }
    public ICommand SelectDefaultOptionsCommand { get; }
    public ICommand SelectMinimalOptionsCommand { get; }
    public ICommand SelectDetailedOptionsCommand { get; }

    #endregion

    #region Events

    /// <summary>
    /// Raised when export completes successfully
    /// </summary>
    public event EventHandler? ExportCompleted;

    /// <summary>
    /// Raised when export fails
    /// </summary>
    public event EventHandler<string>? ExportFailed;

    #endregion

    #region Command Implementations

    private async Task ExecuteExportAsync()
    {
   // This method will be called from parent ViewModels with actual data
        // Here we just validate and prepare
   
        if (string.IsNullOrWhiteSpace(Options.OutputPath))
        {
    ExecuteBrowseOutputPath(null);
      }
        
        if (string.IsNullOrWhiteSpace(Options.OutputPath))
   {
       StatusMessage = "Export cancelled - no output path selected";
return;
        }

        // Parent ViewModel will handle actual export
      await Task.CompletedTask;
    }

    private void ExecuteBrowseOutputPath(object? parameter)
    {
     var defaultFileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}{FileExtension}";
        var filePath = _dialogService.ShowSaveFileDialog(defaultFileName, FileFilter);
     
        if (!string.IsNullOrWhiteSpace(filePath))
        {
     Options.OutputPath = filePath;
            OnPropertyChanged(nameof(Options));
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Exports movie files
    /// </summary>
    public async Task<bool> ExportMoviesAsync(IEnumerable<MovieFileModel> movies)
    {
        try
        {
     IsExporting = true;
            ExportProgress = 0;
     StatusMessage = "Exporting movies...";

    var progress = new Progress<int>(p =>
   {
          ExportProgress = p;
      StatusMessage = $"Exporting... {p}%";
   });

      var success = await _exportService.ExportMoviesAsync(movies, Options, progress);
   
            if (success)
    {
    StatusMessage = $"Export completed: {Options.OutputPath}";
   ExportCompleted?.Invoke(this, EventArgs.Empty);
     }
       else
            {
        StatusMessage = "Export failed";
           ExportFailed?.Invoke(this, "Export operation failed");
     }
  
  return success;
        }
    catch (Exception ex)
    {
    StatusMessage = $"Export error: {ex.Message}";
     ExportFailed?.Invoke(this, ex.Message);
   return false;
   }
        finally
        {
            IsExporting = false;
          ExportProgress = 0;
        }
    }

    /// <summary>
    /// Exports TV episode files
    /// </summary>
    public async Task<bool> ExportEpisodesAsync(IEnumerable<FileEpisodeModel> episodes)
    {
   try
        {
      IsExporting = true;
       ExportProgress = 0;
    StatusMessage = "Exporting episodes...";

            var progress = new Progress<int>(p =>
        {
         ExportProgress = p;
      StatusMessage = $"Exporting... {p}%";
 });

          var success = await _exportService.ExportEpisodesAsync(episodes, Options, progress);
            
     if (success)
     {
         StatusMessage = $"Export completed: {Options.OutputPath}";
    ExportCompleted?.Invoke(this, EventArgs.Empty);
            }
          else
            {
    StatusMessage = "Export failed";
ExportFailed?.Invoke(this, "Export operation failed");
            }
     
return success;
}
        catch (Exception ex)
        {
StatusMessage = $"Export error: {ex.Message}";
     ExportFailed?.Invoke(this, ex.Message);
      return false;
     }
   finally
  {
  IsExporting = false;
        ExportProgress = 0;
        }
    }

    /// <summary>
    /// Generates summary report for movies
    /// </summary>
    public async Task<string> GenerateMovieSummaryAsync(
        IEnumerable<MovieFileModel> movies,
    FileStatistics statistics)
  {
        return await _exportService.GenerateMovieSummaryAsync(movies, statistics);
    }

    /// <summary>
    /// Generates summary report for episodes
    /// </summary>
    public async Task<string> GenerateEpisodeSummaryAsync(
   IEnumerable<FileEpisodeModel> episodes,
        FileStatistics statistics)
    {
        return await _exportService.GenerateEpisodeSummaryAsync(episodes, statistics);
    }

    /// <summary>
    /// Resets export settings to defaults
    /// </summary>
    public void ResetToDefaults()
    {
     Options = ExportOptions.Default;
        SelectedFormat = ExportFormat.Csv;
  ExportProgress = 0;
        StatusMessage = string.Empty;
    }

    #endregion
}
