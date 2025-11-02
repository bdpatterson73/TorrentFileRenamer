# Auto-Monitor User Guide

## Welcome to Automatic File Processing! ??

This guide will help you set up and use the Auto-Monitor feature to automatically organize your TV show downloads.

---

## What is Auto-Monitor?

Auto-Monitor watches a folder on your computer for new TV show files. When it finds one, it automatically:
1. Waits for the file to finish downloading
2. Figures out the show name, season, and episode
3. Renames it in a Plex-friendly format
4. Moves it to your TV library folder
5. Logs everything so you can see what happened

Think of it as a helpful robot that keeps your media library organized while you sleep! ??

---

## Quick Start (3 Easy Steps)

### Step 1: Open Auto-Monitor
1. Launch TorrentFileRenamer
2. Click the **"Auto Monitor"** tab
   - OR press **Ctrl+3** on your keyboard

### Step 2: Configure
1. Click the **"Configure"** button
2. Set your **Watch Folder** (where files download to)
   - Example: `C:\Downloads\TV Shows`
3. Set your **Destination Folder** (your Plex library)
   - Example: `D:\Plex Library\TV Shows`
4. Click **OK** to save

### Step 3: Start Monitoring
1. Click **"Start Monitoring"**
2. Done! The status indicator will turn green ??

Now Auto-Monitor is running. Any TV show files that appear in your watch folder will be processed automatically!

---

## Configuration Explained

### Watch Folder
**What it is:** The folder Auto-Monitor watches for new files.

**Best Practice:**
- Use a dedicated folder for TV show downloads
- Don't put movies in this folder (they'll be skipped)
- Make sure your torrent client downloads to this folder

**Example:**
```
C:\Users\YourName\Downloads\TV Shows\
```

### Destination Folder
**What it is:** Where Auto-Monitor moves organized files.

**Best Practice:**
- Point this to your Plex TV Shows library
- Make sure you have enough disk space
- Don't use the same folder as Watch Folder

**Example:**
```
D:\Plex Library\TV Shows\
```

### File Extensions
**What it is:** Types of video files to monitor.

**Default:** `*.mp4;*.mkv;*.avi;*.m4v`

**When to change:**
- If you use other video formats (e.g., `*.wmv`)
- Separate multiple extensions with semicolons (`;`)
- Always start with asterisk and dot (`*.`)

**Example:**
```
*.mp4;*.mkv;*.avi;*.m4v;*.mov
```

### Stability Delay
**What it is:** How long (in seconds) Auto-Monitor waits before processing a file.

**Default:** 30 seconds

**Why it matters:**
- Files being downloaded can't be processed
- Stability delay waits until download finishes
- If files are still processing too early, increase this

**Recommendations:**
- **Fast downloads (100+ MB/s):** 20-30 seconds
- **Normal downloads (10-50 MB/s):** 30-60 seconds
- **Slow downloads (<10 MB/s):** 60-120 seconds
- **Network drives:** 60-120 seconds

### Auto-start on Load
**What it is:** Automatically start monitoring when you open the app.

**Use this if:**
- You want "set and forget" operation
- Your computer is always on
- You download files regularly

**Don't use this if:**
- You manually control when to process files
- You download to different folders sometimes
- You want to review files before processing

---

## Understanding the Interface

### Status Indicator

The status indicator shows Auto-Monitor's current state:

| Color | Status | Meaning |
|-------|--------|---------|
| ?? **Red** | Error | Something went wrong |
| ?? **Yellow** | Starting/Stopping | Changing state |
| ?? **Green** | Running | Actively monitoring |
| ? **Gray** | Stopped | Not monitoring |

### Activity Log

The activity log shows a real-time list of everything Auto-Monitor does:

**Columns:**
- **Time:** When it happened
- **File:** The file name
- **Activity:** What happened (Found, Completed, Failed, etc.)
- **Status:** Success or Failed
- **Message:** Details about what happened

**Color Coding:**
- **White rows:** Successful operations
- **Red rows:** Errors or failures

**Tips:**
- Click column headers to sort
- Scroll to see older entries
- Use "Clear Log" to start fresh

---

## Common Workflows

### Scenario 1: First Time Setup

1. **Install and launch** TorrentFileRenamer
2. **Click** Auto Monitor tab (Ctrl+3)
3. **Click** Configure button
4. **Browse** to your downloads folder
5. **Browse** to your Plex library folder
6. **Check** "Auto-start on load" if desired
7. **Click** OK
8. **Click** "Start Monitoring"
9. **Done!** You're all set

### Scenario 2: Testing Auto-Monitor

1. **Start monitoring** as above
2. **Copy** a TV show file into your watch folder
3. **Watch** the activity log - you should see:
   - "File detected" within 1 second
   - "Processing..." after stability delay
   - "Completed" when done
4. **Check** your destination folder - file should be there!

### Scenario 3: Daily Use

**With Auto-start enabled:**
- Just launch the app
- Auto-Monitor starts automatically
- Downloads are processed as they complete
- Review activity log periodically

**Without Auto-start:**
- Launch the app
- Go to Auto Monitor tab
- Click "Start Monitoring"
- Let it run in the background

### Scenario 4: Stopping Monitoring

1. **Go to** Auto Monitor tab
2. **Click** "Stop Monitoring"
3. **Wait** for status to show "Stopped"
4. Now you can safely close the app

---

## What Gets Processed?

### ? WILL Process

**TV Shows with clear episode info:**
```
Breaking.Bad.S01E01.Pilot.1080p.mkv
? Breaking Bad\Season 01\Breaking Bad - S01E01 - Pilot.mkv

The.Office.US.S03E15.720p.mp4
? The Office US\Season 03\The Office US - S03E15.mp4

Game.of.Thrones.S08E06.FINAL.2160p.mkv
? Game of Thrones\Season 08\Game of Thrones - S08E06.mkv
```

**Multi-episode files:**
```
Friends.S02E23E24.720p.mkv
? Friends\Season 02\Friends - S02E23E24.mkv
```

### ? WON'T Process

**Movies:**
```
Avatar.2009.1080p.mkv  ? Detected as movie, skipped
Inception.2010.720p.mkv  ? Detected as movie, skipped
```

**Files without episode info:**
```
random_video.mkv  ? Can't determine show/episode
vacation_footage.mp4  ? Not a TV show
```

**Non-video files:**
```
document.pdf  ? Not a video file
music.mp3  ? Not a video file
```

**Plex compatibility issues:**
- Files with year in episode position
- Files with ambiguous naming
- Files that don't follow standard formats

---

## Reading the Activity Log

### Example Log Entries

**Successful Processing:**
```
Time      File    Activity  Status   Message
2024-12-14 10:30:15  Breaking.Bad.S01E01.mkv     Found     Success  File detected: Created
2024-12-14 10:30:45  Breaking.Bad.S01E01.mkv       Started   Success  Processing: Breaking.Bad.S01E01.mkv
2024-12-14 10:31:30  Breaking.Bad.S01E01.mkv       Completed Success  Successfully moved to: D:\Plex\TV Shows\Breaking Bad\Season 01\Breaking Bad - S01E01.mkv
```

**Failed Processing (Parse Error):**
```
Time            File          Activity  Status  Message
2024-12-14 10:35:20  random_file.mkv       Found     Success File detected: Created
2024-12-14 10:35:50  random_file.mkv       Failed    Failed  Could not parse episode information
```

**Failed Processing (Destination Exists):**
```
Time      File        Activity  Status  Message
2024-12-14 10:40:10  The.Office.S02E01.mkv    Found     Success File detected: Created
2024-12-14 10:40:40  The.Office.S02E01.mkv    Failed    Failed  Destination file already exists
```

**System Events:**
```
Time          File     Activity  Status   Message
2024-12-14 10:30:00  System   Started   Success  Folder monitoring started successfully
2024-12-14 11:00:00  System   Stopped   Success  Folder monitoring stopped
2024-12-14 11:05:00  System   Config    Success  Configuration updated
```

---

## Troubleshooting

### Problem: Monitoring won't start

**Symptom:** Start button is grayed out or clicking does nothing

**Solutions:**
1. Check Watch Folder is set and exists
2. Check Destination Folder is set and valid
3. Make sure both folders are different
4. Check you have permissions to access both folders

### Problem: Files aren't being detected

**Symptom:** Copy file to watch folder, but nothing happens

**Solutions:**
1. Check file extension matches configuration (e.g., `.mkv`)
2. Make sure file is a TV show, not a movie
3. Check activity log for "Found" entry
4. Verify monitoring status is green (Running)

### Problem: Files process too quickly

**Symptom:** Files processed while still downloading

**Solutions:**
1. Increase stability delay in configuration
2. Try 60-120 seconds instead of 30
3. For network drives, use 120-180 seconds

### Problem: Processing fails with "Could not parse"

**Symptom:** File detected but fails to process

**Solutions:**
1. Check filename has format like `ShowName.S##E##.mkv`
2. Rename file to include season/episode numbers
3. Use format: `Show Name - S01E01.mkv`
4. Avoid special characters in filename

### Problem: "Destination file already exists"

**Symptom:** File skipped because destination exists

**Solutions:**
1. Check if file was already processed
2. Delete duplicate from destination folder
3. Or rename the new file before processing
4. Check if you accidentally set duplicate watch folders

### Problem: Performance issues

**Symptom:** App slow or unresponsive

**Solutions:**
1. Clear activity log (keeps only recent entries)
2. Close other applications
3. Check disk space on destination drive
4. Reduce max log entries in configuration

---

## Tips & Best Practices

### ?? Setup Tips

1. **Test first:** Start with one file to verify everything works
2. **Separate folders:** Don't use same folder for watch and destination
3. **Backup configuration:** Note your settings in case of reset
4. **Start small:** Begin with recent downloads, add older files gradually

### ?? Folder Organization

1. **Dedicated folders:** Use separate folders for TV shows and movies
2. **Avoid nesting:** Don't put watch folder inside destination folder
3. **Network drives:** Use local drives when possible for better performance
4. **Permissions:** Ensure read/write access to both folders

### ? Performance Tips

1. **Local drives:** Process faster than network drives
2. **SSD destination:** Faster file operations
3. **Limit log entries:** Keep activity log under 100 entries
4. **Close when not needed:** Stop monitoring if not downloading

### ??? Safety Tips

1. **Test destination:** Make sure it's your Plex folder, not system folder
2. **Backup first:** Keep backups of important files
3. **Review logs:** Check activity log for errors regularly
4. **Monitor disk space:** Ensure enough space before processing large files

### ?? Naming Tips

**Good filenames (will process successfully):**
```
Breaking.Bad.S05E16.1080p.mkv
The.Mandalorian.S02E08.2160p.mkv
Friends.S01E01.Pilot.720p.mkv
Game.of.Thrones.S08E06.mkv
```

**Bad filenames (may fail):**
```
bb_s5e16.mkv ? Too abbreviated
TheMandalorian-208.mkv          ? Wrong episode format
Friends 1x01.mkv     ? Use S01E01 format
got_final_episode.mkv    ? No episode number
```

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **Ctrl+3** | Open Auto Monitor tab |
| **Ctrl+,** | Open Settings/Configuration |
| **F1** | Show keyboard shortcuts |
| **F5** | Refresh view |
| **Escape** | Close dialog |
| **Enter** | Confirm in dialog |

---

## Frequently Asked Questions

### Q: Does Auto-Monitor work when the app is closed?
**A:** No. The app must be running for Auto-Monitor to work. However, it can run minimized to the system tray (future feature).

### Q: Can I monitor multiple folders?
**A:** Currently, only one watch folder is supported. Multiple folders is a planned feature.

### Q: What happens if I have the same show in different quality?
**A:** Auto-Monitor will process each file. If the destination already exists, the new file is skipped. You'll need to manually decide which quality to keep.

### Q: Can Auto-Monitor handle movies?
**A:** Not yet. Currently only TV shows are supported. Movie support is planned for a future update.

### Q: Does it delete the original file?
**A:** Yes, after a successful copy and verification, the original file in the watch folder is deleted. The processed file remains in the destination folder.

### Q: What if processing fails?
**A:** The original file stays in the watch folder. Check the activity log for the error message and fix the issue. You can manually process it later using the TV Episodes or Movies tab.

### Q: Can I undo a processed file?
**A:** Not automatically. You'll need to manually move the file back and rename it if needed.

### Q: How much disk space do I need?
**A:** Temporarily, you need space for both the original and the copy. After processing, the original is deleted. For a 5GB file, you briefly need 10GB free, then back to 5GB.

### Q: Is it safe to process while Plex is scanning?
**A:** Yes. However, Plex may not see the new files until its next library scan. Some Plex servers auto-scan, others need manual refresh.

### Q: Can I schedule Auto-Monitor to run only at certain times?
**A:** Not yet. Scheduled monitoring is planned for a future update. For now, manually start/stop as needed.

---

## Getting Help

### In the App
- Press **F1** for keyboard shortcuts
- Check the **Activity Log** for error details
- Review **Status Message** at bottom of screen

### Documentation
- **Quick Reference:** Technical code examples
- **Completion Report:** Detailed feature documentation
- **This Guide:** User-friendly instructions

### Support
- Check the activity log first
- Review this guide's Troubleshooting section
- Check that your file names follow the right format
- Verify folder permissions and disk space

---

## What's Next?

### Planned Features
- Movie auto-processing
- Multiple watch folder profiles
- Scheduled monitoring times
- Duplicate file detection
- Notifications
- Remote folder monitoring
- Mobile companion app

### Stay Updated
- Check for app updates regularly
- Review release notes for new features
- Provide feedback on features you'd like to see

---

## Summary

**Auto-Monitor makes your life easier by:**
- ? Automatically organizing TV show files
- ? Using Plex-friendly naming
- ? Waiting for downloads to complete
- ? Logging everything for transparency
- ? Running in the background while you do other things

**Just remember:**
1. Set your watch and destination folders
2. Start monitoring
3. Let it run
4. Check activity log occasionally
5. Enjoy your organized media library!

---

**Happy monitoring! ????**

If you have questions or issues, check the Troubleshooting section above or review the activity log for detailed error messages.

---

*User Guide Last Updated: December 2024*  
*TorrentFileRenamer Version: Phase 8*
