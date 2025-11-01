# File Copy Progress Enhancement

## Overview

Added comprehensive file copy progress feedback to the TorrentFileRenamer application. Users can now see real-time progress information including bytes transferred, transfer speed, and estimated time remaining when copying large media files to their Plex server.

## New Features Added

### 1. FileOperationProgress Class (`FileOperationProgress.cs`)
- **Purpose**: Provides file copy operations with detailed progress reporting
- **Key Features**:
  - Real-time byte transfer tracking
  - Transfer speed calculation (MB/s, GB/s, etc.)
  - Estimated time remaining
  - Automatic retry with progress feedback
  - Cancellation support
  - Automatic cleanup on errors or cancellation

### 2. Enhanced File Copy Operations
- **TV Episode Processing**: Now shows detailed progress during each file copy
- **Movie Processing**: Same progress feedback for movie file operations
- **Auto-Monitor Service**: Background file processing now includes progress updates

### 3. Progress Event Arguments (`FileProgressEventArgs`)
- **Detailed Information**:
  - `CopiedBytes` / `TotalBytes`: Raw byte counts
  - `PercentComplete`: Progress percentage
  - `SpeedBytesPerSecond`: Current transfer speed
  - `ElapsedTime`: Time spent copying
  - `FormattedSpeed`: Human-readable speed (e.g., "15.2 MB/s")
  - `FormattedProgress`: Human-readable progress (e.g., "256 MB / 1.2 GB (21.3%)")
  - `EstimatedTimeRemaining`: Calculated ETA
  - `FormattedTimeRemaining`: Human-readable ETA (e.g., "2m 35s")

## User Interface Improvements

### Main Form Updates
1. **Status Bar**: Now shows real-time copy progress with speed and ETA
2. **List View Items**: Individual files show progress percentage during copy
3. **Auto-Monitor**: Folder monitoring displays detailed progress for background copies

### Progress Information Display
```
Copying MovieName.mp4: 512 MB / 1.2 GB (42.7%) at 23.4 MB/s - ETA: 32s
```

## Benefits for Plex Server Users

### 1. **Visibility into Large File Transfers**
- See exactly how much data has been transferred
- Monitor transfer speeds to identify network bottlenecks
- Get accurate time estimates for completion

### 2. **Better User Experience**
- No more wondering if the application has frozen during large transfers
- Clear feedback on network performance
- Professional progress reporting similar to modern file managers

### 3. **Improved Reliability**
- Enhanced retry mechanism with progress awareness
- Automatic cleanup on cancellation or errors
- Better error reporting with context

## Technical Implementation

### Asynchronous File Operations
```csharp
var fileOperation = new FileOperationProgress();
fileOperation.ProgressChanged += (sender, args) => {
    UpdateUI($"Copying: {args.FormattedProgress} at {args.FormattedSpeed}");
};
bool success = await fileOperation.CopyFileWithRetryAsync(source, destination);
```

### Buffer Size Optimization
- Uses 1MB buffer by default for optimal performance with large files
- Configurable buffer size for different network conditions
- Progress updates every 100ms for smooth user experience

### Error Handling
- Graceful handling of network interruptions
- Automatic cleanup of partially copied files
- Detailed error reporting with retry context

## Usage Examples

### Manual Processing
When users click "Process" for TV episodes or movies:
1. Each file shows individual copy progress in the list
2. Status bar displays current file being copied with detailed stats
3. Overall progress bar shows completion across all files

### Auto-Monitor Mode  
When files are automatically detected and processed:
1. Monitoring status item updates with copy progress
2. Background processing includes full progress feedback
3. Completion notifications include transfer statistics

### Performance Metrics
For a 1.5 GB movie file on a typical network:
- **Real-time speed monitoring**: "copying at 45.2 MB/s"
- **Accurate progress**: "750 MB / 1.5 GB (50.0%)"  
- **Time estimates**: "ETA: 18s"

## Code Changes Made

1. **New Files**:
   - `FileOperationProgress.cs` - Core progress functionality
   - `FileProgressDemo.cs` - Demo/testing utilities

2. **Modified Files**:
   - `Form1.cs` - Updated UI to show progress information
   - `FolderMonitorService.cs` - Added progress events to auto-monitoring
   - Enhanced both TV and movie processing workflows

3. **Enhanced Capabilities**:
   - All file copy operations now include progress reporting
   - Better error handling with user-friendly messages
   - Consistent progress display across all features

## Future Enhancements

### Potential Improvements
1. **Network Performance Analysis**: Track and log transfer speeds over time
2. **Bandwidth Throttling**: Allow users to limit transfer speeds
3. **Parallel Processing**: Multiple file transfers with combined progress
4. **Disk Usage Monitoring**: Show available space during transfers
5. **Transfer History**: Log of completed transfers with performance metrics

This enhancement transforms the file copy experience from a "black box" operation into a transparent, informative process that keeps users informed about their large media file transfers to their Plex server.