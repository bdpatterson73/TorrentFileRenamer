# TorrentFileRenamer - WinForms to WPF Migration Plan

## Executive Summary

This document outlines a comprehensive plan to migrate the TorrentFileRenamer application from WinForms to WPF while maintaining .NET 8 as the target framework. The migration will modernize the UI architecture, improve maintainability, and leverage WPF's advanced features including MVVM pattern, data binding, and styling capabilities.

---

## Table of Contents

1. [Current Application Analysis](#current-application-analysis)
2. [Migration Strategy](#migration-strategy)
3. [Architecture Overview](#architecture-overview)
4. [Project Structure](#project-structure)
5. [Phase-by-Phase Implementation](#phase-by-phase-implementation)
6. [Technical Specifications](#technical-specifications)
7. [Testing Strategy](#testing-strategy)
8. [Risk Assessment](#risk-assessment)
9. [Timeline & Resources](#timeline--resources)
10. [Success Criteria](#success-criteria)

---

## 1. Current Application Analysis

### 1.1 Application Overview
**Application Name:** TorrentFileRenamer  
**Current Framework:** WinForms (.NET 8)  
**Target Framework:** WPF (.NET 8)  
**Primary Purpose:** Organize and rename TV episodes and movies for Plex media server

### 1.2 Key Features
- **TV Episode Organization:** Parse and rename TV episodes (S01E01 format)
- **Movie Organization:** Alphabetical organization with quality tag removal
- **Auto-Monitoring:** Watch folders for new files and process automatically
- **Multi-Episode Support:** Handle multi-episode files (S01E01-02)
- **Plex Compatibility:** Validate and ensure Plex-compatible naming
- **File Operations:** Copy, verify, and delete with retry logic
- **Smart Detection:** Distinguish between TV shows and movies automatically
- **Path Validation:** Network path support and disk space checking

### 1.3 Current Architecture Components

#### Core Classes
| Class | Purpose | Lines | Complexity |
|-------|---------|-------|------------|
| `frmMain.cs` | Main application form | ~1400 | High |
| `FileEpisode.cs` | TV episode parsing logic | Unknown | Medium |
| `MovieFile.cs` | Movie file parsing logic | Unknown | Medium |
| `FolderMonitorService.cs` | Auto-monitoring service | Unknown | Medium |
| `AppSettings.cs` | Settings & logging | ~250 | Low |
| `PathValidator.cs` | Path validation | Unknown | Low |
| `MediaTypeDetector.cs` | Media type detection | Unknown | Medium |
| `PlexCompatibilityValidator.cs` | Plex validation | Unknown | Medium |

#### Dialog Forms
- `frmScanOptions` - TV episode scan configuration
- `frmScanMovies` - Movie scan configuration
- `frmFolderMonitor` - Auto-monitor configuration

#### Dependencies
- **NExifTool** (v0.11.0) - Media metadata extraction
- **System.IO.Compression** (v4.3.0) - Archive handling
- **System.IO.Hashing** (v7.0.0) - File verification

### 1.4 Technical Debt & Challenges

#### Identified Issues
1. **Tight UI Coupling:** Business logic mixed with UI code in Form1.cs
2. **Threading Concerns:** Manual thread marshalling using `Invoke()`
3. **Custom Drawing:** Owner-drawn ListView with manual painting
4. **Event-Heavy Architecture:** Form event handlers throughout
5. **Limited Testability:** UI and business logic tightly coupled
6. **Hard-Coded UI:** No separation of presentation from logic

---

## 2. Migration Strategy

### 2.1 Approach: Gradual Migration with Parallel Development

**Strategy:** Create a new WPF project alongside the existing WinForms project, sharing business logic components, then gradually migrate features.

### 2.2 Migration Principles

1. **Preserve Functionality:** All existing features must work in WPF version
2. **MVVM Pattern:** Implement proper Model-View-ViewModel architecture
3. **Code Reuse:** Share business logic classes between projects during transition
4. **Modern UI:** Leverage WPF's styling and templating capabilities
5. **Testability:** Design for unit testing and maintainability
6. **Backward Compatibility:** Maintain settings compatibility during transition

### 2.3 Migration Goals

- ? Modern, responsive UI with WPF features
- ? Clean separation of concerns (MVVM)
- ? Improved maintainability and testability
- ? Enhanced user experience with animations and effects
- ? Better async/await support without manual threading
- ? Consistent styling and theming
- ? All original features preserved

---

## 3. Architecture Overview

### 3.1 WPF Project Architecture

```
TorrentFileRenamer.WPF/
??? App.xaml              # Application entry point
??? App.xaml.cs # Application startup logic
??? Models/     # Data models
? ??? FileEpisodeModel.cs     # TV episode data
?   ??? MovieFileModel.cs            # Movie data
?   ??? ProcessingProgressModel.cs   # Progress tracking
?   ??? MonitoringStatusModel.cs   # Auto-monitor status
??? ViewModels/  # MVVM ViewModels
?   ??? MainViewModel.cs             # Main window VM
?   ??? TvEpisodesViewModel.cs     # TV episodes tab VM
?   ??? MoviesViewModel.cs        # Movies tab VM
?   ??? AutoMonitorViewModel.cs      # Auto-monitor VM
?   ??? Base/
?       ??? ViewModelBase.cs         # Base VM class
?     ??? RelayCommand.cs    # Command implementation
??? Views/            # XAML Views
?   ??? MainWindow.xaml      # Main application window
?   ??? TvEpisodesView.xaml        # TV episodes tab
?   ??? MoviesView.xaml     # Movies tab
?   ??? AutoMonitorView.xaml         # Auto-monitor tab
?   ??? Dialogs/
?       ??? ScanOptionsDialog.xaml   # Scan configuration
?       ??? ProgressDialog.xaml      # Progress window
?       ??? AboutDialog.xaml      # About window
??? Services/               # Business logic services
?   ??? FileProcessingService.cs     # File operations
?   ??? ScanningService.cs           # File scanning
?   ??? SettingsService.cs           # Settings management
??? Converters/   # Value converters
?   ??? BoolToVisibilityConverter.cs
?   ??? StatusToColorConverter.cs
?   ??? FileSizeConverter.cs
??? Resources/        # Styles and resources
?   ??? Styles.xaml         # Global styles
?   ??? Colors.xaml    # Color scheme
?   ??? Icons.xaml    # Icon resources
??? Utilities/      # Helper classes
??? DialogService.cs      # Dialog helper
    ??? NavigationService.cs     # Navigation helper
```

### 3.2 Shared Business Logic (Common Library)

```
TorrentFileRenamer.Core/         # Shared .NET 8 Class Library
??? Models/
?   ??? FileEpisode.cs    # Existing TV episode logic
? ??? MovieFile.cs       # Existing movie logic
?   ??? MediaTypeDetector.cs# Existing detection logic
??? Services/
?   ??? FolderMonitorService.cs      # Existing auto-monitor
?   ??? PathValidator.cs         # Existing validation
?   ??? PlexCompatibilityValidator.cs # Existing Plex validation
??? Utilities/
?   ??? CRC32.cs    # Existing hash utilities
?   ??? HashHelper.cs        # Existing hash helpers
??? Configuration/
    ??? AppSettings.cs      # Existing settings
    ??? LoggingService.cs       # Existing logging
```

---

## 4. Project Structure

### 4.1 Solution Structure

```
TorrentFileRenamer.sln
??? TorrentFileRenamer.Core/         # .NET 8 Class Library (Shared)
??? TorrentFileRenamer.WinForms/     # Existing WinForms (Deprecated)
??? TorrentFileRenamer.WPF/      # New WPF Application (.NET 8)
```

### 4.2 Project References

```
TorrentFileRenamer.WPF
??? References TorrentFileRenamer.Core

TorrentFileRenamer.WinForms (Legacy)
??? References TorrentFileRenamer.Core
```

---

## 5. Phase-by-Phase Implementation

### Phase 1: Foundation Setup (Week 1)

#### 5.1.1 Create Project Structure
- [ ] Create `TorrentFileRenamer.Core` class library (.NET 8)
- [ ] Create `TorrentFileRenamer.WPF` WPF application (.NET 8)
- [ ] Add NuGet packages to WPF project:
  - `CommunityToolkit.Mvvm` (8.x) - MVVM helpers
  - `MaterialDesignThemes` (4.x) - Modern UI components
  - `NExifTool` (0.11.0) - Media metadata
  - `System.IO.Hashing` (7.0.0) - File hashing
- [ ] Configure project dependencies

#### 5.1.2 Extract Business Logic to Core
- [ ] Move `FileEpisode.cs` to Core project
- [ ] Move `MovieFile.cs` to Core project
- [ ] Move `MediaTypeDetector.cs` to Core project
- [ ] Move `FolderMonitorService.cs` to Core project
- [ ] Move `PathValidator.cs` to Core project
- [ ] Move `PlexCompatibilityValidator.cs` to Core project
- [ ] Move `AppSettings.cs` and `LoggingService.cs` to Core project
- [ ] Move utility classes (`CRC32.cs`, `HashHelper.cs`, etc.) to Core
- [ ] Remove any WinForms-specific dependencies from moved classes
- [ ] Update namespaces to `TorrentFileRenamer.Core.*`

#### 5.1.3 Create MVVM Infrastructure
- [ ] Create `ViewModelBase` with INotifyPropertyChanged
- [ ] Create `RelayCommand` implementation
- [ ] Create `AsyncRelayCommand` for async operations
- [ ] Create `ObservableObject` base class
- [ ] Set up dependency injection container (Microsoft.Extensions.DependencyInjection)

**Deliverables:**
- Working Core library with business logic
- Empty WPF application shell
- MVVM base classes

---

### Phase 2: Main Window & Navigation (Week 2)

#### 5.2.1 Main Window Layout
- [ ] Create `MainWindow.xaml` with TabControl
- [ ] Create `MainViewModel.cs` 
- [ ] Implement status bar (progress, messages)
- [ ] Create menu system (File, Edit, Tools, Help)
- [ ] Add toolbar with common actions
- [ ] Implement window chrome and styling

#### 5.2.2 Navigation Infrastructure
- [ ] Implement tab navigation
- [ ] Create view switching logic
- [ ] Set up data contexts for tabs
- [ ] Create navigation service

#### 5.2.3 Global Styles & Resources
- [ ] Create `Styles.xaml` with consistent button styles
- [ ] Create `Colors.xaml` with color scheme
- [ ] Define typography resources
- [ ] Create reusable control templates
- [ ] Set up icon resources

**XAML Structure Example:**
```xml
<Window x:Class="TorrentFileRenamer.WPF.Views.MainWindow"
        xmlns:vm="clr-namespace:TorrentFileRenamer.WPF.ViewModels">
 <Window.DataContext>
        <vm:MainViewModel/>
    </Window.DataContext>
    <Grid>
        <Grid.RowDefinitions>
       <RowDefinition Height="Auto"/> <!-- Menu -->
      <RowDefinition Height="Auto"/> <!-- Toolbar -->
   <RowDefinition Height="*"/>    <!-- Content -->
          <RowDefinition Height="Auto"/> <!-- Status Bar -->
    </Grid.RowDefinitions>
   
      <Menu Grid.Row="0">
            <MenuItem Header="_File">
         <MenuItem Header="_Settings" Command="{Binding ShowSettingsCommand}"/>
     <MenuItem Header="E_xit" Command="{Binding ExitCommand}"/>
            </MenuItem>
            <!-- More menu items -->
        </Menu>
   
        <ToolBar Grid.Row="1">
            <!-- Toolbar buttons -->
        </ToolBar>
        
        <TabControl Grid.Row="2">
        <TabItem Header="TV Episodes">
      <views:TvEpisodesView/>
            </TabItem>
            <TabItem Header="Movies">
                <views:MoviesView/>
   </TabItem>
    <TabItem Header="Auto Monitor">
  <views:AutoMonitorView/>
  </TabItem>
        </TabControl>
     
        <StatusBar Grid.Row="3">
     <StatusBarItem>
                <TextBlock Text="{Binding StatusMessage}"/>
      </StatusBarItem>
        <StatusBarItem>
  <ProgressBar Value="{Binding ProgressValue}" 
  Maximum="{Binding ProgressMaximum}"
         Width="200"/>
    </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```

**Deliverables:**
- Functional main window with navigation
- Basic styling and theming
- Menu and toolbar structure

---

### Phase 3: TV Episodes Feature (Week 3-4)

#### 5.3.1 TV Episodes View
- [ ] Create `TvEpisodesView.xaml`
- [ ] Create `TvEpisodesViewModel.cs`
- [ ] Implement DataGrid for episode list with columns:
  - Source File Path
  - Destination Path
  - Show Name
  - Season Number
  - Episode Number(s)
  - Status
- [ ] Add context menu (Remove, Clear, Select All)
- [ ] Implement row alternating colors
- [ ] Add item selection and multi-select

#### 5.3.2 Scan Functionality
- [ ] Create `ScanOptionsDialog.xaml`
- [ ] Create `ScanOptionsViewModel.cs`
- [ ] Implement folder picker (using FolderBrowserDialog or custom)
- [ ] Add extension filter configuration
- [ ] Implement scan progress dialog
- [ ] Wire up scan command in ViewModel
- [ ] Implement async scanning with progress reporting

#### 5.3.3 Process Functionality
- [ ] Create `ProcessingService.cs` in Core library
- [ ] Implement file copy with progress
- [ ] Implement file verification (size comparison)
- [ ] Implement original file deletion
- [ ] Add retry logic with exponential backoff
- [ ] Create progress dialog for processing
- [ ] Wire up process command
- [ ] Implement cancellation support

#### 5.3.4 Data Binding & Commands
```csharp
public class TvEpisodesViewModel : ViewModelBase
{
    private ObservableCollection<FileEpisodeModel> _episodes;
    public ObservableCollection<FileEpisodeModel> Episodes
    {
        get => _episodes;
        set => SetProperty(ref _episodes, value);
    }

    public ICommand ScanCommand { get; }
    public ICommand ProcessCommand { get; }
    public ICommand RemoveSelectedCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand RemoveUnparsedCommand { get; }

  public TvEpisodesViewModel(IScanningService scanningService, 
    IFileProcessingService processingService)
    {
ScanCommand = new AsyncRelayCommand(ExecuteScanAsync);
 ProcessCommand = new AsyncRelayCommand(ExecuteProcessAsync, 
     CanExecuteProcess);
     RemoveSelectedCommand = new RelayCommand(ExecuteRemoveSelected);
        ClearAllCommand = new RelayCommand(ExecuteClearAll);
    }

    private async Task ExecuteScanAsync()
    {
        // Implementation
    }
}
```

**Deliverables:**
- Fully functional TV episodes tab
- Scan and process operations working
- Progress tracking and error handling

---

### Phase 4: Movies Feature (Week 5)

#### 5.4.1 Movies View
- [ ] Create `MoviesView.xaml`
- [ ] Create `MoviesViewModel.cs`
- [ ] Implement DataGrid for movie list with columns:
  - Source File Path
  - Destination Directory
  - Movie Name
  - Year
  - Status
- [ ] Add context menu
- [ ] Implement selection management

#### 5.4.2 Movie Scan & Process
- [ ] Create `ScanMoviesDialog.xaml`
- [ ] Create `ScanMoviesViewModel.cs`
- [ ] Implement movie scanning logic
- [ ] Wire up movie processing with shared service
- [ ] Add movie-specific validation
- [ ] Implement alphabetical organization

#### 5.4.3 Movie Detection
- [ ] Integrate `MediaTypeDetector` for confidence scoring
- [ ] Display confidence indicators in UI
- [ ] Allow manual override of detection
- [ ] Implement filtering by confidence level

**Deliverables:**
- Fully functional movies tab
- Movie scanning and processing working
- Smart detection integrated

---

### Phase 5: Auto-Monitor Feature (Week 6)

#### 5.5.1 Auto-Monitor View
- [ ] Create `AutoMonitorView.xaml`
- [ ] Create `AutoMonitorViewModel.cs`
- [ ] Implement configuration panel:
  - Watch folder selector
  - Destination folder selector
  - File extensions input
  - Stability delay slider
  - Auto-start checkbox
- [ ] Create monitoring status display
- [ ] Add processed files log (recent items)

#### 5.5.2 Folder Monitoring Configuration Dialog
- [ ] Create `FolderMonitorConfigDialog.xaml`
- [ ] Create `FolderMonitorConfigViewModel.cs`
- [ ] Implement all configuration options
- [ ] Add validation for paths and settings
- [ ] Implement save/load of settings

#### 5.5.3 Integration
- [ ] Wire up `FolderMonitorService` from Core
- [ ] Implement event subscriptions in ViewModel
- [ ] Add status updates to UI
- [ ] Display processing results in log
- [ ] Implement start/stop controls
- [ ] Add auto-start on application load

**XAML Example:**
```xml
<UserControl x:Class="TorrentFileRenamer.WPF.Views.AutoMonitorView">
    <Grid>
        <Grid.RowDefinitions>
  <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Configuration Panel -->
        <GroupBox Grid.Row="0" Header="Configuration">
       <StackPanel>
           <Grid>
         <Grid.ColumnDefinitions>
  <ColumnDefinition Width="Auto"/>
    <ColumnDefinition Width="*"/>
  <ColumnDefinition Width="Auto"/>
   </Grid.ColumnDefinitions>
        
    <TextBlock Text="Watch Folder:" 
    VerticalAlignment="Center"/>
           <TextBox Grid.Column="1" 
       Text="{Binding WatchFolder}"/>
  <Button Grid.Column="2" 
               Content="Browse..."
           Command="{Binding BrowseWatchFolderCommand}"/>
          </Grid>
                <!-- More configuration controls -->
  </StackPanel>
        </GroupBox>
        
  <!-- Status Panel -->
    <GroupBox Grid.Row="1" Header="Status">
        <StackPanel Orientation="Horizontal">
             <TextBlock Text="Status:"/>
       <TextBlock Text="{Binding MonitoringStatus}" 
      FontWeight="Bold"/>
       <Button Content="{Binding StartStopButtonText}"
      Command="{Binding ToggleMonitoringCommand}"/>
        </StackPanel>
     </GroupBox>
        
        <!-- Log Panel -->
        <GroupBox Grid.Row="2" Header="Recent Activity">
            <DataGrid ItemsSource="{Binding RecentActivity}"
        AutoGenerateColumns="False"
         IsReadOnly="True">
      <DataGrid.Columns>
       <DataGridTextColumn Header="Time" 
             Binding="{Binding Timestamp}"/>
          <DataGridTextColumn Header="File" 
                 Binding="{Binding FileName}"/>
   <DataGridTextColumn Header="Status" 
         Binding="{Binding Status}"/>
   <DataGridTextColumn Header="Message" 
          Binding="{Binding Message}"/>
              </DataGrid.Columns>
    </DataGrid>
        </GroupBox>
    </Grid>
</UserControl>
```

**Deliverables:**
- Fully functional auto-monitor tab
- Configuration and status display working
- Integration with folder monitoring service

---

### Phase 6: Dialogs & Utilities (Week 7)

#### 5.6.1 Common Dialogs
- [ ] Create `ProgressDialog.xaml` - Reusable progress window
- [ ] Create `MessageDialog.xaml` - Custom message box
- [ ] Create `ConfirmationDialog.xaml` - Confirmation dialogs
- [ ] Create `AboutDialog.xaml` - About window
- [ ] Create `LogViewerDialog.xaml` - Log viewer

#### 5.6.2 Dialog Service
```csharp
public interface IDialogService
{
    Task<bool> ShowConfirmationAsync(string title, string message);
    Task ShowMessageAsync(string title, string message);
    Task<string?> ShowFolderBrowserAsync(string initialPath);
    Task ShowProgressAsync(string title, Func<IProgress<double>, Task> operation);
    void ShowAbout();
    void ShowLogs();
}
```

#### 5.6.3 Utilities
- [ ] Create value converters (status to color, bool to visibility, etc.)
- [ ] Create validation rules for input fields
- [ ] Create custom controls if needed
- [ ] Implement file drop support

**Deliverables:**
- Reusable dialog system
- Common utilities and converters
- Enhanced user interactions

---

### Phase 7: Settings & Persistence (Week 8)

#### 5.7.1 Settings System
- [ ] Create `SettingsViewModel.cs`
- [ ] Create `SettingsView.xaml` or use dialog
- [ ] Implement settings categories:
  - General (default paths, extensions)
  - Auto-Monitor (monitoring configuration)
  - Plex (Plex validation settings)
  - Logging (log retention, debug mode)
  - UI (theme, colors)
- [ ] Wire up settings save/load
- [ ] Implement settings validation

#### 5.7.2 Data Persistence
- [ ] Ensure compatibility with existing `AppSettings.cs`
- [ ] Implement migration from WinForms settings if needed
- [ ] Add settings backup/restore
- [ ] Implement settings reset to defaults

#### 5.7.3 Recent Files & Paths
- [ ] Implement MRU (Most Recently Used) lists
- [ ] Store last used scan paths
- [ ] Store window position and size
- [ ] Persist column widths and sorting

**Deliverables:**
- Complete settings management
- Data persistence working
- Settings migration from WinForms

---

### Phase 8: Advanced Features (Week 9)

#### 5.8.1 Enhanced UI Features
- [ ] Add drag-and-drop for files and folders
- [ ] Implement context-sensitive help tooltips
- [ ] Add keyboard shortcuts (Ctrl+S for scan, etc.)
- [ ] Implement search/filter in data grids
- [ ] Add sorting and grouping in lists

#### 5.8.2 Plex Integration
- [ ] Display Plex compatibility warnings in UI
- [ ] Implement auto-fix suggestions
- [ ] Add validation result indicators
- [ ] Create Plex compatibility report

#### 5.8.3 File Operations
- [ ] Add file preview (show filename before/after)
- [ ] Implement undo/redo for certain operations
- [ ] Add dry-run mode (simulate without copying)
- [ ] Implement batch operations optimization

#### 5.8.4 Visual Enhancements
- [ ] Add animations for status changes
- [ ] Implement loading spinners for async operations
- [ ] Add success/error visual feedback
- [ ] Create custom styling for important UI elements

**Deliverables:**
- Enhanced user experience
- Advanced features implemented
- Visual polish applied

---

### Phase 9: Testing & Quality Assurance (Week 10)

#### 5.9.1 Unit Testing
- [ ] Create `TorrentFileRenamer.Tests` project
- [ ] Write unit tests for ViewModels
- [ ] Write unit tests for business logic in Core
- [ ] Write unit tests for services
- [ ] Achieve >80% code coverage target

#### 5.9.2 Integration Testing
- [ ] Test file scanning end-to-end
- [ ] Test file processing end-to-end
- [ ] Test auto-monitoring functionality
- [ ] Test settings persistence
- [ ] Test error scenarios and edge cases

#### 5.9.3 UI Testing
- [ ] Manual testing of all features
- [ ] Test on different screen resolutions
- [ ] Test keyboard navigation
- [ ] Test accessibility features
- [ ] Performance testing with large file sets

#### 5.9.4 Bug Fixes & Refinement
- [ ] Fix identified bugs
- [ ] Refine UI based on testing
- [ ] Optimize performance bottlenecks
- [ ] Address usability issues

**Deliverables:**
- Comprehensive test suite
- Bug-free application
- Performance optimizations

---

### Phase 10: Documentation & Deployment (Week 11)

#### 5.10.1 Documentation
- [ ] Create user guide
- [ ] Document keyboard shortcuts
- [ ] Create troubleshooting guide
- [ ] Write developer documentation
- [ ] Create migration guide from WinForms version

#### 5.10.2 Deployment Preparation
- [ ] Create installer (using WiX or Advanced Installer)
- [ ] Configure application icon
- [ ] Set up version numbering
- [ ] Create release notes
- [ ] Prepare update mechanism (if needed)

#### 5.10.3 Release
- [ ] Create release builds
- [ ] Test installer
- [ ] Create deployment package
- [ ] Publish to GitHub releases
- [ ] Update documentation

**Deliverables:**
- Complete documentation
- Installer package
- Release artifacts

---

## 6. Technical Specifications

### 6.1 MVVM Implementation

#### 6.1.1 ViewModelBase
```csharp
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
    if (EqualityComparer<T>.Default.Equals(field, value))
        return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

#### 6.1.2 RelayCommand
```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
  }

    public event EventHandler? CanExecuteChanged
  {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
}

public class AsyncRelayCommand : ICommand
{
 private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
_canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;

        _isExecuting = true;
  CommandManager.InvalidateRequerySuggested();

        try
        {
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
```

### 6.2 Dependency Injection Setup

#### 6.2.1 App.xaml.cs Configuration
```csharp
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
    base.OnStartup(e);

  var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IScanningService, ScanningService>();
        services.AddSingleton<IFileProcessingService, FileProcessingService>();
    services.AddSingleton<FolderMonitorService>();

 // ViewModels
        services.AddTransient<MainViewModel>();
  services.AddTransient<TvEpisodesViewModel>();
        services.AddTransient<MoviesViewModel>();
        services.AddTransient<AutoMonitorViewModel>();

        // Views
      services.AddTransient<MainWindow>();
    }
}
```

### 6.3 Data Models

#### 6.3.1 FileEpisodeModel
```csharp
public class FileEpisodeModel : ObservableObject
{
    private string _sourceFilePath = "";
    private string _destinationPath = "";
 private string _showName = "";
    private int _seasonNumber;
    private string _episodeNumbers = "";
  private ProcessingStatus _status = ProcessingStatus.Pending;
    private string _statusMessage = "";

    public string SourceFilePath
    {
      get => _sourceFilePath;
set => SetProperty(ref _sourceFilePath, value);
    }

    public string DestinationPath
    {
        get => _destinationPath;
        set => SetProperty(ref _destinationPath, value);
    }

    public string ShowName
    {
        get => _showName;
   set => SetProperty(ref _showName, value);
    }

    public int SeasonNumber
    {
        get => _seasonNumber;
    set => SetProperty(ref _seasonNumber, value);
    }

    public string EpisodeNumbers
    {
        get => _episodeNumbers;
        set => SetProperty(ref _episodeNumbers, value);
    }

    public ProcessingStatus Status
    {
        get => _status;
 set => SetProperty(ref _status, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
  set => SetProperty(ref _statusMessage, value);
    }
}

public enum ProcessingStatus
{
    Pending,
    Scanning,
    ReadyToProcess,
    Processing,
    Completed,
  Failed,
    Unparsed
}
```

### 6.4 Value Converters

#### 6.4.1 StatusToColorConverter
```csharp
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProcessingStatus status)
        {
            return status switch
            {
      ProcessingStatus.Completed => new SolidColorBrush(Colors.LightGreen),
           ProcessingStatus.Failed => new SolidColorBrush(Colors.LightCoral),
   ProcessingStatus.Processing => new SolidColorBrush(Colors.LightYellow),
       ProcessingStatus.Unparsed => new SolidColorBrush(Colors.LightGray),
          _ => new SolidColorBrush(Colors.White)
      };
        }
        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

### 6.5 Async Operations Pattern

```csharp
private async Task ExecuteScanAsync()
{
    try
    {
        IsBusy = true;
  StatusMessage = "Scanning for files...";
        ProgressValue = 0;

        var scanOptions = await _dialogService.ShowScanOptionsDialogAsync();
  if (scanOptions == null) return;

        var progress = new Progress<ScanProgress>(p =>
        {
            ProgressValue = p.CurrentProgress;
   ProgressMaximum = p.TotalCount;
   StatusMessage = $"Scanning: {p.CurrentFile}";
        });

   var episodes = await _scanningService.ScanForEpisodesAsync(
        scanOptions.SourcePath,
  scanOptions.DestinationPath,
            scanOptions.Extensions,
            progress,
       _cancellationTokenSource.Token);

        Episodes = new ObservableCollection<FileEpisodeModel>(episodes);
        StatusMessage = $"Scan complete: {episodes.Count} episodes found";
    }
    catch (OperationCanceledException)
    {
  StatusMessage = "Scan cancelled";
    }
    catch (Exception ex)
    {
      _loggingService.LogError("Scan failed", ex);
        await _dialogService.ShowErrorAsync("Scan Error", ex.Message);
    }
    finally
    {
     IsBusy = false;
    }
}
```

---

## 7. Testing Strategy

### 7.1 Unit Tests

#### 7.1.1 ViewModel Tests
```csharp
[TestClass]
public class TvEpisodesViewModelTests
{
    private Mock<IScanningService> _mockScanningService;
    private Mock<IFileProcessingService> _mockProcessingService;
    private Mock<IDialogService> _mockDialogService;
    private TvEpisodesViewModel _viewModel;

    [TestInitialize]
    public void Setup()
    {
        _mockScanningService = new Mock<IScanningService>();
        _mockProcessingService = new Mock<IFileProcessingService>();
        _mockDialogService = new Mock<IDialogService>();
        
        _viewModel = new TvEpisodesViewModel(
    _mockScanningService.Object,
            _mockProcessingService.Object,
      _mockDialogService.Object);
    }

    [TestMethod]
    public async Task ScanCommand_WithValidPath_PopulatesEpisodes()
    {
        // Arrange
        var expectedEpisodes = new List<FileEpisodeModel>
        {
     new FileEpisodeModel { ShowName = "TestShow", SeasonNumber = 1 }
        };
        
 _mockScanningService
          .Setup(s => s.ScanForEpisodesAsync(It.IsAny<string>(), It.IsAny<string>(), 
    It.IsAny<string>(), It.IsAny<IProgress<ScanProgress>>(), 
    It.IsAny<CancellationToken>()))
       .ReturnsAsync(expectedEpisodes);

     _mockDialogService
          .Setup(d => d.ShowScanOptionsDialogAsync())
   .ReturnsAsync(new ScanOptions { SourcePath = "C:\\Test" });

        // Act
        await _viewModel.ExecuteScanAsync();

        // Assert
        Assert.AreEqual(1, _viewModel.Episodes.Count);
        Assert.AreEqual("TestShow", _viewModel.Episodes[0].ShowName);
    }
}
```

#### 7.1.2 Service Tests
```csharp
[TestClass]
public class FileProcessingServiceTests
{
    [TestMethod]
    public async Task CopyFileAsync_WithValidPaths_CopiesSuccessfully()
    {
        // Arrange
        var service = new FileProcessingService();
   var sourcePath = CreateTestFile();
     var destPath = Path.Combine(Path.GetTempPath(), "test_dest.txt");

        // Act
    var result = await service.CopyFileAsync(sourcePath, destPath);

        // Assert
        Assert.IsTrue(result);
     Assert.IsTrue(File.Exists(destPath));

        // Cleanup
        File.Delete(sourcePath);
  File.Delete(destPath);
 }

    private string CreateTestFile()
    {
   var path = Path.Combine(Path.GetTempPath(), "test_source.txt");
        File.WriteAllText(path, "Test content");
        return path;
    }
}
```

### 7.2 Integration Tests

```csharp
[TestClass]
public class EndToEndTests
{
    [TestMethod]
    public async Task ScanAndProcess_CompleteWorkflow_Succeeds()
    {
        // Test complete workflow from scan to processing
        // This would test the full integration of services
    }
}
```

### 7.3 Test Coverage Goals

| Component | Target Coverage |
|-----------|----------------|
| ViewModels | 90%+ |
| Services | 85%+ |
| Core Business Logic | 95%+ |
| Converters | 80%+ |
| Overall | 85%+ |

---

## 8. Risk Assessment

### 8.1 Technical Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Complex business logic in UI code | High | High | Extract to Core library early in Phase 1 |
| Performance with large file sets | Medium | Medium | Implement virtualization in DataGrids, async operations |
| Threading issues with file I/O | Medium | Low | Use async/await consistently, test thoroughly |
| Settings migration from WinForms | Low | Medium | Implement backward-compatible settings loader |
| External dependencies breaking | Low | Low | Pin NuGet package versions, test updates |

### 8.2 Schedule Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Underestimating complexity | High | Medium | Build in 20% buffer time, prioritize features |
| Scope creep | Medium | High | Strict feature lockdown after Phase 8 |
| Resource availability | Medium | Medium | Document thoroughly, modular design |
| Testing takes longer than expected | Medium | Medium | Start testing early, automate where possible |

### 8.3 User Experience Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Users resist UI changes | Medium | Medium | Keep familiar workflows, provide migration guide |
| Missing critical features | High | Low | Thoroughly document existing features in Phase 0 |
| Performance perceived as worse | Medium | Low | Optimize early, use progress indicators |
| Bugs in initial release | High | Medium | Comprehensive testing, phased rollout |

---

## 9. Timeline & Resources

### 9.1 Estimated Timeline

```
Phase 1: Foundation Setup     Week 1 (40 hours)
Phase 2: Main Window & Navigation    Week 2      (40 hours)
Phase 3: TV Episodes Feature           Week 3-4    (80 hours)
Phase 4: Movies Feature                Week 5      (40 hours)
Phase 5: Auto-Monitor Feature          Week 6      (40 hours)
Phase 6: Dialogs & Utilities Week 7      (40 hours)
Phase 7: Settings & Persistence        Week 8   (40 hours)
Phase 8: Advanced Features             Week 9      (40 hours)
Phase 9: Testing & QA     Week 10     (40 hours)
Phase 10: Documentation & Deployment   Week 11   (40 hours)
---------------------------------------------------------------
Total Estimated Time     11 weeks    (440 hours)
```

### 9.2 Resource Requirements

**Developer Skills Required:**
- ? C# and .NET 8 expertise
- ? WPF and XAML proficiency
- ? MVVM pattern experience
- ? Async/await and multi-threading
- ? Unit testing experience
- ? Git version control

**Tools & Software:**
- Visual Studio 2022 (17.8+)
- .NET 8 SDK
- Git for version control
- NuGet package manager
- Optional: ReSharper or similar productivity tools

### 9.3 Milestones

| Milestone | Date | Deliverable |
|-----------|------|-------------|
| M1: Foundation Complete | End of Week 1 | Core library extracted, MVVM base |
| M2: Main Window Complete | End of Week 2 | Navigation working, shell ready |
| M3: TV Episodes Complete | End of Week 4 | Full TV episode functionality |
| M4: Movies Complete | End of Week 5 | Full movie functionality |
| M5: Auto-Monitor Complete | End of Week 6 | Auto-monitoring working |
| M6: Feature Complete | End of Week 9 | All features implemented |
| M7: Testing Complete | End of Week 10 | All tests passing, bugs fixed |
| M8: Release Ready | End of Week 11 | Installer ready, documentation complete |

---

## 10. Success Criteria

### 10.1 Functional Requirements
- ? All WinForms features replicated in WPF
- ? TV episode scanning and processing working
- ? Movie scanning and processing working
- ? Auto-monitoring functioning correctly
- ? Settings persistence compatible with WinForms
- ? All file operations (copy, verify, delete) working
- ? Error handling and logging functional
- ? Plex compatibility validation working

### 10.2 Non-Functional Requirements
- ? Application starts in < 2 seconds
- ? File scanning of 1000+ files completes in reasonable time
- ? UI remains responsive during all operations
- ? Memory usage stays within acceptable limits
- ? No crashes or data loss scenarios
- ? All operations are cancellable

### 10.3 Code Quality
- ? 85%+ unit test coverage
- ? No critical or high-severity code analysis warnings
- ? Clean separation of concerns (MVVM)
- ? Consistent coding style and naming conventions
- ? XML documentation on public APIs
- ? No code duplication (DRY principle)

### 10.4 User Experience
- ? Intuitive UI matching or exceeding WinForms usability
- ? Clear status messages and progress indicators
- ? Helpful error messages with recovery suggestions
- ? Consistent visual design
- ? Keyboard shortcuts for common operations
- ? Responsive and modern appearance

### 10.5 Documentation
- ? Complete user guide
- ? Developer documentation for maintenance
- ? Migration guide from WinForms version
- ? Troubleshooting guide
- ? Code comments where needed

---

## Appendix A: Key Differences - WinForms vs WPF

### UI Architecture

| Aspect | WinForms | WPF |
|--------|----------|-----|
| **Rendering** | GDI+ (Pixel-based) | DirectX (Vector-based) |
| **Scaling** | Poor DPI scaling | Excellent DPI scaling |
| **Styling** | Limited, property-based | Rich, template-based |
| **Data Binding** | Limited, manual | Powerful, automatic |
| **Layout** | Absolute positioning | Flexible layout containers |
| **Threading** | Invoke/BeginInvoke | Dispatcher |

### Development Patterns

| Aspect | WinForms | WPF |
|--------|----------|-----|
| **Pattern** | Code-behind heavy | MVVM recommended |
| **Separation** | UI + Logic mixed | Clean separation |
| **Testability** | Difficult | Easy (ViewModels) |
| **Reusability** | Limited | High (templates, styles) |

### Features

| Feature | WinForms | WPF |
|---------|----------|-----|
| **Animations** | Manual, limited | Built-in, easy |
| **Effects** | Minimal | Rich (blur, shadow, etc.) |
| **3D Graphics** | Not supported | Supported |
| **Custom Controls** | Complex | Straightforward |
| **Resources** | Limited | Comprehensive |

---

## Appendix B: Recommended NuGet Packages

### Core Packages
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
```

### UI Enhancement
```xml
<PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
<PackageReference Include="MaterialDesignColors" Version="2.1.4" />
```

### Existing Dependencies
```xml
<PackageReference Include="NExifTool" Version="0.11.0" />
<PackageReference Include="System.IO.Compression" Version="4.3.0" />
<PackageReference Include="System.IO.Hashing" Version="7.0.0" />
```

### Testing
```xml
<PackageReference Include="MSTest.TestFramework" Version="3.1.1" />
<PackageReference Include="MSTest.TestAdapter" Version="3.1.1" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

---

## Appendix C: Project File Templates

### TorrentFileRenamer.Core.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NExifTool" Version="0.11.0" />
    <PackageReference Include="System.IO.Compression" Version="4.3.0" />
    <PackageReference Include="System.IO.Hashing" Version="7.0.0" />
</ItemGroup>
</Project>
```

### TorrentFileRenamer.WPF.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>Resources\MainIcon.ico</ApplicationIcon>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\TorrentFileRenamer.Core\TorrentFileRenamer.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
    <PackageReference Include="MaterialDesignColors" Version="2.1.4" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  </ItemGroup>
</Project>
```

---

## Appendix D: Sample XAML Styles

### Styles.xaml (Excerpt)
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Base Button Style -->
<Style x:Key="PrimaryButton" TargetType="Button">
        <Setter Property="Background" Value="#2196F3"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Padding" Value="16,8"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Cursor" Value="Hand"/>
   <Setter Property="Template">
            <Setter.Value>
      <ControlTemplate TargetType="Button">
      <Border Background="{TemplateBinding Background}"
      CornerRadius="4"
      Padding="{TemplateBinding Padding}">
   <ContentPresenter HorizontalAlignment="Center"
    VerticalAlignment="Center"/>
</Border>
             </ControlTemplate>
       </Setter.Value>
        </Setter>
        <Style.Triggers>
    <Trigger Property="IsMouseOver" Value="True">
   <Setter Property="Background" Value="#1976D2"/>
            </Trigger>
            <Trigger Property="IsPressed" Value="True">
    <Setter Property="Background" Value="#0D47A1"/>
     </Trigger>
        <Trigger Property="IsEnabled" Value="False">
       <Setter Property="Background" Value="#BDBDBD"/>
                <Setter Property="Foreground" Value="#757575"/>
       </Trigger>
        </Style.Triggers>
    </Style>

    <!-- DataGrid Style -->
    <Style x:Key="ModernDataGrid" TargetType="DataGrid">
        <Setter Property="Background" Value="White"/>
    <Setter Property="BorderBrush" Value="#E0E0E0"/>
  <Setter Property="BorderThickness" Value="1"/>
     <Setter Property="GridLinesVisibility" Value="Horizontal"/>
        <Setter Property="HorizontalGridLinesBrush" Value="#F5F5F5"/>
        <Setter Property="AlternatingRowBackground" Value="#FAFAFA"/>
        <Setter Property="RowBackground" Value="White"/>
        <Setter Property="AutoGenerateColumns" Value="False"/>
        <Setter Property="CanUserAddRows" Value="False"/>
        <Setter Property="CanUserDeleteRows" Value="False"/>
     <Setter Property="SelectionMode" Value="Extended"/>
   <Setter Property="SelectionUnit" Value="FullRow"/>
    </Style>

</ResourceDictionary>
```

---

## Appendix E: Useful Links & Resources

### Official Documentation
- [WPF Documentation - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [MVVM Pattern Guide](https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)

### Community Resources
- [WPF Tutorial](https://www.wpftutorial.net/)
- [Material Design in XAML](http://materialdesigninxaml.net/)
- [MVVM Light Toolkit](http://www.mvvmlight.net/)

### Tools
- [Snoop - WPF Inspector](https://github.com/snoopwpf/snoopwpf)
- [XamlStyler](https://github.com/Xavalon/XamlStyler)
- [Visual Studio IntelliCode](https://visualstudio.microsoft.com/services/intellicode/)

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2024-01-XX | Development Team | Initial comprehensive migration plan created |

---

## Contact & Support

For questions or issues during migration:
- Project Repository: [GitHub - TorrentFileRenamer](https://github.com/bdpatterson73/TorrentFileRenamer)
- Issues: Use GitHub Issues for bug reports and feature requests

---

**End of Migration Plan**

This plan provides a comprehensive roadmap for migrating TorrentFileRenamer from WinForms to WPF while maintaining .NET 8 as the target framework. Follow the phases sequentially for best results, and adjust timelines as needed based on actual progress and resource availability.
