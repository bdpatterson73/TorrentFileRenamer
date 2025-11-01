# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TorrentFileRenamer is a Windows Forms (.NET 8) application that automatically renames and organizes torrent-downloaded media files to be compatible with Plex Media Server. The application can distinguish between TV episodes and movies, applying appropriate naming conventions and folder structures.

## Build and Development Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Publish for release
dotnet publish

# Watch mode for development (auto-rebuild on changes)
dotnet watch run --project TorrentFileRenamer.sln
```

## Architecture Overview

### Core Components

- **frmMain (Form1.cs)**: Main application window and entry point
- **FileEpisode.cs**: TV episode file processing and renaming logic
- **MovieFile.cs**: Movie file processing and renaming logic
- **FolderMonitorService.cs**: Automatic folder monitoring for new files
- **PlexCompatibilityValidator.cs**: Validates naming conventions for Plex compatibility
- **MediaTypeDetector.cs**: Distinguishes between TV episodes and movies
- **AppSettings.cs**: Application configuration and settings persistence

### UI Forms

- **frmMain**: Primary interface for manual file processing
- **frmFolderMonitor**: Configuration for automatic folder monitoring
- **frmScanMovies**: Movie-specific scanning options
- **frmScanOptions**: General scanning and processing options

### Key Features

1. **Media Type Detection**: Automatically identifies TV episodes vs movies using regex patterns
2. **Plex Compatibility**: Ensures file/folder names meet Plex Media Server requirements
3. **Automatic Monitoring**: Watches folders for new files and processes them automatically
4. **Multi-Episode Support**: Handles multi-episode files (e.g., S01E01-E02)
5. **Settings Persistence**: Saves user preferences and folder configurations

### File Processing Flow

1. Input file is analyzed by `MediaTypeDetector` to determine if it's a TV episode or movie
2. For TV episodes: `FileEpisode` class parses season/episode information and creates Plex-compatible names
3. For movies: `MovieFile` class extracts title and year information
4. `PlexCompatibilityValidator` ensures the final names meet Plex requirements
5. Files are moved/renamed to the appropriate destination folder structure

### Dependencies

- **NExifTool**: For metadata extraction
- **System.IO.Compression**: For archive handling
- **System.IO.Hashing**: For file integrity verification

### Project Structure

```
├── Core Classes/
│   ├── FileEpisode.cs          # TV episode processing
│   ├── MovieFile.cs            # Movie processing
│   ├── MediaTypeDetector.cs    # Media type identification
│   └── PlexCompatibilityValidator.cs # Plex naming validation
├── Services/
│   ├── FolderMonitorService.cs # Automatic monitoring
│   └── AppSettings.cs          # Configuration management
├── UI Forms/
│   ├── Form1.cs (frmMain)      # Main interface
│   ├── frmFolderMonitor.cs     # Monitor configuration
│   ├── frmScanMovies.cs        # Movie scanning
│   └── frmScanOptions.cs       # General options
└── Utilities/
    ├── CRC32.cs                # File integrity
    ├── HashHelper.cs           # Hash calculations
    └── PathValidator.cs        # Path validation
```

## Development Notes

- Target framework: .NET 8.0 Windows
- Application type: Windows Forms with manifest
- The application uses Windows-specific APIs for short path names (`kernel32.dll`)
- Logging is implemented via `LoggingService` with daily log files
- Settings are persisted to JSON files in the user's local application data folder