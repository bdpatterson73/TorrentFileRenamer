# Phase 6 Quick Reference - Search, Filter & Export

## ?? Search Implementation

### SearchViewModel Usage
```csharp
// In parent ViewModel constructor
SearchViewModel = new SearchViewModel(searchService);
SearchViewModel.SearchChanged += OnSearchChanged;

// Handle search
private void OnSearchChanged(object? sender, EventArgs e)
{
    ApplySearchAndFilters();
}

// Apply search
private void ApplySearchAndFilters()
{
    var criteria = FilterViewModel.GetCurrentCriteria();
    criteria.SearchText = SearchViewModel.SearchText;
    
    var filtered = _searchService.SearchMovies(AllMovies, criteria);
    Movies = new ObservableCollection<MovieFileModel>(filtered);
 
    SearchViewModel.UpdateResultCount(Movies.Count);
    StatsViewModel.UpdateMovieStatistics(Movies);
}
```

### SearchPanel XAML
```xaml
<controls:SearchPanel DataContext="{Binding SearchViewModel}"/>
```

## ?? Filter Implementation

### FilterViewModel Usage
```csharp
// Constructor
FilterViewModel = new FilterViewModel(searchService, dialogService);
FilterViewModel.FiltersApplied += OnFiltersApplied;
FilterViewModel.FiltersReset += OnFiltersReset;

// Update available extensions
var extensions = _searchService.GetMovieExtensions(AllMovies);
FilterViewModel.UpdateAvailableExtensions(extensions);

// Get and apply criteria
private void OnFiltersApplied(object? sender, EventArgs e)
{
    var criteria = FilterViewModel.GetCurrentCriteria();
    var filtered = _searchService.SearchMovies(AllMovies, criteria);
    Movies = new ObservableCollection<MovieFileModel>(filtered);
}
```

### AdvancedFilterPanel XAML
```xaml
<controls:AdvancedFilterPanel DataContext="{Binding FilterViewModel}"/>
```

### Filter Presets
```csharp
// Predefined presets available:
- "High Confidence Only" (70%+)
- "Needs Review" (<40% or errors)
- "Large Files (>1GB)"
- "MKV Files"
- "Processed Successfully"

// Access
var presets = FilterPreset.GetPredefinedPresets();
```

## ?? Statistics Implementation

### StatsViewModel Usage
```csharp
// Constructor
StatsViewModel = new StatsViewModel(searchService);
StatsViewModel.RefreshRequested += OnStatsRefresh;

// Update stats
StatsViewModel.UpdateMovieStatistics(Movies);

// Get summary
var summary = StatsViewModel.GetSummaryText();
// "150 files | 120 processed | 5 errors | 1.2 GB"
```

### StatsWidget XAML
```xaml
<controls:StatsWidget DataContext="{Binding StatsViewModel}"/>
```

## ?? Export Implementation

### ExportViewModel Usage
```csharp
// Constructor
ExportViewModel = new ExportViewModel(exportService, dialogService);
ExportViewModel.ExportCompleted += OnExportCompleted;
ExportViewModel.ExportFailed += OnExportFailed;

// Export movies
await ExportViewModel.ExportMoviesAsync(Movies);

// Export episodes
await ExportViewModel.ExportEpisodesAsync(Episodes);

// Generate summary
var summary = await ExportViewModel.GenerateMovieSummaryAsync(Movies, stats);
```

### ExportDialog Usage
```csharp
private async Task ExecuteExport()
{
    var dialog = new ExportDialog
    {
        Owner = Application.Current.MainWindow,
 DataContext = ExportViewModel
    };
    
    if (dialog.ShowDialog() == true)
    {
        await ExportViewModel.ExportMoviesAsync(Movies);
    }
}
```

### Export Options
```csharp
// Quick presets
var options = ExportOptions.Default;   // All common fields
var options = ExportOptions.Minimal;   // Name and status only
var options = ExportOptions.Detailed;  // All fields

// Custom options
var options = new ExportOptions
{
    Format = ExportFormat.Csv,
    OutputPath = "export.csv",
    IncludeFileName = true,
IncludeNewFileName = true,
    IncludeMediaName = true,
 IncludeConfidence = true,
    IncludeStatus = true,
    ExportSelectedOnly = false
};
```

## ?? SearchCriteria Configuration

```csharp
var criteria = new SearchCriteria
{
    // Text search
    SearchText = "breaking bad",
    SearchInFileName = true,
    SearchInMovieName = true,
 SearchInShowName = true,
    SearchInYear = false,
    
    // Confidence filter (0-100)
    MinConfidence = 70,
    MaxConfidence = 100,
    
    // File size filter (bytes)
    MinFileSize = 1024 * 1024 * 100,  // 100 MB
    MaxFileSize = long.MaxValue,
    
    // Extensions
    SelectedExtensions = new List<string> { ".mkv", ".mp4" },
    
    // Status
    SelectedStatuses = new List<ProcessingStatus> 
    { 
        ProcessingStatus.Completed,
  ProcessingStatus.Pending 
    },
    
    // Date range
    DateAddedFrom = DateTime.Now.AddDays(-7),
    DateAddedTo = DateTime.Now
};

// Check if any filters active
bool hasFilters = criteria.HasActiveFilters;

// Reset all
criteria.Reset();

// Clone
var copy = criteria.Clone();
```

## ?? Service Integration

### Register Services in App.xaml.cs
```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Phase 6 Services
    services.AddSingleton<ISearchService, SearchService>();
    services.AddSingleton<IExportService, ExportService>();
    
    // Phase 6 ViewModels (Transient - one per parent)
    services.AddTransient<SearchViewModel>();
    services.AddTransient<FilterViewModel>();
    services.AddTransient<ExportViewModel>();
    services.AddTransient<StatsViewModel>();
}
```

### Inject in Parent ViewModels
```csharp
public class MoviesViewModel : ViewModelBase
{
    private readonly ISearchService _searchService;
    private readonly IExportService _exportService;
    private readonly IDialogService _dialogService;
    
    public SearchViewModel SearchViewModel { get; }
    public FilterViewModel FilterViewModel { get; }
    public ExportViewModel ExportViewModel { get; }
    public StatsViewModel StatsViewModel { get; }
    
    public MoviesViewModel(
        ISearchService searchService,
        IExportService exportService,
        IDialogService dialogService)
{
    _searchService = searchService;
        _exportService = exportService;
  _dialogService = dialogService;
        
     // Initialize Phase 6 ViewModels
        SearchViewModel = new SearchViewModel(searchService);
        FilterViewModel = new FilterViewModel(searchService, dialogService);
        ExportViewModel = new ExportViewModel(exportService, dialogService);
        StatsViewModel = new StatsViewModel(searchService);
     
        // Subscribe to events
        SearchViewModel.SearchChanged += OnSearchChanged;
    FilterViewModel.FiltersApplied += OnFiltersApplied;
        FilterViewModel.FiltersReset += OnFiltersReset;
        StatsViewModel.RefreshRequested += OnStatsRefresh;
    }
}
```

## ?? Keyboard Shortcuts (To Implement)

```csharp
// In MainWindow.xaml
<Window.InputBindings>
    <!-- Phase 6 Shortcuts -->
    <KeyBinding Key="F" Modifiers="Control" 
           Command="{Binding FocusSearchCommand}"/>
    <KeyBinding Key="F" Modifiers="Control+Shift" 
             Command="{Binding ToggleFilterPanelCommand}"/>
    <KeyBinding Key="E" Modifiers="Control" 
         Command="{Binding ShowExportCommand}"/>
    <KeyBinding Key="Escape" 
       Command="{Binding ClearSearchCommand}"/>
</Window.InputBindings>
```

## ?? Common Patterns

### Search with Debouncing
```csharp
private System.Timers.Timer? _searchDebounceTimer;

private void OnSearchTextChanged()
{
    _searchDebounceTimer?.Stop();
    _searchDebounceTimer = new System.Timers.Timer(300); // 300ms delay
    _searchDebounceTimer.Elapsed += (s, e) =>
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
         ApplySearchAndFilters();
        });
    };
    _searchDebounceTimer.AutoReset = false;
 _searchDebounceTimer.Start();
}
```

### Filter with Status Update
```csharp
private void ApplyFilters()
{
    try
    {
        IsBusy = true;
        StatusMessage = "Applying filters...";
        
      var criteria = FilterViewModel.GetCurrentCriteria();
        var filtered = _searchService.SearchMovies(AllMovies, criteria);
        
        Movies = new ObservableCollection<MovieFileModel>(filtered);
        StatsViewModel.UpdateMovieStatistics(Movies);
      SearchViewModel.UpdateResultCount(Movies.Count);
   
        StatusMessage = $"Filters applied: {Movies.Count} results";
    }
    finally
 {
        IsBusy = false;
    }
}
```

### Export with Progress
```csharp
private async Task ExportAsync()
{
    var options = new ExportOptions
    {
    Format = ExportFormat.Csv,
        OutputPath = _dialogService.ShowSaveFileDialog("export.csv", "CSV Files|*.csv")
    };
    
    if (string.IsNullOrEmpty(options.OutputPath))
        return;
    
    var progress = new Progress<int>(percent =>
    {
        ExportProgress = percent;
    StatusMessage = $"Exporting... {percent}%";
    });
    
    bool success = await _exportService.ExportMoviesAsync(Movies, options, progress);
    
    if (success)
    {
  StatusMessage = "Export completed successfully";
  _dialogService.ShowMessage("Export Complete", 
  $"Data exported to:\n{options.OutputPath}");
    }
}
```

## ?? FileStatistics Properties

```csharp
// Available properties
stats.TotalFiles          // int
stats.ProcessedFiles      // int (Completed)
stats.PendingFiles        // int (Pending)
stats.ErrorFiles          // int (Failed)
stats.UnprocessedFiles    // int (Unparsed)
stats.AverageConfidence   // double (0-100)
stats.TotalFileSize       // long (bytes)
stats.LastUpdated         // DateTime

// Calculated properties
stats.ProgressPercentage  // double (0-100)
stats.ProgressText        // "120/150 files processed (80.0%)"
stats.FileSizeText        // "1.2 GB" or "850 MB"

// Methods
stats.Reset();  // Reset all to zero
```

## ?? Update Patterns

### After Scanning
```csharp
private async Task ScanAsync()
{
    var episodes = await _scanningService.ScanAsync(...);
    
    AllEpisodes = new ObservableCollection<FileEpisodeModel>(episodes);
    
    // Update search/filter components
    FilterViewModel.UpdateAvailableExtensions(
        _searchService.GetEpisodeExtensions(episodes));
    
    // Update stats
    StatsViewModel.UpdateEpisodeStatistics(AllEpisodes);
    
    // Apply any active filters
    if (FilterViewModel.HasActiveFilters || !string.IsNullOrEmpty(SearchViewModel.SearchText))
  {
        ApplySearchAndFilters();
    }
    else
    {
        Episodes = AllEpisodes;
    }
}
```

### After Processing
```csharp
private async Task ProcessAsync()
{
    await _processingService.ProcessFilesAsync(...);
    
    // Refresh stats
    StatsViewModel.UpdateMovieStatistics(Movies);
    
    // Re-apply filters (processed files may no longer match)
    if (FilterViewModel.HasActiveFilters)
    {
     ApplySearchAndFilters();
    }
}
```

## ?? Resources

### Key Files
- Models: `TorrentFileRenamer.WPF/Models/SearchCriteria.cs`
- Services: `TorrentFileRenamer.WPF/Services/SearchService.cs`
- ViewModels: `TorrentFileRenamer.WPF/ViewModels/SearchViewModel.cs`
- Controls: `TorrentFileRenamer.WPF/Views/Controls/SearchPanel.xaml`

### Documentation
- Implementation: `PHASE6_IMPLEMENTATION_PROGRESS.md`
- Integration Guide: (To be created)
- Testing Guide: (To be created)

---

**Phase 6 Foundation**: ? Complete  
**Build Status**: ? Successful  
**Next**: Integration with MainWindow and Views  
