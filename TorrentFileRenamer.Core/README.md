# TorrentFileRenamer.Core

This is the core business logic library for TorrentFileRenamer. It contains all the platform-independent code for parsing, validating, and processing media files for Plex compatibility.

## Features

### Media File Parsing
- **TV Episodes**: Supports multiple naming formats (SxxExx, xxYYY, Season/Episode words)
- **Movies**: Extracts movie names and years, organizes alphabetically
- **Multi-Episode Support**: Handles multi-episode files (S01E01-E02, S01E01E02E03)

### Validation & Compatibility
- **Plex Compatibility**: Validates and auto-fixes filenames for Plex Media Server
- **Path Validation**: Checks source/destination paths, disk space, permissions
- **Network Path Support**: Handles UNC paths and network shares

### Auto-Monitoring
- **Folder Watching**: Monitors folders for new media files
- **File Stability Detection**: Waits for files to finish downloading/copying
- **Automatic Processing**: Processes and moves files automatically
- **Progress Tracking**: Reports copy progress, speed, and ETA

### Utilities
- **File Hashing**: CRC32 implementation for file verification
- **Progress Reporting**: Detailed copy progress with speed and time estimates
- **Logging**: Comprehensive logging with file rotation

## Project Structure

```
TorrentFileRenamer.Core/
??? Models/
?   ??? FileEpisode.cs       # TV episode data and parsing
?   ??? MovieFile.cs              # Movie file data and parsing
??? Services/
?   ??? MediaTypeDetector.cs      # TV vs Movie detection
?   ??? PathValidator.cs       # Path validation utilities
?   ??? PlexCompatibilityValidator.cs  # Plex naming validation
?   ??? FolderMonitorService.cs   # Auto-monitoring service
??? Configuration/
?   ??? AppSettings.cs            # Settings, logging, preferences
??? Utilities/
    ??? CRC32.cs      # Hash algorithm
  ??? HashHelper.cs  # File hashing utilities
    ??? FileOperationProgress.cs  # File operations with progress
```

## Dependencies

- **NExifTool** (0.11.0) - Media metadata extraction
- **System.IO.Compression** (4.3.0) - Archive handling
- **System.IO.Hashing** (7.0.0) - File verification

## Target Framework

- .NET 8.0

## Usage Example

### Parse a TV Episode
```csharp
using TorrentFileRenamer.Core.Models;

var episode = new FileEpisode(
  @"C:\Downloads\Show.Name.S01E05.720p.mkv",
    @"C:\Media\TV Shows"
);

Console.WriteLine($"Show: {episode.ShowName}");
Console.WriteLine($"Season: {episode.SeasonNumber}");
Console.WriteLine($"Episode: {episode.EpisodeNumber}");
Console.WriteLine($"New Path: {episode.NewFileNamePath}");
```

### Validate for Plex
```csharp
using TorrentFileRenamer.Core.Services;

var validation = PlexCompatibilityValidator.ValidateTVEpisode(
    "Show Name", 1, 5, "Show.Name.S01E05.mkv", 
    @"C:\Media\TV Shows\Show Name\Season 1\Show.Name.S01E05.mkv"
);

if (!validation.IsValid)
{
    foreach (var issue in validation.Issues)
  Console.WriteLine($"Issue: {issue}");
}
```

### Monitor a Folder
```csharp
using TorrentFileRenamer.Core.Services;

var monitor = new FolderMonitorService
{
    WatchFolder = @"C:\Downloads",
    DestinationFolder = @"C:\Media\TV Shows",
    FileExtensions = new[] { ".mkv", ".mp4", ".avi" },
    StabilityDelaySeconds = 30
};

monitor.FileProcessed += (s, e) => 
{
    Console.WriteLine($"{e.FilePath}: {e.Message}");
};

monitor.StartMonitoring();
```

## Design Principles

1. **UI-Agnostic**: No dependencies on WinForms or WPF
2. **Testable**: Business logic separated from presentation
3. **Reusable**: Can be used by any .NET application
4. **Event-Driven**: Services expose events for UI integration
5. **Async-Ready**: Supports async/await for long-running operations

## License

Same as parent project (TorrentFileRenamer)
