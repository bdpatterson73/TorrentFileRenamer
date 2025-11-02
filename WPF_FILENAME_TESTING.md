# WPF Filename Parsing Test Suite

## Overview
Added comprehensive test code to the **WPF application** to verify TV episode filename parsing, especially the new year preservation feature.

## Changes Made

### 1. Added to `TorrentFileRenamer.Core\Models\FileEpisode.cs`

#### New Method: `RunAllTests()`
A comprehensive static method that tests 17 different filename formats:

**Test Categories:**
- **Year Tests** (4 tests)
  - Year in filename: `Robin.Hood.2025.S01E01.mp4`
  - Year in parentheses: `Robin.Hood.(2025).S01E01.mp4`
  - Classic year 1999
  - Year with 1x01 format

- **Standard Formats** (2 tests)
  - Standard S01E01 format
  - Standard 1x01 format

- **Episode Descriptions** (2 tests)
  - With long episode description
  - Without episode description

- **Multi-Episode** (2 tests)
  - S01E01-E02 format
  - S01E01E02 format

- **Special Cases** (3 tests)
  - Episode 0 (pilot/special)
  - Season Episode word format
  - With special characters

- **Quality Indicators** (2 tests)
  - 4K HDR format
  - Multiple quality tags

- **Edge Cases** (2 tests)
  - Spaces in S E format
  - Year and parentheses mixed (Doctor Who)

### 2. Modified `TorrentFileRenamer.WPF\App.xaml.cs`

Added test execution in `OnStartup`:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    // Run filename parsing tests in DEBUG mode
    #if DEBUG
    RunFilenameParsingTests();
    #endif

    // ... rest of startup code
}

private void RunFilenameParsingTests()
{
    try
    {
     // Run the comprehensive test suite
        FileEpisode.RunAllTests(@"C:\TestOutput");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error running filename tests: {ex.Message}");
    }
}
```

## How to Use

### Running the Tests

1. **Build in DEBUG mode** - Tests only run in DEBUG configuration
2. **Start the WPF application**
3. **Check the console output** - The test results will be displayed in the console window

### Test Output Format

Each test displays:
```
Test #1: Year in filename (2025)
????????????????????????????????????????????????????????????????????????????????
  ?? Original: Robin.Hood.2025.S01E01.I.See.Him.720P.Web.X265-Minx.mp4
  ?? Show:Robin Hood (2025)
  ?? Season:   1
  ?? Episode:  1
  ? Result:   Robin.Hood.(2025).S01E01.mp4
  ? SUCCESS
```

### Color Coding
- **Green (?)** - Successfully parsed
- **Red (?)** - Failed to parse or error occurred

### Example Results for Year Tests

**Input:** `Robin.Hood.2025.S01E01.I.See.Him.720P.Web.X265-Minx.mp4`
**Output:** `Robin.Hood.(2025).S01E01.mp4`
- ? Year preserved
- ? Episode title removed
- ? Quality tags removed
- ? Format standardized

**Input:** `Robin.Hood.(2025).S01E01.I.See.Him.720P.Web.X265-Minx.mp4`
**Output:** `Robin.Hood.(2025).S01E01.mp4`
- ? Year already in parentheses preserved
- ? Consistent output format

## Verification Checklist

Use the test output to verify:

- [ ] Years are extracted and preserved
- [ ] Years in parentheses are recognized
- [ ] Shows without years work correctly
- [ ] Episode descriptions are removed
- [ ] Quality indicators (720p, 1080p, etc.) are removed
- [ ] Multi-episode files are handled correctly
- [ ] Special episode 0 works
- [ ] Various formats (S01E01, 1x01, Season 1 Episode 1) all work
- [ ] Special characters are handled properly

## Removing the Tests

When you're satisfied with the parsing logic:

1. **Option 1:** Remove the `#if DEBUG` block from `App.xaml.cs`
2. **Option 2:** Keep it for future debugging (recommended)
3. **Option 3:** Comment out the `RunFilenameParsingTests()` call

The tests have zero impact in RELEASE builds.

## Debug Output

In addition to console output, detailed parsing information is written to the Debug output window showing:
- Parsing pattern matched
- Extracted show name
- Year extraction
- Clean-up steps
- Final generated filename

## Notes

- Tests run only once at application startup
- All tests use `C:\TestOutput` as the output directory (doesn't need to exist)
- No actual files are created or modified
- Tests complete in under 1 second
- Console window stays open until you press a key (see test code)
