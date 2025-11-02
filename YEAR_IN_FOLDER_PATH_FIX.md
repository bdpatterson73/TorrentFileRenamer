# Year in Folder Path Fix

## Issue
The destination folder path was not including the year even though the filename correctly included it.

**Before:**
- Filename: `Robin.Hood.(2025).S01E01.mp4` ?
- Folder: `Z:\TV\ROBIN HOOD\Season 1\` ?

**After:**
- Filename: `Robin.Hood.(2025).S01E01.mp4` ?
- Folder: `Z:\TV\Robin Hood (2025)\Season 1\` ?

## Root Cause
In the `FileEpisode` constructor, the `NewFileNamePath` and `NewDirectoryName` were being constructed using `ShowName` directly after the year had been extracted and removed from it. The year was preserved in the `Year` property but wasn't being used when building the folder paths.

## Solution
Modified the constructor to create a `showFolderName` variable that includes the year when available:

```csharp
// Build paths with year if available - folder name includes year for disambiguation
string showFolderName = Year.HasValue ? $"{ShowName} ({Year.Value})" : ShowName;

NewFileNamePath = OutputDirectory + "\\" + showFolderName + "\\" + "Season " + SeasonNumber.ToString() + "\\" + NewFileName;
NewDirectoryName = OutputDirectory + "\\" + showFolderName + "\\" + "Season " + SeasonNumber.ToString() + "\\";
```

## Files Modified

### TorrentFileRenamer.Core\Models\FileEpisode.cs
1. **Constructor** - Updated path construction to use `showFolderName` with year
2. **ValidateForPlexCompatibility method** - Updated path regeneration in Plex auto-fix to include year

## Benefits
1. **Disambiguation** - Different series with the same name are now stored in separate folders
2. **Consistency** - Folder name matches the filename format
3. **Plex Compatibility** - Plex recognizes the year in folder names for better metadata matching
4. **Organization** - Easier to distinguish between reboots and remakes

## Example Results

### Robin Hood (2025)
- **Source:** `Robin.Hood.2025.S01E01.I.See.Him.720P.Web.X265-Minx.mp4`
- **Destination:** `Z:\TV\Robin Hood (2025)\Season 1\Robin.Hood.(2025).S01E01.mp4`

### Doctor Who (2005)
- **Source:** `Doctor.Who.(2005).S01E01.Rose.720p.mp4`
- **Destination:** `Z:\TV\Doctor Who (2005)\Season 1\Doctor.Who.(2005).S01E01.mp4`

### Show Without Year
- **Source:** `Modern.Show.S01E01.Episode.Title.mp4`
- **Destination:** `Z:\TV\Modern Show\Season 1\Modern.Show.S01E01.mp4`

## Notes
- The folder name includes the year in parentheses for consistency with Plex naming standards
- Shows without years continue to work normally (no year in folder or filename)
- The change applies to both initial path construction and Plex validation auto-fix
- WinForms version will need manual update (edit was blocked by content filter)
