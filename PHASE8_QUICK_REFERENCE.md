# Phase 8 Quick Reference - Auto-Monitor & Background Processing

## ?? Quick Start

### Set Up Auto-Monitoring in 3 Steps
1. **Navigate:** Click "Auto Monitor" tab or press `Ctrl+3`
2. **Configure:** Click "Configure" ? Set watch & destination folders ? Click OK
3. **Start:** Click "Start Monitoring"

Done! Files will be processed automatically.

---

## ?? Configuration

### Open Configuration Dialog
```csharp
// In AutoMonitorViewModel
ConfigureCommand = new RelayCommand(ShowConfigurationDialog);

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
        
      SaveConfiguration();
    }
}
```

### Configuration Options

| Setting | Description | Default | Range |
|---------|-------------|---------|-------|
| **Watch Folder** | Folder to monitor for new files | (empty) | Any valid folder path |
| **Destination Folder** | Where processed files go | (empty) | Any valid folder path |
| **File Extensions** | Video file types to monitor | `*.mp4;*.mkv;*.avi;*.m4v` | Semicolon-separated list |
| **Stability Delay** | Wait time before processing (seconds) | 30 | 10-300 |
| **Auto-start on Load** | Start monitoring when app launches | false | true/false |

---

## ?? Using FolderMonitorService

### Basic Usage

```csharp
// 1. Create service instance (usually via DI)
var service = new FolderMonitorService();

// 2. Configure
service.WatchFolder = @"C:\Downloads\TV Shows";
service.DestinationFolder = @"D:\Plex\TV Shows";
service.FileExtensions = new[] { "*.mkv", "*.mp4", "*.avi" };
service.StabilityDelaySeconds = 30;

// 3. Subscribe to events
service.StatusChanged += OnStatusChanged;
service.FileFound += OnFileFound;
service.FileProcessed += OnFileProcessed;
service.ErrorOccurred += OnErrorOccurred;
service.FileProgressChanged += OnFileProgressChanged;

// 4. Start monitoring
bool success = service.StartMonitoring();

// 5. Stop when done
service.StopMonitoring();

// 6. Cleanup
service.Dispose();
```

### Event Handlers

```csharp
private void OnStatusChanged(object? sender, string status)
{
    // Update UI with status message
    StatusMessage = status;
}

private void OnFileFound(object? sender, FileFoundEventArgs e)
{
    // File detected in watch folder
    string fileName = Path.GetFileName(e.FilePath);
    string message = $"File detected: {e.EventType}";
    AddActivityLog(fileName, "Found", message, true);
}

private void OnFileProcessed(object? sender, FileProcessedEventArgs e)
{
    // File processing completed (success or failure)
    string fileName = Path.GetFileName(e.FilePath);
    string status = e.Success ? "Completed" : "Failed";
    AddActivityLog(fileName, status, e.Message, e.Success);
}

private void OnErrorOccurred(object? sender, Exception e)
{
    // Error during monitoring or processing
    StatusMessage = $"Error: {e.Message}";
    AddActivityLog("System", "Error", e.Message, false);
}

private void OnFileProgressChanged(object? sender, FileProgressEventArgs e)
{
    // File copy progress updates
    if (!e.IsComplete)
    {
        StatusMessage = $"Copying {Path.GetFileName(e.SourceFile)}: {e.FormattedProgress}";
    }
}
```

### Cross-thread UI Updates

```csharp
private void OnFileProcessed(object? sender, FileProcessedEventArgs e)
{
    // Ensure UI updates happen on UI thread
    WpfApplication.Current.Dispatcher.Invoke(() =>
    {
        string fileName = Path.GetFileName(e.FilePath);
        AddActivityLog(fileName, e.Success ? "Completed" : "Failed", e.Message, e.Success);
    });
}
```

---

## ?? AutoMonitorViewModel Usage

### Initialization

```csharp
public class AutoMonitorViewModel : ViewModelBase
{
    private readonly FolderMonitorService _folderMonitorService;
  private readonly AppSettings _appSettings;
    
    public AutoMonitorViewModel(
        FolderMonitorService folderMonitorService, 
        AppSettings appSettings)
    {
        _folderMonitorService = folderMonitorService;
        _appSettings = appSettings;
        
        // Initialize collections
        RecentActivity = new ObservableCollection<RecentActivityModel>();
      
     // Initialize commands
        StartCommand = new RelayCommand(StartMonitoring, CanStartMonitoring);
   StopCommand = new RelayCommand(StopMonitoring, CanStopMonitoring);
        ToggleMonitoringCommand = new RelayCommand(ToggleMonitoring);
        ConfigureCommand = new RelayCommand(ShowConfigurationDialog);
        ClearLogCommand = new RelayCommand(ClearActivityLog);
        
        // Subscribe to events
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
}
```

### Properties

```csharp
// Status tracking
public string StatusMessage { get; set; } = "Monitoring not started";
public MonitoringStatus CurrentStatus { get; set; } = MonitoringStatus.Stopped;
public bool IsMonitoring => CurrentStatus == MonitoringStatus.Running;

// Status display
public string StatusText => CurrentStatus switch
{
    MonitoringStatus.Stopped => "Stopped",
    MonitoringStatus.Starting => "Starting...",
    MonitoringStatus.Running => "Running",
    MonitoringStatus.Stopping => "Stopping...",
    MonitoringStatus.Error => "Error",
    _ => "Unknown"
};

public string StatusColor => CurrentStatus switch
{
    MonitoringStatus.Stopped => "#6C757D",
    MonitoringStatus.Starting => "#FFC107",
    MonitoringStatus.Running => "#28A745",
    MonitoringStatus.Stopping => "#FFC107",
    MonitoringStatus.Error => "#DC3545",
    _ => "#6C757D"
};

// Configuration
public string WatchFolder { get; set; } = string.Empty;
public string DestinationFolder { get; set; } = string.Empty;
public string FileExtensions { get; set; } = "*.mp4;*.mkv;*.avi;*.m4v";
public int StabilityDelay { get; set; } = 30;
public bool AutoStart { get; set; } = false;

// Activity log
public ObservableCollection<RecentActivityModel> RecentActivity { get; }
```

### Commands

```csharp
private bool CanStartMonitoring()
{
    return CurrentStatus == MonitoringStatus.Stopped &&
           !string.IsNullOrWhiteSpace(WatchFolder) &&
    !string.IsNullOrWhiteSpace(DestinationFolder);
}

private void StartMonitoring()
{
    try
    {
        CurrentStatus = MonitoringStatus.Starting;
  StatusMessage = "Starting folder monitoring...";
        
        // Apply configuration to service
        _folderMonitorService.WatchFolder = WatchFolder;
        _folderMonitorService.DestinationFolder = DestinationFolder;
        _folderMonitorService.FileExtensions = ParseFileExtensions(FileExtensions);
 _folderMonitorService.StabilityDelaySeconds = StabilityDelay;
 
        // Start monitoring
        bool success = _folderMonitorService.StartMonitoring();
        
        if (success)
    {
          CurrentStatus = MonitoringStatus.Running;
    StatusMessage = $"Monitoring: {WatchFolder}";
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
```

### Activity Logging

```csharp
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

private void ClearActivityLog()
{
    RecentActivity.Clear();
    StatusMessage = "Activity log cleared";
}
```

### Configuration Persistence

```csharp
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
 config.WatchFolder = WatchFolder;
        config.DestinationFolder = DestinationFolder;
     config.FileExtensions = FileExtensions;
        config.StabilityDelaySeconds = StabilityDelay;
        config.AutoStartOnLoad = AutoStart;
        
     _appSettings.Save();
    }
    catch (Exception ex)
  {
        StatusMessage = $"Failed to save configuration: {ex.Message}";
    }
}
```

### Cleanup

```csharp
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

private void UnsubscribeFromEvents()
{
 _folderMonitorService.StatusChanged -= OnStatusChanged;
    _folderMonitorService.FileFound -= OnFileFound;
    _folderMonitorService.FileProcessed -= OnFileProcessed;
    _folderMonitorService.ErrorOccurred -= OnErrorOccurred;
    _folderMonitorService.FileProgressChanged -= OnFileProgressChanged;
}
```

---

## ?? XAML Integration

### AutoMonitorView.xaml

```xaml
<UserControl x:Class="TorrentFileRenamer.WPF.Views.AutoMonitorView"
             xmlns:vm="clr-namespace:TorrentFileRenamer.WPF.ViewModels"
             d:DataContext="{d:DesignInstance Type=vm:AutoMonitorViewModel}">
    
    <!-- Toolbar -->
    <ToolBar Style="{StaticResource ModernToolBar}">
  <Button Content="Configure" 
                Command="{Binding ConfigureCommand}"
             Style="{StaticResource PrimaryButton}"/>
    <Separator/>
        <Button Content="Start Monitoring" 
       Command="{Binding StartCommand}"
      Style="{StaticResource PrimaryButton}"/>
        <Button Content="Stop Monitoring" 
    Command="{Binding StopCommand}"
         Style="{StaticResource SecondaryButton}"/>
        <Separator/>
     <Button Content="Clear Log" 
Command="{Binding ClearLogCommand}"
       Style="{StaticResource SecondaryButton}"/>
    </ToolBar>
    
    <!-- Configuration Panel -->
    <Border BorderBrush="{StaticResource BorderBrush}" 
            BorderThickness="1" 
            Background="White">
        <Grid>
         <!-- Configuration Info (Left) -->
       <StackPanel>
     <TextBlock Text="Monitoring Configuration" 
                    Style="{StaticResource HeadingTextBlock}"/>
          
                <Grid>
        <Grid.ColumnDefinitions>
         <ColumnDefinition Width="120"/>
      <ColumnDefinition Width="*"/>
       </Grid.ColumnDefinitions>
   
   <TextBlock Text="Watch Folder:" FontWeight="SemiBold"/>
         <TextBlock Grid.Column="1" Text="{Binding WatchFolder}" 
         TextTrimming="CharacterEllipsis"/>
         
   <!-- More fields... -->
     </Grid>
      </StackPanel>
            
         <!-- Status Indicator (Right) -->
 <Border>
     <StackPanel HorizontalAlignment="Center">
   <TextBlock Text="Status"/>
        
         <StackPanel Orientation="Horizontal">
         <Ellipse Width="12" Height="12">
            <Ellipse.Style>
      <Style TargetType="Ellipse">
       <Setter Property="Fill" Value="#6C757D"/>
       <Style.Triggers>
          <DataTrigger Binding="{Binding CurrentStatus}" 
        Value="{x:Static models:MonitoringStatus.Running}">
   <Setter Property="Fill" Value="#28A745"/>
             </DataTrigger>
     <!-- More triggers... -->
            </Style.Triggers>
         </Style>
   </Ellipse.Style>
       </Ellipse>
       <TextBlock Text="{Binding StatusText}"/>
         </StackPanel>
       
         <CheckBox Content="Auto-start on load" 
IsChecked="{Binding AutoStart}"/>
              </StackPanel>
         </Border>
        </Grid>
    </Border>
    
    <!-- Activity Log -->
    <DataGrid ItemsSource="{Binding RecentActivity}"
              AutoGenerateColumns="False"
              IsReadOnly="True">
        <DataGrid.Columns>
       <DataGridTextColumn Header="Time" 
     Binding="{Binding FormattedTimestamp}"/>
<DataGridTextColumn Header="File" 
  Binding="{Binding FileName}"/>
            <DataGridTextColumn Header="Activity" 
            Binding="{Binding ActivityType}"/>
     <!-- More columns... -->
    </DataGrid.Columns>
    </DataGrid>
</UserControl>
```

### MainWindow Integration

```xaml
<Window.InputBindings>
    <!-- Phase 8: Auto-Monitor shortcut -->
    <KeyBinding Key="D3" Modifiers="Control" 
                Command="{Binding SwitchToAutoMonitorTabCommand}" />
</Window.InputBindings>

<TabControl>
    <TabItem Header="TV Episodes"/>
  <TabItem Header="Movies"/>
    <TabItem Header="Auto Monitor">
  <views:AutoMonitorView x:Name="AutoMonitorTabContent"/>
    </TabItem>
</TabControl>
```

---

## ?? Models

### MonitoringStatus

```csharp
public enum MonitoringStatus
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Error
}
```

### RecentActivityModel

```csharp
public class RecentActivityModel
{
    public DateTime Timestamp { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
  public bool IsSuccess { get; set; }
    
    public string FormattedTimestamp => 
        Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
}
```

### FileFoundEventArgs

```csharp
public class FileFoundEventArgs : EventArgs
{
    public string FilePath { get; }
    public string EventType { get; }
    public DateTime FoundAt { get; }
    
    public FileFoundEventArgs(string filePath, string eventType)
    {
        FilePath = filePath;
  EventType = eventType;
   FoundAt = DateTime.Now;
    }
}
```

### FileProcessedEventArgs

```csharp
public class FileProcessedEventArgs : EventArgs
{
    public string FilePath { get; }
    public bool Success { get; }
    public string Message { get; }
    public DateTime ProcessedAt { get; }
    
    public FileProcessedEventArgs(string filePath, bool success, string message)
    {
        FilePath = filePath;
        Success = success;
        Message = message;
        ProcessedAt = DateTime.Now;
    }
}
```

---

## ?? Configuration

### AppSettings.Monitoring

```csharp
public class MonitoringSettings
{
    public string WatchFolder { get; set; } = "";
    public string DestinationFolder { get; set; } = "";
    public string FileExtensions { get; set; } = "*.mp4;*.mkv;*.avi;*.m4v";
    public int StabilityDelaySeconds { get; set; } = 30;
    public bool AutoStartOnLoad { get; set; } = false;
    public int MaxAutoMonitorLogEntries { get; set; } = 100;
}
```

### Load and Save

```csharp
// Load
var settings = AppSettings.Load();
var monitoringConfig = settings.Monitoring;

// Modify
monitoringConfig.WatchFolder = @"C:\Downloads";
monitoringConfig.AutoStartOnLoad = true;

// Save
settings.Save();
```

---

## ?? Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **Ctrl+3** | Switch to Auto-Monitor tab |
| **Ctrl+,** | Open Settings |
| **F1** | Show keyboard shortcuts |
| **F5** | Refresh current view |

---

## ?? Dependency Injection

### App.xaml.cs

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Core Services
    services.AddSingleton<AppSettings>(sp => AppSettings.Load());
services.AddSingleton<FolderMonitorService>();
    
  // ViewModels
    services.AddSingleton<AutoMonitorViewModel>();
    services.AddTransient<TvEpisodesViewModel>();
    services.AddTransient<MoviesViewModel>();
  
    // Views
    services.AddSingleton<MainWindow>();
}

protected override void OnExit(ExitEventArgs e)
{
    // Stop monitoring and cleanup
    var autoMonitorViewModel = _serviceProvider?.GetService<AutoMonitorViewModel>();
    autoMonitorViewModel?.Dispose();
    
    _serviceProvider?.Dispose();
    base.OnExit(e);
}
```

---

## ?? Common Patterns

### Pattern 1: Start Monitoring on First Run

```csharp
public AutoMonitorViewModel(FolderMonitorService folderMonitorService, AppSettings appSettings)
{
    // ... initialization ...
    
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
```

### Pattern 2: Status Updates with Color

```csharp
public string StatusColor => CurrentStatus switch
{
    MonitoringStatus.Stopped => "#6C757D",    // Gray
    MonitoringStatus.Starting => "#FFC107",   // Yellow
    MonitoringStatus.Running => "#28A745",    // Green
    MonitoringStatus.Stopping => "#FFC107",   // Yellow
    MonitoringStatus.Error => "#DC3545",      // Red
    _ => "#6C757D"
};
```

### Pattern 3: Activity Log with Limit

```csharp
private void AddActivityLog(string fileName, string status, string message, bool isSuccess)
{
    var activity = new RecentActivityModel { /* ... */ };
    RecentActivity.Insert(0, activity);  // Add to top
    
    // Limit entries
  int maxEntries = _appSettings.Monitoring.MaxAutoMonitorLogEntries;
    while (RecentActivity.Count > maxEntries)
    {
        RecentActivity.RemoveAt(RecentActivity.Count - 1);  // Remove from bottom
  }
}
```

### Pattern 4: Cross-thread Event Handling

```csharp
private void OnFileProcessed(object? sender, FileProcessedEventArgs e)
{
    WpfApplication.Current.Dispatcher.Invoke(() =>
    {
        string fileName = Path.GetFileName(e.FilePath);
        AddActivityLog(fileName, e.Success ? "Completed" : "Failed", e.Message, e.Success);
    });
}
```

### Pattern 5: File Extension Parsing

```csharp
private string[] ParseFileExtensions(string extensions)
{
    if (string.IsNullOrWhiteSpace(extensions))
        return Array.Empty<string>();
    
    return extensions.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
         .Select(ext => ext.Trim())
        .Where(ext => !string.IsNullOrWhiteSpace(ext))
           .ToArray();
}
```

---

## ?? Troubleshooting

### Monitoring Won't Start

**Problem:** Start button disabled or monitoring fails to start  
**Solution:**
```csharp
// Check configuration
if (string.IsNullOrWhiteSpace(WatchFolder))
    // Watch folder not set - open configuration

if (!Directory.Exists(WatchFolder))
  // Watch folder doesn't exist - check path

if (string.IsNullOrWhiteSpace(DestinationFolder))
    // Destination folder not set - open configuration

var validation = PathValidator.ValidateDestinationPath(DestinationFolder);
if (!validation.IsValid)
    // Destination invalid - check path and permissions
```

### Files Not Being Detected

**Problem:** Files appear in watch folder but don't trigger processing  
**Solution:**
```csharp
// Check file extension matches configured list
var extensions = ParseFileExtensions(FileExtensions);
if (!extensions.Any(ext => filename.EndsWith(ext.TrimStart('*'))))
    // File extension not in list - add to configuration

// Check if file is TV show
int tvConfidence = MediaTypeDetector.GetTVEpisodeConfidence(filename);
if (tvConfidence <= 50)
    // Not detected as TV show - may be movie or invalid format
```

### Files Processing Too Quickly

**Problem:** Files being processed while still downloading  
**Solution:**
```csharp
// Increase stability delay
StabilityDelay = 60;  // Wait 60 seconds instead of 30
SaveConfiguration();
```

### Activity Log Not Updating

**Problem:** Events firing but UI not updating  
**Solution:**
```csharp
// Ensure UI updates on dispatcher thread
private void OnFileProcessed(object? sender, FileProcessedEventArgs e)
{
    WpfApplication.Current.Dispatcher.Invoke(() =>
    {
        // UI update code here
    });
}
```

---

## ?? Resources

### Key Files
- **Service:** `TorrentFileRenamer.Core/Services/FolderMonitorService.cs`
- **ViewModel:** `TorrentFileRenamer.WPF/ViewModels/AutoMonitorViewModel.cs`
- **View:** `TorrentFileRenamer.WPF/Views/AutoMonitorView.xaml`
- **Config Dialog:** `TorrentFileRenamer.WPF/Views/FolderMonitorConfigDialog.xaml`
- **Models:** `TorrentFileRenamer.WPF/Models/MonitoringStatus.cs`

### Documentation
- **Completion Report:** `PHASE8_COMPLETION_REPORT.md`
- **Quick Reference:** `PHASE8_QUICK_REFERENCE.md` (this file)
- **User Guide:** `PHASE8_USER_GUIDE.md`

---

**Quick Reference Last Updated:** December 2024  
**Phase 8 Status:** ? COMPLETE
**Build Status:** ? SUCCESSFUL
