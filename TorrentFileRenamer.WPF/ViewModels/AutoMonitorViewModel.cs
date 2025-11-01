using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.Core.Services;
using TorrentFileRenamer.Core.Utilities;
using TorrentFileRenamer.WPF.Models;
using TorrentFileRenamer.WPF.ViewModels.Base;
using TorrentFileRenamer.WPF.Views;
using WpfApplication = System.Windows.Application;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for Auto Monitor tab
/// </summary>
public class AutoMonitorViewModel : ViewModelBase
{
    private readonly FolderMonitorService _folderMonitorService;
    private readonly AppSettings _appSettings;
    
  private string _statusMessage = "Monitoring not started";
    private MonitoringStatus _currentStatus = MonitoringStatus.Stopped;
    private string _watchFolder = string.Empty;
    private string _destinationFolder = string.Empty;
    private string _fileExtensions = "*.mp4;*.mkv;*.avi;*.m4v";
    private int _stabilityDelay = 30;
    private bool _autoStart = false;

    public AutoMonitorViewModel(FolderMonitorService folderMonitorService, AppSettings appSettings)
    {
        _folderMonitorService = folderMonitorService;
        _appSettings = appSettings;
        
   RecentActivity = new ObservableCollection<RecentActivityModel>();
        
     // Commands
        StartCommand = new RelayCommand(StartMonitoring, CanStartMonitoring);
      StopCommand = new RelayCommand(StopMonitoring, CanStopMonitoring);
    ToggleMonitoringCommand = new RelayCommand(ToggleMonitoring);
ConfigureCommand = new RelayCommand(ShowConfigurationDialog);
   ClearLogCommand = new RelayCommand(ClearActivityLog);
   
   // Subscribe to folder monitor events
 SubscribeToEvents();
        
      // Load configuration
        LoadConfiguration();
        
        // Auto-start if configured
  if (_autoStart && CanStartMonitoring())
        {
         WpfApplication.Current.Dispatcher.BeginInvoke(new Action(() =>
 {
       StartMonitoring();
    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }

    #region Properties

    public string StatusMessage
    {
     get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

  public MonitoringStatus CurrentStatus
 {
     get => _currentStatus;
set
      {
      if (SetProperty(ref _currentStatus, value))
   {
    OnPropertyChanged(nameof(IsMonitoring));
           OnPropertyChanged(nameof(StatusText));
   OnPropertyChanged(nameof(StatusColor));
        StartCommand.RaiseCanExecuteChanged();
       StopCommand.RaiseCanExecuteChanged();
        }
        }
    }

    public bool IsMonitoring => _currentStatus == MonitoringStatus.Running;

    public string StatusText => _currentStatus switch
  {
        MonitoringStatus.Stopped => "Stopped",
      MonitoringStatus.Starting => "Starting...",
        MonitoringStatus.Running => "Running",
   MonitoringStatus.Stopping => "Stopping...",
        MonitoringStatus.Error => "Error",
      _ => "Unknown"
    };

    public string StatusColor => _currentStatus switch
  {
        MonitoringStatus.Stopped => "#6C757D",
      MonitoringStatus.Starting => "#FFC107",
        MonitoringStatus.Running => "#28A745",
        MonitoringStatus.Stopping => "#FFC107",
    MonitoringStatus.Error => "#DC3545",
   _ => "#6C757D"
  };

    public string WatchFolder
    {
        get => _watchFolder;
    set => SetProperty(ref _watchFolder, value);
}

  public string DestinationFolder
    {
        get => _destinationFolder;
        set => SetProperty(ref _destinationFolder, value);
    }

    public string FileExtensions
  {
   get => _fileExtensions;
     set => SetProperty(ref _fileExtensions, value);
  }

    public int StabilityDelay
    {
        get => _stabilityDelay;
        set => SetProperty(ref _stabilityDelay, value);
    }

    public bool AutoStart
    {
  get => _autoStart;
     set => SetProperty(ref _autoStart, value);
 }

  public ObservableCollection<RecentActivityModel> RecentActivity { get; }

#endregion

    #region Commands

    public RelayCommand StartCommand { get; }
    public RelayCommand StopCommand { get; }
    public RelayCommand ToggleMonitoringCommand { get; }
    public RelayCommand ConfigureCommand { get; }
    public RelayCommand ClearLogCommand { get; }

    #endregion

    #region Command Methods

    private bool CanStartMonitoring()
    {
        return _currentStatus == MonitoringStatus.Stopped &&
    !string.IsNullOrWhiteSpace(_watchFolder) &&
      !string.IsNullOrWhiteSpace(_destinationFolder);
 }

    private bool CanStopMonitoring()
    {
        return _currentStatus == MonitoringStatus.Running ||
      _currentStatus == MonitoringStatus.Starting;
    }

    private void StartMonitoring()
    {
      try
  {
  CurrentStatus = MonitoringStatus.Starting;
  StatusMessage = "Starting folder monitoring...";

    // Apply configuration to service
  _folderMonitorService.WatchFolder = _watchFolder;
            _folderMonitorService.DestinationFolder = _destinationFolder;
   _folderMonitorService.FileExtensions = ParseFileExtensions(_fileExtensions);
  _folderMonitorService.StabilityDelaySeconds = _stabilityDelay;

            // Start monitoring
     bool success = _folderMonitorService.StartMonitoring();

 if (success)
{
  CurrentStatus = MonitoringStatus.Running;
        StatusMessage = $"Monitoring: {_watchFolder}";
    AddActivityLog("System", "Started", "Folder monitoring started successfully", true);
         }
            else
            {
         CurrentStatus = MonitoringStatus.Error;
       StatusMessage = "Failed to start monitoring";
        AddActivityLog("System", "Error", "Failed to start folder monitoring", false);
    }
        }
        catch (Exception ex)
    {
        CurrentStatus = MonitoringStatus.Error;
    StatusMessage = $"Error: {ex.Message}";
            AddActivityLog("System", "Error", $"Exception starting monitoring: {ex.Message}", false);
        }
    }

    private void StopMonitoring()
    {
   try
        {
            CurrentStatus = MonitoringStatus.Stopping;
     StatusMessage = "Stopping folder monitoring...";

            _folderMonitorService.StopMonitoring();

   CurrentStatus = MonitoringStatus.Stopped;
       StatusMessage = "Monitoring stopped";
        AddActivityLog("System", "Stopped", "Folder monitoring stopped", true);
  }
 catch (Exception ex)
    {
      CurrentStatus = MonitoringStatus.Error;
    StatusMessage = $"Error stopping: {ex.Message}";
            AddActivityLog("System", "Error", $"Exception stopping monitoring: {ex.Message}", false);
  }
    }

    private void ToggleMonitoring()
{
        if (IsMonitoring)
        {
       if (CanStopMonitoring())
    StopMonitoring();
}
        else
        {
  if (CanStartMonitoring())
    StartMonitoring();
     }
    }

    private void ShowConfigurationDialog()
    {
 var dialog = new FolderMonitorConfigDialog
        {
          Owner = WpfApplication.Current.MainWindow
        };

    // Pre-populate with current settings
      dialog.ViewModel.WatchFolder = _watchFolder;
  dialog.ViewModel.DestinationFolder = _destinationFolder;
        dialog.ViewModel.FileExtensions = _fileExtensions;
   dialog.ViewModel.StabilityDelay = _stabilityDelay;
        dialog.ViewModel.AutoStart = _autoStart;

    if (dialog.ShowDialog() == true)
  {
       // Apply new settings
    WatchFolder = dialog.ViewModel.WatchFolder;
  DestinationFolder = dialog.ViewModel.DestinationFolder;
      FileExtensions = dialog.ViewModel.FileExtensions;
    StabilityDelay = dialog.ViewModel.StabilityDelay;
 AutoStart = dialog.ViewModel.AutoStart;

        // Save configuration
    SaveConfiguration();

            StatusMessage = "Configuration updated";
        AddActivityLog("System", "Config", "Configuration updated", true);

      // Update command states
            StartCommand.RaiseCanExecuteChanged();
        }
    }

  private void ClearActivityLog()
    {
     RecentActivity.Clear();
        StatusMessage = "Activity log cleared";
 }

    #endregion

    #region Event Handlers

    private void SubscribeToEvents()
    {
      _folderMonitorService.StatusChanged += OnStatusChanged;
      _folderMonitorService.FileFound += OnFileFound;
      _folderMonitorService.FileProcessed += OnFileProcessed;
      _folderMonitorService.ErrorOccurred += OnErrorOccurred;
      _folderMonitorService.FileProgressChanged += OnFileProgressChanged;
    }

    private void UnsubscribeFromEvents()
    {
        _folderMonitorService.StatusChanged -= OnStatusChanged;
  _folderMonitorService.FileFound -= OnFileFound;
   _folderMonitorService.FileProcessed -= OnFileProcessed;
        _folderMonitorService.ErrorOccurred -= OnErrorOccurred;
        _folderMonitorService.FileProgressChanged -= OnFileProgressChanged;
    }

    private void OnStatusChanged(object? sender, string status)
    {
        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
       StatusMessage = status;
     });
    }

    private void OnFileFound(object? sender, FileFoundEventArgs e)
    {
     WpfApplication.Current.Dispatcher.Invoke(() =>
   {
          string fileName = Path.GetFileName(e.FilePath);
      AddActivityLog(fileName, "Found", $"File detected: {e.EventType}", true);
        });
  }

    private void OnFileProcessed(object? sender, FileProcessedEventArgs e)
    {
WpfApplication.Current.Dispatcher.Invoke(() =>
{
      string fileName = Path.GetFileName(e.FilePath);
       AddActivityLog(fileName, e.Success ? "Completed" : "Failed", e.Message, e.Success);
      });
 }

    private void OnErrorOccurred(object? sender, Exception e)
    {
        WpfApplication.Current.Dispatcher.Invoke(() =>
   {
       CurrentStatus = MonitoringStatus.Error;
        StatusMessage = $"Error: {e.Message}";
            AddActivityLog("System", "Error", e.Message, false);
});
  }

    private void OnFileProgressChanged(object? sender, FileProgressEventArgs e)
    {
        WpfApplication.Current.Dispatcher.Invoke(() =>
        {
  if (!string.IsNullOrEmpty(e.CustomMessage))
  {
     StatusMessage = e.CustomMessage;
 }
   else if (!e.IsComplete)
  {
     StatusMessage = $"Copying {Path.GetFileName(e.SourceFile)}: {e.FormattedProgress}";
      }
        });
    }

    #endregion

    #region Helper Methods

    private void AddActivityLog(string fileName, string status, string message, bool isSuccess)
    {
        var activity = new RecentActivityModel
      {
 Timestamp = DateTime.Now,
     FileName = fileName,
          ActivityType = status,
   Status = isSuccess ? "Success" : "Failed",
       Message = message,
       IsSuccess = isSuccess
    };

   RecentActivity.Insert(0, activity);

        // Limit log entries
        int maxEntries = _appSettings.Monitoring.MaxAutoMonitorLogEntries;
        while (RecentActivity.Count > maxEntries)
        {
   RecentActivity.RemoveAt(RecentActivity.Count - 1);
 }
    }

    private string[] ParseFileExtensions(string extensions)
    {
 if (string.IsNullOrWhiteSpace(extensions))
      return Array.Empty<string>();

        return extensions.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
   .Select(ext => ext.Trim())
      .Where(ext => !string.IsNullOrWhiteSpace(ext))
      .ToArray();
    }

    private void LoadConfiguration()
    {
        try
     {
        var config = _appSettings.Monitoring;
   WatchFolder = config.WatchFolder;
      DestinationFolder = config.DestinationFolder;
     FileExtensions = config.FileExtensions;
   StabilityDelay = config.StabilityDelaySeconds;
  AutoStart = config.AutoStartOnLoad;

   StartCommand.RaiseCanExecuteChanged();
        }
        catch (Exception ex)
   {
    StatusMessage = $"Failed to load configuration: {ex.Message}";
  }
    }

    private void SaveConfiguration()
    {
        try
        {
   var config = _appSettings.Monitoring;
config.WatchFolder = _watchFolder;
  config.DestinationFolder = _destinationFolder;
       config.FileExtensions = _fileExtensions;
      config.StabilityDelaySeconds = _stabilityDelay;
  config.AutoStartOnLoad = _autoStart;

            _appSettings.Save();
  }
        catch (Exception ex)
        {
       StatusMessage = $"Failed to save configuration: {ex.Message}";
  }
    }

    #endregion

  #region Cleanup

    protected override void Dispose(bool disposing)
    {
if (disposing)
        {
 // Stop monitoring if running
if (IsMonitoring)
{
       StopMonitoring();
      }

    // Unsubscribe from events
      UnsubscribeFromEvents();
   }

     base.Dispose(disposing);
    }

    #endregion
}
