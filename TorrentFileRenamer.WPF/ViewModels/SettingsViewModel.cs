using System.Windows.Input;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels.Base;
using System.Text.Json;
using System.IO;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for the settings dialog
/// </summary>
public class SettingsViewModel : ViewModelBase
{
 private readonly AppSettings _settings;
  private readonly IDialogService _dialogService;

    // General Settings
    private string _defaultSourcePath;
    private string _defaultDestinationPath;
    private string _defaultFileExtensions;
    private bool _rememberLastPaths;
    
    // Logging Settings
    private bool _enableLogging;
    private int _logRetentionDays;
    
    // Plex Settings
    private bool _enablePlexValidation;
    private bool _autoFixPlexIssues;
    private bool _promptForPlexIssues;
 private bool _skipPlexIncompatibleInAutoMode;
    
    // Monitoring Settings
    private string _watchFolder;
    private string _destinationFolder;
    private string _fileExtensions;
    private int _stabilityDelaySeconds;
  private bool _autoStartOnLoad;
    private bool _processSubfolders;
    private int _maxAutoMonitorLogEntries;

  public SettingsViewModel(AppSettings settings, IDialogService dialogService)
    {
     _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
 // Load current settings
    LoadSettings();
        
  // Initialize commands
        BrowseSourcePathCommand = new RelayCommand(ExecuteBrowseSourcePath);
BrowseDestinationPathCommand = new RelayCommand(ExecuteBrowseDestinationPath);
        BrowseWatchFolderCommand = new RelayCommand(ExecuteBrowseWatchFolder);
 BrowseMonitorDestinationCommand = new RelayCommand(ExecuteBrowseMonitorDestination);
    SaveCommand = new RelayCommand(ExecuteSave);
        CancelCommand = new RelayCommand(ExecuteCancel);
  ResetToDefaultsCommand = new RelayCommand(ExecuteResetToDefaults);
        
 // New Phase 5 commands
        ExportSettingsCommand = new RelayCommand(ExecuteExportSettings);
      ImportSettingsCommand = new RelayCommand(ExecuteImportSettings);
        ApplyBasicPresetCommand = new RelayCommand(ExecuteApplyBasicPreset);
        ApplyAdvancedPresetCommand = new RelayCommand(ExecuteApplyAdvancedPreset);
        ApplyPlexPresetCommand = new RelayCommand(ExecuteApplyPlexPreset);
    }

    #region Properties

    // General Settings
    public string DefaultSourcePath
    {
get => _defaultSourcePath;
    set => SetProperty(ref _defaultSourcePath, value);
  }

    public string DefaultDestinationPath
    {
   get => _defaultDestinationPath;
        set => SetProperty(ref _defaultDestinationPath, value);
    }

    public string DefaultFileExtensions
    {
    get => _defaultFileExtensions;
        set => SetProperty(ref _defaultFileExtensions, value);
    }

    public bool RememberLastPaths
    {
get => _rememberLastPaths;
set => SetProperty(ref _rememberLastPaths, value);
    }

// Logging Settings
    public bool EnableLogging
    {
   get => _enableLogging;
     set => SetProperty(ref _enableLogging, value);
    }

    public int LogRetentionDays
    {
 get => _logRetentionDays;
        set => SetProperty(ref _logRetentionDays, value);
    }

    // Plex Settings
    public bool EnablePlexValidation
    {
get => _enablePlexValidation;
        set => SetProperty(ref _enablePlexValidation, value);
    }

  public bool AutoFixPlexIssues
    {
        get => _autoFixPlexIssues;
   set => SetProperty(ref _autoFixPlexIssues, value);
    }

    public bool PromptForPlexIssues
    {
    get => _promptForPlexIssues;
   set => SetProperty(ref _promptForPlexIssues, value);
    }

    public bool SkipPlexIncompatibleInAutoMode
    {
    get => _skipPlexIncompatibleInAutoMode;
   set => SetProperty(ref _skipPlexIncompatibleInAutoMode, value);
  }

    // Monitoring Settings
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

    public int StabilityDelaySeconds
    {
      get => _stabilityDelaySeconds;
 set => SetProperty(ref _stabilityDelaySeconds, value);
    }

    public bool AutoStartOnLoad
    {
        get => _autoStartOnLoad;
        set => SetProperty(ref _autoStartOnLoad, value);
 }

    public bool ProcessSubfolders
    {
   get => _processSubfolders;
        set => SetProperty(ref _processSubfolders, value);
    }

  public int MaxAutoMonitorLogEntries
    {
        get => _maxAutoMonitorLogEntries;
        set => SetProperty(ref _maxAutoMonitorLogEntries, value);
    }

    public bool? DialogResult { get; private set; }

    #endregion

    #region Commands

    public ICommand BrowseSourcePathCommand { get; }
    public ICommand BrowseDestinationPathCommand { get; }
    public ICommand BrowseWatchFolderCommand { get; }
    public ICommand BrowseMonitorDestinationCommand { get; }
    public ICommand SaveCommand { get; }
  public ICommand CancelCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }
    
    // New Phase 5 commands
    public ICommand ExportSettingsCommand { get; }
    public ICommand ImportSettingsCommand { get; }
    public ICommand ApplyBasicPresetCommand { get; }
  public ICommand ApplyAdvancedPresetCommand { get; }
public ICommand ApplyPlexPresetCommand { get; }

    #endregion

#region Command Implementations

 private void ExecuteBrowseSourcePath()
    {
        var path = _dialogService.ShowFolderBrowserDialog(DefaultSourcePath);
    if (!string.IsNullOrEmpty(path))
 {
   DefaultSourcePath = path;
  }
 }

    private void ExecuteBrowseDestinationPath()
    {
      var path = _dialogService.ShowFolderBrowserDialog(DefaultDestinationPath);
     if (!string.IsNullOrEmpty(path))
        {
        DefaultDestinationPath = path;
        }
  }

    private void ExecuteBrowseWatchFolder()
    {
  var path = _dialogService.ShowFolderBrowserDialog(WatchFolder);
        if (!string.IsNullOrEmpty(path))
    {
     WatchFolder = path;
  }
    }

    private void ExecuteBrowseMonitorDestination()
    {
    var path = _dialogService.ShowFolderBrowserDialog(DestinationFolder);
  if (!string.IsNullOrEmpty(path))
        {
  DestinationFolder = path;
    }
    }

    private void ExecuteSave()
 {
   // Validate settings
     if (!ValidateSettings())
     return;

        // Save settings to AppSettings object
        SaveSettings();
    
        // Persist to file
        _settings.Save();
    
        DialogResult = true;
    }

 private void ExecuteCancel()
{
        DialogResult = false;
    }

    private void ExecuteResetToDefaults()
    {
     var result = _dialogService.ShowConfirmation(
   "Reset Settings",
  "Are you sure you want to reset all settings to their default values?");
            
     if (result)
 {
 var defaultSettings = new AppSettings();
    _settings.DefaultSourcePath = defaultSettings.DefaultSourcePath;
  _settings.DefaultDestinationPath = defaultSettings.DefaultDestinationPath;
  _settings.DefaultFileExtensions = defaultSettings.DefaultFileExtensions;
        _settings.RememberLastPaths = defaultSettings.RememberLastPaths;
       _settings.EnableLogging = defaultSettings.EnableLogging;
  _settings.LogRetentionDays = defaultSettings.LogRetentionDays;
  _settings.PlexSettings = new PlexSettings();
_settings.Monitoring = new MonitoringSettings();
      
  LoadSettings();
            _dialogService.ShowMessage("Settings Reset", "All settings have been reset to their default values.");
        }
    }

    /// <summary>
    /// Export current settings to a JSON file
    /// </summary>
    private void ExecuteExportSettings()
    {
        try
        {
            var filePath = _dialogService.ShowSaveFileDialog("settings.json", "JSON Files|*.json|All Files|*.*");
 if (string.IsNullOrEmpty(filePath))
                return;

            // Create a settings export object with current values
var exportSettings = new
            {
         DefaultSourcePath,
      DefaultDestinationPath,
                DefaultFileExtensions,
       RememberLastPaths,
            EnableLogging,
            LogRetentionDays,
           PlexSettings = new
    {
        EnablePlexValidation,
         AutoFixPlexIssues,
            PromptForPlexIssues,
      SkipPlexIncompatibleInAutoMode
      },
 Monitoring = new
    {
           WatchFolder,
        DestinationFolder,
 FileExtensions,
         StabilityDelaySeconds,
  AutoStartOnLoad,
 ProcessSubfolders,
        MaxAutoMonitorLogEntries
  },
        ExportedAt = DateTime.Now,
  Version = "2.0"
            };

     var json = JsonSerializer.Serialize(exportSettings, new JsonSerializerOptions 
     { 
        WriteIndented = true 
         });
            
            File.WriteAllText(filePath, json);
     
    _dialogService.ShowMessage("Export Successful", 
            $"Settings have been exported to:\n{filePath}");
        }
        catch (Exception ex)
        {
      _dialogService.ShowMessage("Export Failed", 
                $"Failed to export settings: {ex.Message}");
  }
    }

    /// <summary>
    /// Import settings from a JSON file
    /// </summary>
  private void ExecuteImportSettings()
    {
        try
        {
            var result = _dialogService.ShowConfirmation("Import Settings",
           "Importing will overwrite your current settings. Do you want to continue?");
       
            if (!result)
       return;

            var filePath = _dialogService.ShowFolderBrowserDialog(); // Using folder browser as fallback
            // Note: In a real implementation, you'd want ShowOpenFileDialog
  if (string.IsNullOrEmpty(filePath))
        return;

            // For now, show a message that this feature needs ShowOpenFileDialog
 _dialogService.ShowMessage("Import Settings", 
      "Settings import will be available when file selection dialog is implemented.\n" +
     "Expected file format: JSON with all settings properties.");
          
        // TODO: Implement actual import when ShowOpenFileDialog is available in IDialogService
         /*
    var json = File.ReadAllText(filePath);
            var importedSettings = JsonSerializer.Deserialize<SettingsExport>(json);
            
            // Apply imported settings
       DefaultSourcePath = importedSettings.DefaultSourcePath;
            DefaultDestinationPath = importedSettings.DefaultDestinationPath;
     // ... etc            
  _dialogService.ShowMessage("Import Successful", 
          "Settings have been imported successfully. Click Save to apply them.");
          */
        }
        catch (Exception ex)
        {
       _dialogService.ShowMessage("Import Failed", 
      $"Failed to import settings: {ex.Message}");
  }
    }

    /// <summary>
    /// Apply basic preset configuration
    /// </summary>
    private void ExecuteApplyBasicPreset()
    {
        var result = _dialogService.ShowConfirmation("Apply Basic Preset",
            "This will apply a basic configuration. Continue?");
        
        if (!result)
            return;

    // Basic preset: Simple setup
        DefaultFileExtensions = "*.mp4;*.mkv;*.avi";
        RememberLastPaths = true;
    EnableLogging = true;
      LogRetentionDays = 30;
        EnablePlexValidation = false;
        AutoFixPlexIssues = false;
    PromptForPlexIssues = false;
        SkipPlexIncompatibleInAutoMode = false;
        StabilityDelaySeconds = 10;
        AutoStartOnLoad = false;
  ProcessSubfolders = false;
        MaxAutoMonitorLogEntries = 50;

   _dialogService.ShowMessage("Preset Applied", 
          "Basic configuration has been applied. Review and click Save to confirm.");
    }

    /// <summary>
    /// Apply advanced preset configuration
    /// </summary>
    private void ExecuteApplyAdvancedPreset()
    {
        var result = _dialogService.ShowConfirmation("Apply Advanced Preset",
            "This will apply an advanced configuration with all features enabled. Continue?");
      
        if (!result)
            return;

        // Advanced preset: All features enabled
        DefaultFileExtensions = "*.mp4;*.mkv;*.avi;*.m4v;*.wmv;*.flv";
    RememberLastPaths = true;
        EnableLogging = true;
        LogRetentionDays = 90;
    EnablePlexValidation = true;
        AutoFixPlexIssues = false;
   PromptForPlexIssues = true;
        SkipPlexIncompatibleInAutoMode = false;
StabilityDelaySeconds = 30;
        AutoStartOnLoad = false;
        ProcessSubfolders = true;
        MaxAutoMonitorLogEntries = 100;

        _dialogService.ShowMessage("Preset Applied", 
            "Advanced configuration has been applied. Review and click Save to confirm.");
  }

    /// <summary>
    /// Apply Plex-optimized preset configuration
    /// </summary>
    private void ExecuteApplyPlexPreset()
    {
        var result = _dialogService.ShowConfirmation("Apply Plex-Optimized Preset",
   "This will apply the optimal configuration for Plex Media Server. Continue?");
        
    if (!result)
      return;

        // Plex preset: Optimized for Plex Media Server
     DefaultFileExtensions = "*.mp4;*.mkv;*.avi;*.m4v";
        RememberLastPaths = true;
        EnableLogging = true;
   LogRetentionDays = 60;
        EnablePlexValidation = true;
        AutoFixPlexIssues = true;
        PromptForPlexIssues = true;
        SkipPlexIncompatibleInAutoMode = true;
        StabilityDelaySeconds = 20;
        AutoStartOnLoad = false;
    ProcessSubfolders = true;
      MaxAutoMonitorLogEntries = 75;

  _dialogService.ShowMessage("Preset Applied", 
 "Plex-optimized configuration has been applied. Review and click Save to confirm.");
    }

    #endregion

    #region Private Methods

    private void LoadSettings()
    {
        // General Settings
   _defaultSourcePath = _settings.DefaultSourcePath;
      _defaultDestinationPath = _settings.DefaultDestinationPath;
     _defaultFileExtensions = _settings.DefaultFileExtensions;
      _rememberLastPaths = _settings.RememberLastPaths;
   
  // Logging Settings
    _enableLogging = _settings.EnableLogging;
      _logRetentionDays = _settings.LogRetentionDays;
      
  // Plex Settings
    _enablePlexValidation = _settings.PlexSettings.EnablePlexValidation;
        _autoFixPlexIssues = _settings.PlexSettings.AutoFixPlexIssues;
        _promptForPlexIssues = _settings.PlexSettings.PromptForPlexIssues;
      _skipPlexIncompatibleInAutoMode = _settings.PlexSettings.SkipPlexIncompatibleInAutoMode;
    
      // Monitoring Settings
        _watchFolder = _settings.Monitoring.WatchFolder;
        _destinationFolder = _settings.Monitoring.DestinationFolder;
 _fileExtensions = _settings.Monitoring.FileExtensions;
        _stabilityDelaySeconds = _settings.Monitoring.StabilityDelaySeconds;
      _autoStartOnLoad = _settings.Monitoring.AutoStartOnLoad;
        _processSubfolders = _settings.Monitoring.ProcessSubfolders;
 _maxAutoMonitorLogEntries = _settings.Monitoring.MaxAutoMonitorLogEntries;
  
     OnPropertyChanged(string.Empty); // Notify all properties changed
    }

    private void SaveSettings()
    {
        // General Settings
        _settings.DefaultSourcePath = DefaultSourcePath;
      _settings.DefaultDestinationPath = DefaultDestinationPath;
        _settings.DefaultFileExtensions = DefaultFileExtensions;
    _settings.RememberLastPaths = RememberLastPaths;
  
        // Logging Settings
        _settings.EnableLogging = EnableLogging;
  _settings.LogRetentionDays = LogRetentionDays;
      
      // Plex Settings
 _settings.PlexSettings.EnablePlexValidation = EnablePlexValidation;
        _settings.PlexSettings.AutoFixPlexIssues = AutoFixPlexIssues;
    _settings.PlexSettings.PromptForPlexIssues = PromptForPlexIssues;
        _settings.PlexSettings.SkipPlexIncompatibleInAutoMode = SkipPlexIncompatibleInAutoMode;
   
        // Monitoring Settings
        _settings.Monitoring.WatchFolder = WatchFolder;
    _settings.Monitoring.DestinationFolder = DestinationFolder;
   _settings.Monitoring.FileExtensions = FileExtensions;
 _settings.Monitoring.StabilityDelaySeconds = StabilityDelaySeconds;
_settings.Monitoring.AutoStartOnLoad = AutoStartOnLoad;
 _settings.Monitoring.ProcessSubfolders = ProcessSubfolders;
        _settings.Monitoring.MaxAutoMonitorLogEntries = MaxAutoMonitorLogEntries;
    }

    private bool ValidateSettings()
    {
   // Validate log retention days
    if (LogRetentionDays < 1 || LogRetentionDays > 365)
        {
     _dialogService.ShowMessage("Invalid Setting", "Log retention days must be between 1 and 365.");
            return false;
        }
        
        // Validate stability delay
   if (StabilityDelaySeconds < 5 || StabilityDelaySeconds > 300)
        {
   _dialogService.ShowMessage("Invalid Setting", "Stability delay must be between 5 and 300 seconds.");
      return false;
        }
  
 // Validate max log entries
      if (MaxAutoMonitorLogEntries < 1 || MaxAutoMonitorLogEntries > 100)
        {
            _dialogService.ShowMessage("Invalid Setting", "Max auto-monitor log entries must be between 1 and 100.");
  return false;
        }
 
   return true;
    }

    #endregion
}
