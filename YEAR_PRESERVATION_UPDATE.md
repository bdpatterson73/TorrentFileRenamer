# Year Preservation Feature - Implementation Summary

## Overview
Updated the TV episode renaming logic to preserve and include the year component when present in the original filename. This ensures shows with multiple series across different years (e.g., "Robin Hood (2025)") maintain their year identifier.

## Changes Made

### 1. Core Models Updated
**Files Modified:**
- `TorrentFileRenamer.Core\Models\FileEpisode.cs`
- `TorrentFileRenamer.WinForms\FileEpisode.cs`
- `TorrentFileRenamer.WPF\Models\FileEpisodeModel.cs`

### 2. New Property Added
```csharp
public int? Year { get; set; }
```
Stores the extracted year from the original filename (e.g., 2025 from "Robin Hood 2025" or "Robin Hood (2025)")

### 3. New Method: ExtractYear()
Extracts year from the show name supporting two formats:
- **Parenthesized format**: `Robin Hood (2025)` ? extracts 2025
- **Standalone format**: `Robin Hood 2025` ? extracts 2025

Validates years are in the range 1900-2099 to avoid false positives.

### 4. Updated Parsing Logic
Year extraction is now called **before** the `CleanShowName()` method in all parsing methods:
- `TryParseSxxExxFormat()`
- `TryParseNumberxNumberFormat()`
- `TryParseSeasonEpisodeWordsFormat()`
- `TryParseEpisodeOnlyFormat()`

This ensures the year is captured before being removed by the cleaning process.

### 5. Updated Filename Generation
The `GenerateNewFileName()` method now includes the year in the output filename when available:

**With Year:**
```
Robin.Hood.(2025).S01E01.mp4
```

**Without Year:**
```
Robin.Hood.S01E01.mp4
```

### 6. WPF Model Enhancements
Added convenience properties to `FileEpisodeModel`:
- `Year` - Direct access to the year value
- `ShowNameWithYear` - Display property that formats as "Robin Hood (2025)"

## Example Transformations

### Before Update:
**Input:** `Robin.Hood.2025.S01E01.I.See.Him.720P.Web.X265-Minx.mp4`
**Output:** `Robin.Hood.S01E01.mp4`
*(Year was lost)*

### After Update:
**Input:** `Robin.Hood.2025.S01E01.I.See.Him.720P.Web.X265-Minx.mp4`
**Output:** `Robin.Hood.(2025).S01E01.mp4`
*(Year preserved)*

**Input:** `Robin.Hood.(2025).S01E01.I.See.Him.720P.Web.X265-Minx.mp4`
**Output:** `Robin.Hood.(2025).S01E01.mp4`
*(Year preserved from parentheses)*

## Folder Structure
When copied to the server, files will be organized as:
```
Output Directory/
??? Robin Hood (2025)/
    ??? Season 01/
   ??? Robin.Hood.(2025).S01E01.mp4
```

Note: The folder name "Robin Hood (2025)" may contain spaces which are preserved for folder names, while the filename uses dots as separators.

## Benefits
1. **Disambiguation** - Easily differentiate between different series with the same name
2. **Plex Compatibility** - Year in parentheses is a Plex-recommended format for disambiguating shows
3. **Backward Compatible** - Files without years continue to work as before
4. **Flexible Parsing** - Handles both `Show 2025` and `Show (2025)` formats

## Testing Recommendations
Test with the following filename patterns:
- `Robin.Hood.2025.S01E01.Episode.Name.mp4`
- `Robin.Hood.(2025).S01E01.Episode.Name.mp4`
- `Show.Name.1999.S01E01.mp4`
- `Show.Name.S01E01.mp4` (no year - should work as before)
- `Show.2020.1x01.mp4` (alternate format)

## Notes
- Episode titles are still dropped from the final filename (as per original design)
- The year extraction validates that years are between 1900-2099 to avoid false matches
- The `CleanShowName()` method still removes years, but only **after** they've been captured
