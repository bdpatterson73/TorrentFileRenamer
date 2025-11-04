# Auto-Scroll During Processing Feature

## Overview
Implemented automatic scrolling to keep the currently processing item in view during TV episode and movie processing operations. This enhances user experience by making it easy to see which file is currently being processed.

## Changes Made

### 1. New Behavior: ScrollToItemBehavior.cs
**Location:** `TorrentFileRenamer.WPF\Behaviors\ScrollToItemBehavior.cs`

Created a reusable WPF behavior that automatically scrolls an `ItemsControl` to bring a specific item into view.

**Key Features:**
- Automatically centers the target item in the viewport
- Handles cases where the item container hasn't been generated yet
- Works with any `ItemsControl` (including those in `ScrollViewer`)
- Smooth scrolling experience
- Gracefully handles errors

**Usage:**
```xml
<ItemsControl ItemsSource="{Binding Items}">
    <i:Interaction.Behaviors>
        <behaviors:ScrollToItemBehavior TargetItem="{Binding CurrentProcessingItem}" />
    </i:Interaction.Behaviors>
</ItemsControl>
```

### 2. TvEpisodesViewModel Updates
**Location:** `TorrentFileRenamer.WPF\ViewModels\TvEpisodesViewModel.cs`

**Added:**
- `CurrentProcessingEpisode` property to track the currently processing item
- Property binding updates in `ProcessAsync` method:
  - Sets `CurrentProcessingEpisode` as each file is processed
  - Clears `CurrentProcessingEpisode` when processing completes

**Implementation:**
```csharp
// New property
public FileEpisodeModel? CurrentProcessingEpisode { get; set; }

// In ProcessAsync - update on progress
var overallProgress = new Progress<(int current, int total)>(p =>
{
    // ... existing code ...
    
    // Update the current processing episode for auto-scroll
  if (p.current > 0 && p.current <= episodesToProcess.Count)
    {
var currentEpisode = episodesToProcess[p.current - 1];
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            CurrentProcessingEpisode = currentEpisode;
        });
    }
});

// After processing completes
CurrentProcessingEpisode = null;
```

### 3. MoviesViewModel Updates
**Location:** `TorrentFileRenamer.WPF\ViewModels\MoviesViewModel.cs`

**Added:**
- `CurrentProcessingMovie` property to track the currently processing item
- Property binding updates in `ProcessAsync` method:
  - Sets `CurrentProcessingMovie` as each file is processed
  - Clears `CurrentProcessingMovie` when processing completes

**Implementation:**
```csharp
// New property
public MovieFileModel? CurrentProcessingMovie { get; set; }

// In ProcessAsync - update on progress
var overallProgress = new Progress<(int current, int total)>(p =>
{
    // ... existing code ...
    
    // Update the current processing movie for auto-scroll
    if (p.current > 0 && p.current <= moviesToProcess.Count)
    {
        var currentMovie = moviesToProcess[p.current - 1];
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
    {
            CurrentProcessingMovie = currentMovie;
        });
    }
});

// After processing completes
CurrentProcessingMovie = null;
```

### 4. TvEpisodesView.xaml Updates
**Location:** `TorrentFileRenamer.WPF\Views\TvEpisodesView.xaml`

**Changes:**
- Added `ScrollToItemBehavior` to Card View ItemsControl
- Added `ScrollToItemBehavior` to Compact View ItemsControl
- Both behaviors bind to `CurrentProcessingEpisode` property

**Example:**
```xml
<ItemsControl ItemsSource="{Binding Episodes}" Margin="16">
    <i:Interaction.Behaviors>
        <behaviors:ScrollToItemBehavior TargetItem="{Binding CurrentProcessingEpisode}" />
    </i:Interaction.Behaviors>
    <!-- ... rest of ItemsControl ... -->
</ItemsControl>
```

### 5. MoviesView.xaml Updates
**Location:** `TorrentFileRenamer.WPF\Views\MoviesView.xaml`

**Changes:**
- Added `ScrollToItemBehavior` to Card View ItemsControl
- Added `ScrollToItemBehavior` to Compact View ItemsControl
- Both behaviors bind to `CurrentProcessingMovie` property

**Example:**
```xml
<ItemsControl ItemsSource="{Binding Movies}" Margin="16">
    <i:Interaction.Behaviors>
        <behaviors:ScrollToItemBehavior TargetItem="{Binding CurrentProcessingMovie}" />
    </i:Interaction.Behaviors>
 <!-- ... rest of ItemsControl ... -->
</ItemsControl>
```

## How It Works

1. **During Processing:**
   - As each file is processed, the `ProcessAsync` method updates the `CurrentProcessingEpisode` or `CurrentProcessingMovie` property
   - This happens in the `overallProgress` callback when the file index changes

2. **Auto-Scroll Trigger:**
   - When `CurrentProcessingEpisode`/`CurrentProcessingMovie` changes, the `ScrollToItemBehavior` is notified
   - The behavior finds the visual container for that item
   - It calculates the scroll position to center the item in the viewport
   - The `ScrollViewer` smoothly scrolls to that position

3. **After Processing:**
   - The `CurrentProcessingEpisode`/`CurrentProcessingMovie` is cleared (set to null)
   - This prevents unwanted scrolling after processing completes

## View Modes Supported

The auto-scroll feature works in:
- ? **Card View** - Full card layout with all details
- ? **Compact View** - Condensed card layout
- ? **Grid View** - DataGrid has its own built-in scrolling behavior

## Benefits

1. **Better Visibility:** Users can always see which file is currently being processed
2. **Progress Tracking:** Easy to monitor progress through a long list of files
3. **User Experience:** No need to manually scroll to find the current item
4. **Smooth Animation:** Natural scrolling behavior that centers the item
5. **Non-Intrusive:** Only scrolls during active processing, doesn't interfere with user interaction otherwise

## Technical Details

### Thread Safety
- Uses `Dispatcher.Invoke` to ensure UI updates happen on the UI thread
- Safe to call from background processing tasks

### Performance
- Uses `BeginInvoke` with `DispatcherPriority.Background` to avoid blocking the UI
- Minimal overhead as it only scrolls when the current item changes
- Gracefully handles cases where containers aren't generated yet

### Compatibility
- Works with both TV Episodes and Movies
- Compatible with Card and Compact view modes
- No changes needed for Grid view (DataGrid handles its own scrolling)

## Testing Recommendations

1. **Process a long list** of files (20+) to see the auto-scrolling in action
2. **Test both Card and Compact views** to ensure scrolling works in both
3. **Try switching views** during processing to ensure no errors occur
4. **Cancel processing mid-way** to verify the current item is cleared properly
5. **Test with different screen sizes** to ensure centering works correctly

## Future Enhancements

Possible improvements for future versions:
- Add option to disable auto-scrolling in settings
- Add smooth animation easing for scroll transitions
- Support for grid view auto-scrolling
- Highlight the currently processing item with a subtle visual indicator
