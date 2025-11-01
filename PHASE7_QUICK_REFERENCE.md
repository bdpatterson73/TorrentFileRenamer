# Phase 7 Quick Reference - Export & Data Management

## ?? Quick Start

### Export Data in 3 Steps
1. **Open Export Dialog:** Press `Ctrl+Shift+E` or click ?? Export button
2. **Select Format:** Choose CSV, JSON, or XML
3. **Click Export:** Select location and export!

---

## ?? Export Formats

### CSV (Comma-Separated Values)
```csharp
// Best for: Excel, Google Sheets, data analysis
ExportFormat.Csv
Extension: .csv
Filter: "CSV Files (*.csv)|*.csv"
```

**Example Output:**
```csv
"FileName","MovieName","Year","Status"
"Avatar.2009.1080p.mkv","Avatar","2009","Completed"
```

### JSON (JavaScript Object Notation)
```csharp
// Best for: APIs, web applications, programmatic access
ExportFormat.Json
Extension: .json
Filter: "JSON Files (*.json)|*.json"
```

**Example Output:**
```json
[
  {
    "fileName": "Avatar.2009.1080p.mkv",
    "movieName": "Avatar",
    "year": "2009",
 "status": "Completed"
}
]
```

### XML (Extensible Markup Language)
```csharp
// Best for: Enterprise systems, data exchange
ExportFormat.Xml
Extension: .xml
Filter: "XML Files (*.xml)|*.xml"
```

**Example Output:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Movies ExportDate="2024-12-14 10:30:00" TotalCount="1">
  <Movie>
    <FileName>Avatar.2009.1080p.mkv</FileName>
    <MovieName>Avatar</MovieName>
    <Year>2009</Year>
    <Status>Completed</Status>
  </Movie>
</Movies>
```

---

## ?? Export Options

### Field Selection

| Field | Movies | TV Episodes | Description |
|-------|--------|-------------|-------------|
| **FileName** | ? | ? | Original file name |
| **NewFileName** | ? | ? | Processed/renamed file name |
| **MediaName** | ? | ? | Movie name or show name |
| **Year** | ? | ? | Movie release year |
| **Season/Episode** | ? | ? | Season and episode numbers |
| **Confidence** | ? | ? | Parsing confidence score |
| **Status** | ? | ? | Processing status |
| **FileSize** | ? | ? | File size in bytes |
| **Extension** | ? | ? | File extension |
| **FullPaths** | ? | ? | Source and destination paths |
| **Errors** | ? | ? | Error messages if any |
| **Timestamp** | ? | ? | Export timestamp |

### Quick Presets

```csharp
// Default - Balanced selection
ExportOptions.Default
// Includes: FileName, NewFileName, MediaName, Year, Confidence,
//       Status, FileSize, Extension, Errors, Timestamp

// Minimal - Essential fields only
ExportOptions.Minimal
// Includes: FileName, NewFileName, MediaName, Status

// Detailed - Everything
ExportOptions.Detailed
// Includes: All available fields
```

---

## ?? Code Usage

### Basic Export (Movies)

```csharp
// In MoviesViewModel
private async Task ExecuteExportAsync()
{
    // Create export ViewModel
    var exportViewModel = new ExportViewModel(_exportService, _dialogService);
    
    // Show dialog
    var dialog = new ExportDialog
    {
     Owner = Application.Current.MainWindow,
        DataContext = exportViewModel
    };
    
    if (dialog.ShowDialog() == true)
    {
  // Export visible movies
        var success = await exportViewModel.ExportMoviesAsync(Movies);
        
        if (success)
      {
// Success handling
        }
    }
}
```

### Basic Export (TV Episodes)

```csharp
// In TvEpisodesViewModel
private async Task ExecuteExportAsync()
{
    var exportViewModel = new ExportViewModel(_exportService, _dialogService);
 var dialog = new ExportDialog { DataContext = exportViewModel };
    
    if (dialog.ShowDialog() == true)
 {
        var success = await exportViewModel.ExportEpisodesAsync(Episodes);
    }
}
```

### Custom Export Options

```csharp
// Programmatic export without dialog
var options = new ExportOptions
{
    Format = ExportFormat.Json,
    OutputPath = @"C:\Exports\movies.json",
    IncludeFileName = true,
    IncludeNewFileName = true,
IncludeMediaName = true,
    IncludeYear = true,
    IncludeStatus = true,
    IncludeFullPaths = false,
    IncludeTimestamp = true
};

var progress = new Progress<int>(percent =>
{
    StatusMessage = $"Exporting... {percent}%";
});

var success = await _exportService.ExportMoviesAsync(
    movies, 
    options, 
    progress
);
```

### Generate Summary Report

```csharp
// Get statistics first
var stats = StatsViewModel.GetCurrentStatistics();

// Generate summary
var summary = await _exportService.GenerateMovieSummaryAsync(
    movies, 
    stats
);

// Output:
// Movie Files Export Summary
// =========================
// Export Date: 2024-12-14 10:30:00
// Statistics:
//   Total Files: 150
//   Processed: 120
//   Pending: 25
//   Errors: 5
//   Average Confidence: 87.5%
//   Total Size: 125.4 GB
```

---

## ?? Usage Patterns

### Pattern 1: Export Filtered Results

```csharp
// User applies filters
FilterViewModel.ApplySearchText("Avatar");
FilterViewModel.SetMinConfidence(70);

// Export shows only filtered items
await ExecuteExportAsync();
// Result: Only high-confidence Avatar movies exported
```

### Pattern 2: Export with Progress

```csharp
IsProcessing = true;
StatusMessage = "Exporting...";

var progress = new Progress<int>(percent =>
{
    ProgressValue = percent;
    StatusMessage = $"Exporting... {percent}%";
});

try
{
    var success = await _exportService.ExportMoviesAsync(
        Movies.ToList(), 
        options, 
        progress
    );
    
    if (success)
    {
        StatusMessage = "Export completed successfully";
        
 // Offer to open file
        if (await _dialogService.ShowConfirmationAsync(
       "Export Complete", 
          "Open the exported file?"))
        {
    Process.Start(new ProcessStartInfo
            {
      FileName = options.OutputPath,
           UseShellExecute = true
    });
        }
    }
}
finally
{
    IsProcessing = false;
}
```

### Pattern 3: Export Summary with Data

```csharp
// Export data file
await exportViewModel.ExportMoviesAsync(movies);

// Generate and save summary
var stats = StatsViewModel.GetCurrentStatistics();
var summary = await _exportService.GenerateMovieSummaryAsync(
    movies, 
    stats
);

var summaryPath = Path.ChangeExtension(
    exportViewModel.Options.OutputPath, 
    ".summary.txt"
);

await File.WriteAllTextAsync(summaryPath, summary);
```

---

## ?? Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **Ctrl+Shift+E** | Open export dialog |
| **Ctrl+S** | Scan for files (context-aware) |
| **Ctrl+P** | Process files (context-aware) |
| **Ctrl+F** | Toggle search panel |
| **Ctrl+Shift+F** | Toggle advanced filters |
| **Ctrl+E** | Focus search box |
| **Escape** | Close dialog/clear search |
| **Enter** | Confirm/Execute (in dialogs) |

---

## ?? Configuration

### Service Registration

```csharp
// App.xaml.cs
private void ConfigureServices(IServiceCollection services)
{
  // Export service
    services.AddSingleton<IExportService, ExportService>();
    
    // Export ViewModel (transient for each use)
    services.AddTransient<ExportViewModel>();
}
```

### Dependency Injection

```csharp
// MoviesViewModel constructor
public MoviesViewModel(
    IScanningService scanningService,
    IFileProcessingService fileProcessingService,
    IDialogService dialogService,
    IWindowStateService windowStateService,
    IExportService exportService,  // Phase 7
    SearchViewModel searchViewModel,
    FilterViewModel filterViewModel,
    StatsViewModel statsViewModel)
{
    _exportService = exportService;
    
    // Initialize export command
    ExportCommand = new AsyncRelayCommand(
        ExecuteExportAsync, 
        () => Movies.Count > 0 && !IsProcessing
    );
}
```

---

## ?? UI Customization

### Export Dialog Styling

The export dialog uses consistent styling from previous phases:

```xaml
<!-- Card container for options -->
<Border Style="{StaticResource CardContainer}">
    <!-- Content -->
</Border>

<!-- Primary action button -->
<Button Content="Export" 
        Style="{StaticResource PrimaryButton}"/>

<!-- Secondary action button -->
<Button Content="Cancel" 
        Style="{StaticResource SecondaryButton}"/>

<!-- Progress bar -->
<ProgressBar Style="{StaticResource ModernProgressBar}"/>
```

### Toolbar Button

```xaml
<Button Content="?? Export" 
        Style="{StaticResource SecondaryButton}"
        Command="{Binding ShowExportCommand}"
      ToolTip="Export data to file (Ctrl+Shift+E)"/>
```

---

## ?? Export Statistics

### Export Metadata

All exports include metadata:

**CSV:**
- First row: Column headers
- Each row: One item

**JSON:**
- Array of objects
- `exportedAt` timestamp (ISO 8601)

**XML:**
- Root attributes: `ExportDate`, `TotalCount`
- Each item as child element
- `exportedAt` timestamp per item

### File Naming Convention

Default pattern: `export_YYYYMMDD_HHmmss.{ext}`

Examples:
- `export_20241214_103045.csv`
- `export_20241214_103045.json`
- `export_20241214_103045.xml`

---

## ?? Error Handling

### Common Export Errors

```csharp
// File access denied
catch (UnauthorizedAccessException ex)
{
    await _dialogService.ShowErrorAsync(
        "Access Denied", 
        "Cannot write to the selected location. Choose a different folder."
    );
}

// Disk full
catch (IOException ex)
{
    await _dialogService.ShowErrorAsync(
        "Export Failed", 
        "Not enough disk space or file is locked."
    );
}

// Invalid path
catch (ArgumentException ex)
{
    await _dialogService.ShowErrorAsync(
 "Invalid Path", 
 "The selected file path is invalid."
    );
}

// General error
catch (Exception ex)
{
    await _dialogService.ShowErrorAsync(
        "Export Error", 
      $"An error occurred during export:\n\n{ex.Message}"
    );
}
```

### Validation

```csharp
// Check if data available
if (Movies.Count == 0)
{
await _dialogService.ShowMessageAsync(
 "Export", 
        "No movies to export."
    );
    return;
}

// Check if output path selected
if (string.IsNullOrWhiteSpace(exportViewModel.Options.OutputPath))
{
    StatusMessage = "Export cancelled - no output path selected";
    return;
}

// Check if file already exists (dialog handles this)
```

---

## ?? Testing

### Manual Test Checklist

- [ ] Export 1 movie to CSV
- [ ] Export 100+ movies to CSV
- [ ] Export TV episodes to JSON
- [ ] Export with special characters in names
- [ ] Export with full paths enabled
- [ ] Export with minimal options
- [ ] Export with detailed options
- [ ] Cancel export dialog
- [ ] Export to read-only location (should fail gracefully)
- [ ] Open exported file from success dialog
- [ ] Test each export format (CSV, JSON, XML)
- [ ] Verify keyboard shortcut works
- [ ] Verify toolbar button works
- [ ] Verify command routing from MainViewModel

### Automated Test Ideas

```csharp
[Fact]
public async Task ExportMovies_ToCsv_ShouldCreateValidFile()
{
    // Arrange
    var movies = CreateTestMovies(10);
    var options = new ExportOptions
    {
     Format = ExportFormat.Csv,
      OutputPath = Path.GetTempFileName()
    };
    
    // Act
    var success = await _exportService.ExportMoviesAsync(
        movies, 
  options
    );
    
    // Assert
    Assert.True(success);
    Assert.True(File.Exists(options.OutputPath));
    
  var lines = File.ReadAllLines(options.OutputPath);
    Assert.Equal(11, lines.Length); // Header + 10 movies
}
```

---

## ?? Resources

### Related Documentation
- **IExportService Interface:** `Services/IExportService.cs`
- **ExportService Implementation:** `Services/ExportService.cs`
- **ExportViewModel:** `ViewModels/ExportViewModel.cs`
- **ExportDialog:** `Views/ExportDialog.xaml`
- **ExportOptions Model:** `Models/ExportOptions.cs`

### External Resources
- [CSV Format Specification](https://tools.ietf.org/html/rfc4180)
- [JSON Specification](https://www.json.org/)
- [XML Specification](https://www.w3.org/XML/)

---

## ?? Tips & Best Practices

### For Users

1. **Filter Before Export:** Use search/filter to export only what you need
2. **Use Presets:** Start with Default preset, customize as needed
3. **Check File Size:** Large exports may take time
4. **Safe Locations:** Export to Documents folder, not system folders
5. **Keep Summaries:** Export timestamp helps track changes

### For Developers

1. **Progress Reporting:** Always report progress for long operations
2. **Error Handling:** Catch and display user-friendly error messages
3. **UTF-8 Encoding:** Ensures international characters work correctly
4. **Streaming:** Consider streaming for very large datasets
5. **Validation:** Validate options before starting export
6. **Async/Await:** Keep UI responsive during export

---

## ?? Version History

**v1.0 (Phase 7):**
- ? CSV export
- ? JSON export
- ? XML export
- ? Export dialog
- ? Keyboard shortcuts
- ? Progress reporting

**Future Enhancements:**
- Excel export (EPPlus)
- PDF export
- HTML export
- Custom templates
- Export profiles

---

## ?? Troubleshooting

**Q: Export button is disabled**  
A: Ensure you have scanned files first. Export requires data to be available.

**Q: Can't save to selected location**  
A: Check folder permissions. Try saving to Documents folder instead.

**Q: Special characters appear wrong**  
A: File should be UTF-8 encoded. Try opening with Notepad++ or VS Code.

**Q: Excel shows garbled text**  
A: In Excel, use Data > From Text/CSV and select UTF-8 encoding.

**Q: Export seems slow**  
A: Large exports (1000+ files) may take 10-30 seconds. Progress bar shows status.

**Q: Where are my exported files?**  
A: Check the path you selected in Browse dialog, or check Documents folder.

---

**Quick Reference Last Updated:** December 2024  
**Phase 7 Status:** ? COMPLETE  
**Next Phase:** Phase 8 - Auto-Monitor & Background Processing
