using System.Text.RegularExpressions;
using System.Diagnostics;
using static System.Windows.Forms.Design.AxImporter;
using System.Runtime.InteropServices;
using System.Text;
//using TMDbLib.Client;
//using TMDbLib.Objects.General;
//using TMDbLib.Objects.Authentication;
//using TMDbLib.Objects.Movies;
//using TMDbLib.Objects.Reviews;
//using TMDbLib.Objects.Search;
//using TMDbLib.Rest;
//using TMDbLib.Utilities;
//using Credits = TMDbLib.Objects.Movies.Credits;

namespace TorrentFileRenamer
{
    public partial class frmMain : Form
    {
        private static string videoFolder = "";
        const int MAX_PATH = 255;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath,
       int shortPathLength
       );

        // Folder monitoring service and settings
        private FolderMonitorService? _folderMonitorService;
        private ListViewItem? _monitoringStatusItem;
        private string _savedWatchFolder = "";
        private string _savedDestinationFolder = "";
        private string _savedFileExtensions = "*.mp4;*.mkv;*.avi;*.m4v";
        private int _savedStabilityDelay = 30;
        private bool _savedAutoStart = false;
        private int _savedMaxAutoMonitorLogEntries = 20;

        public frmMain()
        {
            InitializeComponent();
            ConfigureModernUI();
        }

        private void ConfigureModernUI()
        {
            // Enable modern look
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            // Configure ListView alternating row colors
            ConfigureListViewAppearance();
        }

        private void ConfigureListViewAppearance()
        {
            // Configure TV ListView
            lvFiles.OwnerDraw = true;
            lvFiles.DrawItem += ListView_DrawItem;
            lvFiles.DrawSubItem += ListView_DrawSubItem;
            lvFiles.DrawColumnHeader += ListView_DrawColumnHeader;

            // Configure Movies ListView
            lvMovies.OwnerDraw = true;
            lvMovies.DrawItem += ListView_DrawItem;
            lvMovies.DrawSubItem += ListView_DrawSubItem;
            lvMovies.DrawColumnHeader += ListView_DrawColumnHeader;

            // Configure Movie Cleaner ListView
            lvMovieCleaner.OwnerDraw = true;
            lvMovieCleaner.DrawItem += ListView_DrawItem;
            lvMovieCleaner.DrawSubItem += ListView_DrawSubItem;
            lvMovieCleaner.DrawColumnHeader += ListView_DrawColumnHeader;
        }

        private void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
            {
                e.Graphics.DrawRectangle(pen, e.Bounds);
            }

            var textRect = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 6, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, new Font("Segoe UI", 9F, FontStyle.Bold),
                                textRect, Color.FromArgb(60, 60, 60),
                                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private void ListView_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            // This will be handled by DrawSubItem for detailed view
            if (sender is ListView listView && listView.View != View.Details)
            {
                e.DrawDefault = true;
            }
        }

        private void ListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            var backColor = e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 248, 248);

            if (e.Item.Selected)
            {
                backColor = Color.FromArgb(220, 235, 252);
            }

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            var textColor = e.Item.Selected ? Color.FromArgb(40, 40, 40) : Color.FromArgb(60, 60, 60);
            var textRect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height);

            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, new Font("Segoe UI", 9F),
                                textRect, textColor,
                                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // Draw grid lines
            using (var pen = new Pen(Color.FromArgb(230, 230, 230)))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Load application settings
            var settings = AppSettings.Load();
            ApplySettings(settings);
            
            // Initialize folder monitoring service
            InitializeFolderMonitoring();

            // Cleanup old logs
            LoggingService.CleanupOldLogs(settings.LogRetentionDays);
            LoggingService.LogInfo("Application started", "Startup");

            // Test the problematic filename parsing (remove this after testing)
            #if DEBUG
            FileEpisode.TestFilenameParsing(@"C:\temp\I Fought the Law 2025 S01E01 720p WEB-DL HEVC x265 BONE.mkv", @"C:\TestOutput");
            FileEpisode.TestFilenameParsing(@"C:\temp\Show Name S01E00 Pilot Episode.mkv", @"C:\TestOutput");
            FileEpisode.TestFilenameParsing(@"C:\temp\Show:Name*With?Problems S01E01.mkv", @"C:\TestOutput");
            FileEpisode.TestFilenameParsing(@"C:\temp\Unknown Show S01E01.mkv", @"C:\TestOutput");
            FileEpisode.TestFilenameParsing(@"C:\temp\Smallville - S01 E01 - Pilot (720P - Amzn Web-Dl).mp4", @"C:\TestOutput");
            #endif
        }

        private void ApplySettings(AppSettings settings)
        {
            try
            {
                // Apply monitoring settings with proper defaults
                _savedWatchFolder = settings.Monitoring.WatchFolder ?? "";
                _savedDestinationFolder = settings.Monitoring.DestinationFolder ?? "";
                _savedFileExtensions = !string.IsNullOrEmpty(settings.Monitoring.FileExtensions) 
                    ? settings.Monitoring.FileExtensions 
                    : "*.mp4;*.mkv;*.avi;*.m4v";
                _savedStabilityDelay = settings.Monitoring.StabilityDelaySeconds > 0 
                    ? settings.Monitoring.StabilityDelaySeconds 
                    : 30;
                _savedAutoStart = settings.Monitoring.AutoStartOnLoad;
                _savedMaxAutoMonitorLogEntries = settings.Monitoring.MaxAutoMonitorLogEntries > 0
                    ? settings.Monitoring.MaxAutoMonitorLogEntries
                    : 20;
                
                LoggingService.LogInfo($"Settings applied - Watch: '{_savedWatchFolder}', Dest: '{_savedDestinationFolder}', Ext: '{_savedFileExtensions}', Delay: {_savedStabilityDelay}s, AutoStart: {_savedAutoStart}, MaxLogEntries: {_savedMaxAutoMonitorLogEntries}", "Settings");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to apply settings", ex, "Settings");
                
                // Set safe defaults if settings loading fails
                _savedWatchFolder = "";
                _savedDestinationFolder = "";
                _savedFileExtensions = "*.mp4;*.mkv;*.avi;*.m4v";
                _savedStabilityDelay = 30;
                _savedAutoStart = false;
                _savedMaxAutoMonitorLogEntries = 20;
            }
        }

        private void SaveCurrentSettings()
        {
            try
            {
                var settings = AppSettings.Load();
                
                // Update with current values
                settings.Monitoring.WatchFolder = _savedWatchFolder;
                settings.Monitoring.DestinationFolder = _savedDestinationFolder;
                settings.Monitoring.FileExtensions = _savedFileExtensions;
                settings.Monitoring.StabilityDelaySeconds = _savedStabilityDelay;
                settings.Monitoring.AutoStartOnLoad = _savedAutoStart;
                settings.Monitoring.MaxAutoMonitorLogEntries = _savedMaxAutoMonitorLogEntries;
                
                settings.Save();
                LoggingService.LogInfo("Current settings saved", "Settings");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to save current settings", ex, "Settings");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Save settings before closing
                SaveCurrentSettings();
                
                // Stop monitoring service
                _folderMonitorService?.StopMonitoring();
                _folderMonitorService?.Dispose();
                
                LoggingService.LogInfo("Application closing", "Shutdown");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error during application shutdown", ex, "Shutdown");
            }
            
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Validates system state before processing files
        /// </summary>
        private bool ValidateBeforeProcessing(bool isTVEpisodes)
        {
            var itemsToProcess = isTVEpisodes ? 
                lvFiles.Items.Cast<ListViewItem>().Where(item => item.BackColor != Color.LightGray) :
                lvMovies.Items.Cast<ListViewItem>().Where(item => item.BackColor != Color.LightGray);
                
            if (!itemsToProcess.Any())
            {
                MessageBox.Show("No valid items to process. All items are unparsed or invalid.", 
                    "No Valid Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            // Check destination paths and space for a sample of items
            var sampleItems = itemsToProcess.Take(5).ToList();
            long totalEstimatedSize = 0;
            
            foreach (ListViewItem lvi in sampleItems)
            {
                try
                {
                    string destinationPath;
                    string sourceFile;
                    
                    if (isTVEpisodes)
                    {
                        FileEpisode fe = (FileEpisode)lvi.Tag;
                        destinationPath = fe.NewDirectoryName;
                        sourceFile = fe.FullFilePath;
                    }
                    else
                    {
                        MovieFile mv = (MovieFile)lvi.Tag;
                        destinationPath = Path.GetDirectoryName(mv.NewDestDirectory);
                        sourceFile = mv.FileNamePath;
                    }
                    
                    // Validate destination directory
                    var validation = PathValidator.ValidateDestinationPath(destinationPath);
                    if (!validation.IsValid)
                    {
                        var continueResult = MessageBox.Show(
                            $"Destination validation warning: {validation.Message}\n\n" +
                            "Continue anyway?", 
                            "Validation Warning", 
                            MessageBoxButtons.YesNo, 
                            MessageBoxIcon.Warning);
                            
                        if (continueResult != DialogResult.Yes)
                            return false;
                    }
                    
                    // Add to size estimation
                    if (File.Exists(sourceFile))
                    {
                        FileInfo fi = new FileInfo(sourceFile);
                        totalEstimatedSize += fi.Length;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Validation error: {ex.Message}");
                }
            }
            
            // Extrapolate total size if we sampled
            if (sampleItems.Count < itemsToProcess.Count())
            {
                double ratio = (double)itemsToProcess.Count() / sampleItems.Count;
                totalEstimatedSize = (long)(totalEstimatedSize * ratio);
            }
            
            return true;
        }

        /// <summary>
        /// Show help information to the user
        /// </summary>
        private void ShowHelp()
        {
            string helpText = @"Torrent File Renamer Help

TV EPISODES:
• Supports formats: S01E01, 1x01, Season 1 Episode 1
• Multi-episode files: S01E01-02, S01E01E02
• Creates structure: Show Name/Season X/Episode files

MOVIES:
• Organizes by first letter (A-Z, 0-9, #)
• Removes quality tags (720p, 1080p, etc.)
• Handles years in various formats

FEATURES:
• Smart file type detection
• Multi-select removal
• Progress tracking
• Disk space checking
• Network path support

TIPS:
• Always verify destination paths
• Use 'Remove Unparsed' for cleanup
• Check available disk space
• Review unparsed files (gray) before processing";

            MessageBox.Show(helpText, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Enhanced scan with async operations and cancellation support
        /// </summary>
        private async void miScanFiles_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Scanning for TV episodes...";
            statusProgress.Value = 0;
            
            frmScanOptions options = new frmScanOptions();
            DialogResult dr = options.ShowDialog();
            if (dr == DialogResult.OK)
            {
                // Create cancellation token for long operations
                using var cts = new CancellationTokenSource();
                
                try
                {
                    // Use async file enumeration for better performance
                    var allFiles = await Task.Run(() => 
                        Directory.EnumerateFiles(options.VideoPath, "*.*", SearchOption.AllDirectories)
                        .Where(f => f.EndsWith(options.FileExtension, StringComparison.OrdinalIgnoreCase))
                        .ToArray(), cts.Token);
                    
                    statusProgress.Maximum = allFiles.Length;
                    statusProgress.Value = 0;
                    
                    int totalVideoFiles = 0;
                    int likelyMoviesSkipped = 0;
                    int validEpisodes = 0;
                    int unparsedFiles = 0;

                    // Process files in batches for better responsiveness
                    const int batchSize = 10;
                    for (int i = 0; i < allFiles.Length; i += batchSize)
                    {
                        var batch = allFiles.Skip(i).Take(batchSize);
                        
                        await Task.Run(() => {
                            foreach (string s in batch)
                            {
                                if (cts.Token.IsCancellationRequested)
                                    return;
                                    
                                ProcessFileForScan(s, options, ref totalVideoFiles, ref likelyMoviesSkipped, 
                                                 ref validEpisodes, ref unparsedFiles);
                            }
                        }, cts.Token);
                        
                        // Update UI on main thread
                        statusProgress.Value = Math.Min(i + batchSize, allFiles.Length);
                        Application.DoEvents();
                    }
                    
                    statusLabel.Text = $"TV scan completed: {validEpisodes} episodes found, {unparsedFiles} unparsed, {likelyMoviesSkipped} movies skipped from {totalVideoFiles} total files at {DateTime.Now:HH:mm:ss}";
                    
                    // Show summary if useful
                    if (totalVideoFiles > 0)
                    {
                        await ShowScanSummary(totalVideoFiles, validEpisodes, unparsedFiles, likelyMoviesSkipped);
                    }
                }
                catch (OperationCanceledException)
                {
                    statusLabel.Text = "Scan cancelled by user";
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied to source directory. Please check permissions.", 
                        "Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Scan failed - Access denied";
                }
                catch (DirectoryNotFoundException)
                {
                    MessageBox.Show("Source directory not found. Please verify the path.", 
                        "Directory Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Scan failed - Directory not found";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during scan: {ex.Message}", 
                        "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Scan failed - Error occurred";
                    Debug.WriteLine($"Scan error: {ex}");
                }
            }
            else
            {
                statusLabel.Text = "TV episode scan cancelled";
                Debug.WriteLine("Dialog Cancelled");
            }
        }

        private void ProcessFileForScan(string filePath, frmScanOptions options, 
            ref int totalVideoFiles, ref int likelyMoviesSkipped, 
            ref int validEpisodes, ref int unparsedFiles)
        {
            totalVideoFiles++;
            
            // Check if this file is likely a movie and should be skipped
            int tvConfidence = MediaTypeDetector.GetTVEpisodeConfidence(filePath);
            int movieConfidence = MediaTypeDetector.GetMovieConfidence(filePath);
            
            // Skip files that seem more like movies than TV episodes
            if (movieConfidence > tvConfidence && movieConfidence > 40)
            {
                likelyMoviesSkipped++;
                Debug.WriteLine($"Skipping likely movie: {Path.GetFileName(filePath)} (Movie: {movieConfidence}%, TV: {tvConfidence}%)");
                return;
            }
            
            FileEpisode fe = new FileEpisode(filePath, options.OutputDirectory);
            
            // Create local variables to avoid ref parameter issue in lambda
            int localValidEpisodes = validEpisodes;
            int localUnparsedFiles = unparsedFiles;
            
            // Process on main thread since it involves UI updates
            Invoke(new Action(() => {
                AddFileToList(fe, ref localValidEpisodes, ref localUnparsedFiles, tvConfidence, movieConfidence);
            }));
            
            // Update the original ref parameters
            validEpisodes = localValidEpisodes;
            unparsedFiles = localUnparsedFiles;
        }

        private void AddFileToList(FileEpisode fe, ref int validEpisodes, ref int unparsedFiles, 
            int tvConfidence, int movieConfidence)
        {
            // Allow episode 0 for special episodes - only exclude truly invalid parsing
            if (fe.EpisodeNumber >= 0 && !string.IsNullOrWhiteSpace(fe.ShowName) && fe.ShowName != "Unknown Show")
            {
                // Check Plex compatibility for manual processing
                bool shouldAdd = true;
                if (fe.PlexValidation != null && !fe.PlexValidation.IsValid)
                {
                    // Handle Plex compatibility issues in manual mode
                    if (fe.PlexValidation.SuggestedAction == PlexValidationAction.PromptUser)
                    {
                        string issuesText = string.Join("\n• ", fe.PlexValidation.Issues);
                        string warningsText = fe.PlexValidation.Warnings.Any() ? 
                            "\n\nWarnings:\n• " + string.Join("\n• ", fe.PlexValidation.Warnings) : "";
                        
                        var result = MessageBox.Show(
                            $"Plex Compatibility Issues Found:\n• {issuesText}{warningsText}\n\n" +
                            $"Original file: {Path.GetFileName(fe.FullFilePath)}\n" +
                            $"Suggested name: {fe.NewFileName}\n\n" +
                            "Would you like to:\n" +
                            "• Yes: Use the suggested fix\n" +
                            "• No: Skip this file\n" +
                            "• Cancel: Stop scanning",
                            "Plex Compatibility Issue",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Warning);

                        switch (result)
                        {
                            case DialogResult.Yes:
                                // Use the suggested filename (already applied by auto-fix)
                                break;
                            case DialogResult.No:
                                shouldAdd = false;
                                break;
                            case DialogResult.Cancel:
                                return; // Stop scanning entirely
                        }
                    }
                }

                if (shouldAdd)
                {
                    validEpisodes++;
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = fe.FullFilePath;
                    lvi.SubItems.Add(fe.NewFileNamePath);
                    lvi.SubItems.Add(fe.ShowName);
                    lvi.SubItems.Add(fe.SeasonNumber.ToString());
                    
                    // Display episode numbers - show all episodes if multi-episode
                    string episodeDisplay;
                    if (fe.IsMultiEpisode)
                    {
                        episodeDisplay = string.Join(", ", fe.EpisodeNumbers.OrderBy(x => x));
                    }
                    else
                    {
                        episodeDisplay = fe.EpisodeNumber.ToString();
                    }
                    lvi.SubItems.Add(episodeDisplay);
                    
                    // Add status based on Plex validation
                    string status = "";
                    if (fe.PlexValidation != null)
                    {
                        if (!fe.PlexValidation.IsValid)
                            status = "⚠️ Plex Issues Fixed";
                        else if (fe.PlexValidation.Warnings.Any())
                            status = "⚠️ Plex Warnings";
                        else
                            status = "✅ Plex Compatible";
                    }
                    lvi.SubItems.Add(status);
                    
                    lvi.Tag = fe;
                    
                    // Color code based on Plex validation
                    if (fe.PlexValidation != null && fe.PlexValidation.Warnings.Any())
                        lvi.BackColor = Color.LightYellow;
                    
                    lvFiles.Items.Add(lvi);
                }
            }
            else
            {
                // Only add unparsed files that have some TV characteristics
                if (tvConfidence > 20 || movieConfidence < 30)
                {
                    unparsedFiles++;
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = fe.FullFilePath;
                    lvi.SubItems.Add("UNPARSED - Please review");
                    lvi.SubItems.Add(fe.ShowName ?? "Unknown");
                    lvi.SubItems.Add(fe.SeasonNumber.ToString());
                    lvi.SubItems.Add(fe.EpisodeNumber.ToString());
                    lvi.SubItems.Add("Not processed");
                    lvi.BackColor = Color.LightGray;
                    lvi.Tag = fe;
                    lvFiles.Items.Add(lvi);
                }
                else
                {
                    // This was the missing variable - we don't increment it here since it's handled in the loop
                    Debug.WriteLine($"Skipping unparsed likely movie: {Path.GetFileName(fe.FullFilePath)}");
                }
            }
        }

        private async Task ShowScanSummary(int totalVideoFiles, int validEpisodes, int unparsedFiles, int likelyMoviesSkipped)
        {
            await Task.Delay(100); // Brief delay for UI responsiveness
            
            string summary = $"Scan Results:\n" +
                           $"Total video files: {totalVideoFiles}\n" +
                           $"Valid TV episodes: {validEpisodes}\n" +
                           $"Unparsed files: {unparsedFiles}\n" +
                           $"Movies skipped: {likelyMoviesSkipped}";
            
            if (unparsedFiles > 0)
            {
                summary += "\n\nTip: Review unparsed files (gray) and remove them if they're not TV episodes.";
            }
            
            MessageBox.Show(summary, "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool RetryFileMove(string sourceFile, string destFile, int NumRetries)
        {
            for (int i = 0; i < NumRetries; i++)
            {
                try
                {
                    if (File.Exists(destFile))
                        File.Delete(destFile);
                    File.Copy(sourceFile, destFile);
                    return true;
                }
                catch (IOException)
                {
                    Debug.WriteLine("IO Exception. Waiting for file copy retry. " + i.ToString());
                    System.Threading.Thread.Sleep(5000);
                }
            }
            return false;
        }

        /// <summary>
        /// Enhanced processing with better progress tracking and error recovery
        /// </summary>
        private async void miProcess_Click(object sender, EventArgs e)
        {
            if (lvFiles.Items.Count == 0)
            {
                MessageBox.Show("No TV episodes to process. Please scan for files first.", 
                    "No Files", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Pre-processing validation
            if (!ValidateBeforeProcessing(true))
                return;

            // Ask for user confirmation
            int validItems = lvFiles.Items.Cast<ListViewItem>()
                .Count(item => item.BackColor != Color.LightGray);
            
            var result = MessageBox.Show(
                $"Process {validItems} TV episode(s)?\n\nThis will:\n" +
                "1. Copy files to the server\n" +
                "2. Verify file sizes match\n" +
                "3. Delete original files if verification succeeds\n\n" +
                "Continue?", 
                "Confirm Processing", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
                
            if (result != DialogResult.Yes)
                return;

            // Initialize processing with cancellation support
            using var cts = new CancellationTokenSource();
            var progress = new Progress<ProcessingProgress>(UpdateProcessingProgress);
            
            try
            {
                await ProcessFilesAsync(validItems, progress, cts.Token);
            }
            catch (OperationCanceledException)
            {
                statusLabel.Text = "Processing cancelled by user";
                LoggingService.LogInfo("File processing cancelled by user", "Processing");
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Processing failed with error";
                LoggingService.LogError("File processing failed", ex, "Processing");
                MessageBox.Show($"Processing failed: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ProcessFilesAsync(int totalItems, IProgress<ProcessingProgress> progress, CancellationToken cancellationToken)
        {
            int processedCount = 0;
            int successfulCopies = 0;
            int successfulDeletions = 0;
            int errors = 0;
            
            statusProgress.Maximum = totalItems;
            DateTime startTime = DateTime.Now;
            
            var itemsToProcess = lvFiles.Items.Cast<ListViewItem>()
                .Where(item => item.BackColor != Color.LightGray)
                .ToList();

            // Phase 1: Copy files
            LoggingService.LogInfo($"Starting to process {totalItems} files", "Processing");
            
            foreach (var lvi in itemsToProcess)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                FileEpisode fe = (FileEpisode)lvi.Tag;
                processedCount++;
                
                var currentProgress = new ProcessingProgress
                {
                    CurrentFile = Path.GetFileName(fe.FullFilePath),
                    ProcessedCount = processedCount,
                    TotalCount = totalItems,
                    Phase = ProcessingPhase.Copying,
                    Message = $"Copying {processedCount}/{totalItems}: {Path.GetFileName(fe.FullFilePath)}"
                };
                
                progress.Report(currentProgress);
                
                try
                {
                    if (!Directory.Exists(fe.NewDirectoryName))
                    {
                        LoggingService.LogDebug($"Creating directory: {fe.NewDirectoryName}", "Processing");
                        Directory.CreateDirectory(fe.NewDirectoryName);
                    }

                    // Enhanced retry logic with async
                    bool retVal = await RetryFileOperationAsync(
                        () => CopyFileAsync(fe.FullFilePath, fe.NewFileNamePath),
                        maxRetries: 5,
                        cancellationToken);
                    
                    // Update UI on the UI thread
                    Invoke(new Action(() => {
                        if (retVal)
                        {
                            lvi.BackColor = Color.Yellow;
                            successfulCopies++;
                            LoggingService.LogDebug($"Successfully copied: {Path.GetFileName(fe.FullFilePath)}", "Processing");
                        }
                        else
                        {
                            lvi.BackColor = Color.Red;
                            lvi.SubItems[5].Text = "Copy failed after retries";
                            errors++;
                            LoggingService.LogError($"Failed to copy after retries: {fe.FullFilePath}", null, "Processing");
                        }
                    }));
                }
                catch (Exception ex)
                {
                    // Update UI on the UI thread
                    Invoke(new Action(() => {
                        lvi.BackColor = Color.Red;
                        lvi.SubItems[5].Text = $"Error: {ex.Message}";
                        errors++;
                    }));
                    LoggingService.LogError($"Error processing {fe.FullFilePath}", ex, "Processing");
                }
                
                // Update UI
                Invoke(new Action(() => {
                    lvFiles.EnsureVisible(itemsToProcess.IndexOf(lvi));
                    lvFiles.Refresh();
                }));
            }
            
            // Phase 2: Verify and cleanup
            LoggingService.LogInfo("Starting verification and cleanup phase", "Processing");
            processedCount = 0;
            
            foreach (var lvi in itemsToProcess.Where(item => item.BackColor == Color.Yellow))
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                FileEpisode fe = (FileEpisode)lvi.Tag;
                processedCount++;
                
                var currentProgress = new ProcessingProgress
                {
                    CurrentFile = Path.GetFileName(fe.FullFilePath),
                    ProcessedCount = processedCount,
                    TotalCount = successfulCopies,
                    Phase = ProcessingPhase.Verifying,
                    Message = $"Verifying and cleaning up {processedCount}/{successfulCopies}"
                };
                
                progress.Report(currentProgress);
                
                try
                {
                    await Task.Run(() => {
                        FileInfo fiLocal = new FileInfo(fe.FullFilePath);
                        FileInfo fiRemote = new FileInfo(fe.NewFileNamePath);
                        
                        // Prepare the UI updates on the background thread but don't execute them yet
                        string statusMessage;
                        Color statusColor;
                        
                        if (!fiRemote.Exists)
                        {
                            statusMessage = "Remote file not found";
                            statusColor = Color.Red;
                            errors++;
                            LoggingService.LogError($"Remote file not found: {fe.NewFileNamePath}", null, "Processing");
                        }
                        else if (fiLocal.Length == fiRemote.Length)
                        {
                            File.Delete(fe.FullFilePath);
                            statusMessage = "Completed - Original deleted";
                            statusColor = Color.LightGreen;
                            successfulDeletions++;
                            LoggingService.LogInfo($"Successfully processed: {Path.GetFileName(fe.FullFilePath)}", "Processing");
                        }
                        else
                        {
                            statusMessage = $"Size mismatch - Local: {fiLocal.Length}, Remote: {fiRemote.Length}";
                            statusColor = Color.Orange;
                            errors++;
                            LoggingService.LogWarning($"Size mismatch for {fe.FullFilePath}: Local={fiLocal.Length}, Remote={fiRemote.Length}", "Processing");
                        }
                        
                        // Now update the UI on the UI thread
                        Invoke(new Action(() => {
                            lvi.SubItems[5].Text = statusMessage;
                            lvi.BackColor = statusColor;
                        }));
                        
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Update UI on the UI thread
                    Invoke(new Action(() => {
                        lvi.SubItems[5].Text = $"Cleanup error: {ex.Message}";
                        lvi.BackColor = Color.Red;
                        errors++;
                    }));
                    LoggingService.LogError($"Cleanup error for {fe.FullFilePath}", ex, "Processing");
                }
            }
            
            // Final summary
            TimeSpan duration = DateTime.Now - startTime;
            string finalMessage = $"Processing completed in {duration.Minutes}m {duration.Seconds}s - " +
                                $"Copied: {successfulCopies}, Deleted: {successfulDeletions}, Errors: {errors}";
            
            progress.Report(new ProcessingProgress
            {
                Phase = ProcessingPhase.Complete,
                Message = finalMessage,
                ProcessedCount = totalItems,
                TotalCount = totalItems
            });
            
            LoggingService.LogInfo($"Processing completed: Copied={successfulCopies}, Deleted={successfulDeletions}, Errors={errors}, Duration={duration}", "Processing");
            
            // Show completion summary
            string summary = $"TV Episode Processing Complete!\n\n" +
                           $"Files processed: {totalItems}\n" +
                           $"Successfully copied: {successfulCopies}\n" +
                           $"Original files deleted: {successfulDeletions}\n" +
                           $"Errors: {errors}\n" +
                           $"Duration: {duration.Minutes}m {duration.Seconds}s";
                           
            MessageBox.Show(summary, "Processing Complete", 
                MessageBoxButtons.OK, 
                errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private async Task<bool> CopyFileAsync(string source, string destination)
        {
            const int bufferSize = 8192; // 8KB buffer
            
            using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            using var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
            
            await sourceStream.CopyToAsync(destinationStream);
            return true;
        }

        private async Task<bool> RetryFileOperationAsync(Func<Task<bool>> operation, int maxRetries, CancellationToken cancellationToken)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return await operation();
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    LoggingService.LogWarning($"File operation attempt {attempt} failed, retrying in 5 seconds...", "Processing");
                    await Task.Delay(5000, cancellationToken);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"File operation attempt {attempt} failed", ex, "Processing");
                    if (attempt >= maxRetries)
                        throw;
                    await Task.Delay(5000, cancellationToken);
                }
            }
            return false;
        }

        private void UpdateProcessingProgress(ProcessingProgress progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<ProcessingProgress>(UpdateProcessingProgress), progress);
                return;
            }
            
            statusLabel.Text = progress.Message;
            statusProgress.Value = progress.ProcessedCount;
            statusProgress.Maximum = progress.TotalCount;
        }

        public class ProcessingProgress
        {
            public string CurrentFile { get; set; } = "";
            public int ProcessedCount { get; set; }
            public int TotalCount { get; set; }
            public ProcessingPhase Phase { get; set; }
            public string Message { get; set; } = "";
        }

        public enum ProcessingPhase
        {
            Copying,
            Verifying,
            Complete
        }

        /// <summary>
        /// Show application logs for troubleshooting
        /// </summary>
        private void miShowLogs_Click(object sender, EventArgs e)
        {
            try
            {
                var logs = LoggingService.GetRecentLogs(200);
                
                var logForm = new Form
                {
                    Text = "Application Logs",
                    Size = new Size(800, 600),
                    StartPosition = FormStartPosition.CenterParent
                };
                
                var textBox = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 9),
                    ReadOnly = true,
                    Text = string.Join(Environment.NewLine, logs)
                };
                
                logForm.Controls.Add(textBox);
                logForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to show logs: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetShortPath(string path)
        {
            var shortPath = new StringBuilder(MAX_PATH);
            GetShortPathName(path, shortPath, MAX_PATH);
            return shortPath.ToString();
        }

        public static bool DeleteFileWithRetry(string filename)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    string did = GetShortPath(filename);
                    File.Delete(GetShortPath(filename));
                    return true;
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            }

            return false;
        }

        private void miRemove_Click(object sender, EventArgs e)
        {
            // Remove all currently selected listview items for TV episodes
            if (lvFiles.SelectedItems.Count == 0)
            {
                statusLabel.Text = "No items selected for removal";
                return;
            }

            int removedCount = 0;
            // Create a copy of the selected items since we'll be modifying the collection
            ListViewItem[] selectedItems = new ListViewItem[lvFiles.SelectedItems.Count];
            lvFiles.SelectedItems.CopyTo(selectedItems, 0);

            foreach (ListViewItem lvi in selectedItems)
            {
                try
                {
                    lvFiles.Items.Remove(lvi);
                    removedCount++;
                }
                catch (Exception ex) 
                {
                    Debug.WriteLine($"Error removing TV episode item: {ex.Message}");
                }
            }
            
            statusLabel.Text = $"Removed {removedCount} TV episode item(s) from list";
        }

        /// <summary>
        /// Remove selected movie items from the list
        /// </summary>
        private void msMovieRemove_Click(object sender, EventArgs e)
        {
            // Remove all currently selected listview items for movies
            if (lvMovies.SelectedItems.Count == 0)
            {
                statusLabel.Text = "No movie items selected for removal";
                return;
            }

            int removedCount = 0;
            // Create a copy of the selected items since we'll be modifying the collection
            ListViewItem[] selectedItems = new ListViewItem[lvMovies.SelectedItems.Count];
            lvMovies.SelectedItems.CopyTo(selectedItems, 0);

            foreach (ListViewItem lvi in selectedItems)
            {
                try
                {
                    lvMovies.Items.Remove(lvi);
                    removedCount++;
                }
                catch (Exception ex) 
                {
                    Debug.WriteLine($"Error removing movie item: {ex.Message}");
                }
            }
            
            statusLabel.Text = $"Removed {removedCount} movie item(s) from list";
        }

        /// <summary>
        /// Clear all movie items from the list
        /// </summary>
        private void msMovieClear_Click(object sender, EventArgs e)
        {
            lvMovies.Items.Clear();
            statusLabel.Text = "Movie list cleared";
        }

        private void tsbScan_Click(object sender, EventArgs e)
        {
            miScanFiles_Click(this, null);
        }

        private void tsbProcess_Click(object sender, EventArgs e)
        {
            miProcess_Click(this, null);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvFiles.Items.Clear();
            statusLabel.Text = "TV episode list cleared";
        }

        private void InitializeFolderMonitoring()
        {
            try
            {
                _folderMonitorService = new FolderMonitorService();
                
                // Subscribe to events
                _folderMonitorService.StatusChanged += FolderMonitorService_StatusChanged;
                _folderMonitorService.FileFound += FolderMonitorService_FileFound;
                _folderMonitorService.FileProcessed += FolderMonitorService_FileProcessed;
                _folderMonitorService.ErrorOccurred += FolderMonitorService_ErrorOccurred;

                // Load saved settings (in a real app, you'd load from config file or registry)
                LoadMonitoringSettings();

                // Auto-start only if explicitly enabled AND valid configuration exists
                if (_savedAutoStart && 
                    !string.IsNullOrWhiteSpace(_savedWatchFolder) && 
                    !string.IsNullOrWhiteSpace(_savedDestinationFolder) &&
                    Directory.Exists(_savedWatchFolder))
                {
                    LoggingService.LogInfo($"Auto-starting monitoring: Watch='{_savedWatchFolder}', Dest='{_savedDestinationFolder}'", "AutoMonitor");
                    
                    bool started = ConfigureAndStartMonitoring(_savedWatchFolder, _savedDestinationFolder, _savedFileExtensions, _savedStabilityDelay);
                    if (!started)
                    {
                        LoggingService.LogWarning("Auto-start monitoring failed - configuration may be invalid", "AutoMonitor");
                    }
                }
                else
                {
                    LoggingService.LogInfo($"Auto-monitoring not started - AutoStart: {_savedAutoStart}, ValidConfig: {!string.IsNullOrWhiteSpace(_savedWatchFolder) && !string.IsNullOrWhiteSpace(_savedDestinationFolder)}", "AutoMonitor");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing folder monitoring: {ex.Message}", 
                    "Monitoring Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoggingService.LogError("Failed to initialize folder monitoring", ex, "AutoMonitor");
            }
        }

        private void LoadMonitoringSettings()
        {
            // In a real application, load these from app settings or registry
            // For now, use defaults - you could enhance this to save/load from a config file
        }

        private void SaveMonitoringSettings()
        {
            // In a real application, save these to app settings or registry
        }

        private void FolderMonitorService_StatusChanged(object? sender, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateMonitoringStatus), status);
            }
            else
            {
                UpdateMonitoringStatus(status);
            }
        }

        private void UpdateMonitoringStatus(string status)
        {
            statusLabel.Text = $"Auto-Monitor: {status}";

            // Update monitoring status in the TV episodes list
            if (_monitoringStatusItem == null)
            {
                _monitoringStatusItem = new ListViewItem("Folder Monitoring");
                _monitoringStatusItem.BackColor = Color.LightBlue;
                _monitoringStatusItem.SubItems.Add(status);
                _monitoringStatusItem.SubItems.Add("System");
                _monitoringStatusItem.SubItems.Add("Auto");
                _monitoringStatusItem.SubItems.Add("Monitor");
                _monitoringStatusItem.SubItems.Add(DateTime.Now.ToString("HH:mm:ss"));
                lvFiles.Items.Insert(0, _monitoringStatusItem);
            }
            else
            {
                _monitoringStatusItem.SubItems[1].Text = status;
                _monitoringStatusItem.SubItems[5].Text = DateTime.Now.ToString("HH:mm:ss");
            }
        }

        private void miConfigureMonitoring_Click(object sender, EventArgs e)
        {
            // Use the same configuration as the auto-monitor
            miAutoMonitor_Click(sender, e);
        }

        private void FolderMonitorService_FileFound(object? sender, FileFoundEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<FileFoundEventArgs>(HandleFileFound), e);
            }
            else
            {
                HandleFileFound(e);
            }
        }

        private void HandleFileFound(FileFoundEventArgs e)
        {
            Debug.WriteLine($"File found: {Path.GetFileName(e.FilePath)} ({e.EventType})");
            statusLabel.Text = $"Auto-Monitor: Found TV show - {Path.GetFileName(e.FilePath)}";
        }

        private void FolderMonitorService_FileProcessed(object? sender, FileProcessedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<FileProcessedEventArgs>(HandleFileProcessed), e);
            }
            else
            {
                HandleFileProcessed(e);
            }
        }

        private void HandleFileProcessed(FileProcessedEventArgs e)
        {
            // Add processed file to the list for tracking
            ListViewItem lvi = new ListViewItem($"🔄 {Path.GetFileName(e.FilePath)}");
            lvi.BackColor = e.Success ? Color.LightGreen : Color.LightCoral;
            lvi.SubItems.Add(e.Success ? "Auto-processed" : "Auto-failed");
            lvi.SubItems.Add("Auto-Monitor");
            lvi.SubItems.Add("Auto");
            lvi.SubItems.Add("Auto");
            lvi.SubItems.Add($"{e.Message} at {e.ProcessedAt:HH:mm:ss}");
            
            // Insert newest items right after the monitoring status item (most recent first)
            int insertIndex = _monitoringStatusItem != null ? 1 : 0;
            if (insertIndex <= lvFiles.Items.Count)
            {
                lvFiles.Items.Insert(insertIndex, lvi);
            }
            else
            {
                lvFiles.Items.Add(lvi);
            }

            // Keep only the configured number of auto-processed items to avoid cluttering
            var autoItems = lvFiles.Items.Cast<ListViewItem>()
                .Where(item => item.Text.StartsWith("🔄"))
                .Skip(_savedMaxAutoMonitorLogEntries)  // Keep only the first N items, remove the rest
                .ToList();
            
            foreach (var item in autoItems)
            {
                lvFiles.Items.Remove(item);
            }

            Debug.WriteLine($"File processed: {Path.GetFileName(e.FilePath)} - Success: {e.Success} - {e.Message}");
        }

        private void FolderMonitorService_ErrorOccurred(object? sender, Exception e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Exception>(HandleMonitoringError), e);
            }
            else
            {
                HandleMonitoringError(e);
            }
        }

        private void HandleMonitoringError(Exception ex)
        {
            Debug.WriteLine($"Monitoring error: {ex.Message}");
            statusLabel.Text = $"Auto-Monitor Error: {ex.Message}";
        }

        private bool ConfigureAndStartMonitoring(string watchFolder, string destinationFolder, string extensions, int stabilityDelay)
        {
            if (_folderMonitorService == null)
                return false;

            try
            {
                // Parse extensions
                string[] extensionArray = extensions.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(ext => ext.Trim())
                    .ToArray();

                _folderMonitorService.WatchFolder = watchFolder;
                _folderMonitorService.DestinationFolder = destinationFolder;
                _folderMonitorService.FileExtensions = extensionArray;
                _folderMonitorService.StabilityDelaySeconds = stabilityDelay;

                return _folderMonitorService.StartMonitoring();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting monitoring: {ex.Message}", 
                    "Monitoring Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void miAutoMonitor_Click(object sender, EventArgs e)
        {
            // Use the proper folder monitor configuration form
            try
            {
                using (var configForm = new frmFolderMonitor())
                {
                    // Initialize config form with current settings
                    configForm.WatchFolder = _savedWatchFolder;
                    configForm.DestinationPath = _savedDestinationFolder;
                    configForm.FileExtensions = _savedFileExtensions;
                    configForm.StabilityDelaySeconds = _savedStabilityDelay;
                    configForm.AutoStartOnLoad = _savedAutoStart;
                    configForm.MaxAutoMonitorLogEntries = _savedMaxAutoMonitorLogEntries;
                    
                    // Show config form as dialog
                    DialogResult result = configForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        // Apply new settings
                        _savedWatchFolder = configForm.WatchFolder;
                        _savedDestinationFolder = configForm.DestinationPath;
                        _savedFileExtensions = configForm.FileExtensions;
                        _savedStabilityDelay = configForm.StabilityDelaySeconds;
                        _savedAutoStart = configForm.AutoStartOnLoad;
                        _savedMaxAutoMonitorLogEntries = configForm.MaxAutoMonitorLogEntries;
                        
                        // Save settings to persistent storage
                        SaveCurrentSettings();
                        
                        // Stop current monitoring if running
                        if (_folderMonitorService != null && _folderMonitorService.IsMonitoring)
                        {
                            _folderMonitorService.StopMonitoring();
                        }
                        
                        // Start monitoring with new settings if auto-start is enabled
                        if (_savedAutoStart && !string.IsNullOrWhiteSpace(_savedWatchFolder) && !string.IsNullOrWhiteSpace(_savedDestinationFolder))
                        {
                            if (ConfigureAndStartMonitoring(_savedWatchFolder, _savedDestinationFolder, _savedFileExtensions, _savedStabilityDelay))
                            {
                                MessageBox.Show("Auto-monitoring configured and started successfully!", 
                                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Auto-monitoring configuration saved, but failed to start monitoring. Please check the settings.", 
                                    "Partial Success", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Auto-monitoring settings saved. Use 'Start/Stop Monitoring' to begin monitoring.", 
                                "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring auto-monitor: {ex.Message}", 
                    "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoggingService.LogError("Failed to configure auto-monitoring", ex, "AutoMonitor");
            }
        }

        private void miToggleMonitoring_Click(object sender, EventArgs e)
        {
            if (_folderMonitorService == null)
            {
                MessageBox.Show("Monitoring service not initialized.", "Service Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_folderMonitorService.IsMonitoring)
            {
                // Stop monitoring
                _folderMonitorService.StopMonitoring();
                MessageBox.Show("Auto-monitoring stopped.", "Monitoring Stopped", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoggingService.LogInfo("Auto-monitoring stopped by user", "AutoMonitor");
            }
            else
            {
                // Try to start monitoring with current settings
                if (string.IsNullOrWhiteSpace(_savedWatchFolder) || string.IsNullOrWhiteSpace(_savedDestinationFolder))
                {
                    var result = MessageBox.Show("No monitoring configuration found. Would you like to configure monitoring now?", 
                        "Configuration Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        miAutoMonitor_Click(sender, e);
                    }
                }
                else
                {
                    // Start monitoring with existing settings
                    if (ConfigureAndStartMonitoring(_savedWatchFolder, _savedDestinationFolder, _savedFileExtensions, _savedStabilityDelay))
                    {
                        MessageBox.Show($"Auto-monitoring started successfully!\n\n" +
                                      $"Watch folder: {_savedWatchFolder}\n" +
                                      $"Destination: {_savedDestinationFolder}\n" +
                                      $"Extensions: {_savedFileExtensions}\n" +
                                      $"Stability delay: {_savedStabilityDelay} seconds", 
                                      "Monitoring Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoggingService.LogInfo($"Auto-monitoring started - Watch: {_savedWatchFolder}, Dest: {_savedDestinationFolder}", "AutoMonitor");
                    }
                    else
                    {
                        MessageBox.Show("Failed to start monitoring. Please check your configuration.", 
                            "Start Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void miMonitoringStatus_Click(object sender, EventArgs e)
        {
            if (_folderMonitorService == null)
            {
                MessageBox.Show("Monitoring service not initialized.", "Status", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string status = _folderMonitorService.IsMonitoring ? "Running" : "Stopped";
            
            string statusDetails = $"Auto-Monitoring Status: {status}\n\n";
            
            if (!string.IsNullOrWhiteSpace(_savedWatchFolder))
            {
                statusDetails += $"Watch Folder: {_savedWatchFolder}\n";
                statusDetails += $"Destination: {_savedDestinationFolder}\n";
                statusDetails += $"File Extensions: {_savedFileExtensions}\n";
                statusDetails += $"Stability Delay: {_savedStabilityDelay} seconds\n";
                statusDetails += $"Auto-start on Load: {(_savedAutoStart ? "Yes" : "No")}\n\n";
                
                if (_folderMonitorService.IsMonitoring)
                {
                    statusDetails += "The service is actively monitoring for new TV episode files.";
                }
                else
                {
                    statusDetails += "Use 'Start/Stop Monitoring' to begin monitoring.";
                }
            }
            else
            {
                statusDetails += "No monitoring configuration found.\n";
                statusDetails += "Use 'Configure Auto-Monitor' to set up monitoring.";
            }
            
            MessageBox.Show(statusDetails, "Monitoring Status", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Movie scan functionality - scan for movie files
        /// </summary>
        private async void toolStripButton1_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Scanning for movies...";
            statusProgress.Value = 0;
            
            frmScanMovies options = new frmScanMovies();
            DialogResult dr = options.ShowDialog();
            if (dr == DialogResult.OK)
            {
                // Create cancellation token for long operations
                using var cts = new CancellationTokenSource();
                
                try
                {
                    // Use async file enumeration for better performance
                    var allFiles = await Task.Run(() => 
                        Directory.EnumerateFiles(options.VideoPath, "*.*", SearchOption.AllDirectories)
                        .Where(f => f.EndsWith(options.FileExtension, StringComparison.OrdinalIgnoreCase))
                        .ToArray(), cts.Token);
                    
                    statusProgress.Maximum = allFiles.Length;
                    statusProgress.Value = 0;
                    
                    int totalVideoFiles = 0;
                    int likelyTVShowsSkipped = 0;
                    int validMovies = 0;
                    int unparsedFiles = 0;

                    // Process files in batches for better responsiveness
                    const int batchSize = 10;
                    for (int i = 0; i < allFiles.Length; i += batchSize)
                    {
                        var batch = allFiles.Skip(i).Take(batchSize);
                        
                        await Task.Run(() => {
                            foreach (string s in batch)
                            {
                                if (cts.Token.IsCancellationRequested)
                                    return;
                                    
                                ProcessMovieFileForScan(s, options, ref totalVideoFiles, ref likelyTVShowsSkipped, 
                                                       ref validMovies, ref unparsedFiles);
                            }
                        }, cts.Token);
                        
                        // Update UI on main thread
                        statusProgress.Value = Math.Min(i + batchSize, allFiles.Length);
                        Application.DoEvents();
                    }
                    
                    statusLabel.Text = $"Movie scan completed: {validMovies} movies found, {unparsedFiles} unparsed, {likelyTVShowsSkipped} TV shows skipped from {totalVideoFiles} total files at {DateTime.Now:HH:mm:ss}";
                    
                    // Show summary if useful
                    if (totalVideoFiles > 0)
                    {
                        await ShowMovieScanSummary(totalVideoFiles, validMovies, unparsedFiles, likelyTVShowsSkipped);
                    }
                }
                catch (OperationCanceledException)
                {
                    statusLabel.Text = "Movie scan cancelled by user";
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied to source directory. Please check permissions.", 
                        "Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Movie scan failed - Access denied";
                }
                catch (DirectoryNotFoundException)
                {
                    MessageBox.Show("Source directory not found. Please verify the path.", 
                        "Directory Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Movie scan failed - Directory not found";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during movie scan: {ex.Message}", 
                        "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Movie scan failed - Error occurred";
                    Debug.WriteLine($"Movie scan error: {ex}");
                }
            }
            else
            {
                statusLabel.Text = "Movie scan cancelled";
                Debug.WriteLine("Movie scan dialog cancelled");
            }
        }

        private void ProcessMovieFileForScan(string filePath, frmScanMovies options, 
            ref int totalVideoFiles, ref int likelyTVShowsSkipped, 
            ref int validMovies, ref int unparsedFiles)
        {
            totalVideoFiles++;
            
            // Check if this file is likely a TV show and should be skipped
            int tvConfidence = MediaTypeDetector.GetTVEpisodeConfidence(filePath);
            int movieConfidence = MediaTypeDetector.GetMovieConfidence(filePath);
            
            // Skip files that seem more like TV shows than movies
            if (tvConfidence > movieConfidence && tvConfidence > 40)
            {
                likelyTVShowsSkipped++;
                Debug.WriteLine($"Skipping likely TV show: {Path.GetFileName(filePath)} (TV: {tvConfidence}%, Movie: {movieConfidence}%)");
                return;
            }
            
            MovieFile mv = new MovieFile(filePath, options.OutputDirectory);
            
            // Create local variables to avoid ref parameter issue in lambda
            int localValidMovies = validMovies;
            int localUnparsedFiles = unparsedFiles;
            
            // Process on main thread since it involves UI updates
            Invoke(new Action(() => {
                AddMovieToList(mv, ref localValidMovies, ref localUnparsedFiles, tvConfidence, movieConfidence);
            }));
            
            // Update the original ref parameters
            validMovies = localValidMovies;
            unparsedFiles = localUnparsedFiles;
        }

        private void AddMovieToList(MovieFile mv, ref int validMovies, ref int unparsedFiles, 
            int tvConfidence, int movieConfidence)
        {
            // Only exclude movies with completely invalid parsing
            if (!string.IsNullOrWhiteSpace(mv.MovieName) && mv.MovieName != "Unknown Movie")
            {
                validMovies++;
                ListViewItem lvi = new ListViewItem();
                lvi.Text = mv.FileNamePath ?? "Unknown";
                lvi.SubItems.Add(mv.NewDestDirectory ?? "");
                lvi.SubItems.Add(mv.MovieName ?? "Unknown");
                lvi.SubItems.Add(mv.MovieYear ?? "");
                lvi.SubItems.Add($"Movie: {movieConfidence}%, TV: {tvConfidence}%");
                lvi.Tag = mv;
                lvMovies.Items.Add(lvi);
            }
            else
            {
                // Only add unparsed files that have some movie characteristics
                if (movieConfidence > 20 || tvConfidence < 30)
                {
                    unparsedFiles++;
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = mv.FileNamePath ?? "Unknown";
                    lvi.SubItems.Add("UNPARSED - Please review");
                    lvi.SubItems.Add(mv.MovieName ?? "Unknown");
                    lvi.SubItems.Add(mv.MovieYear ?? "");
                    lvi.SubItems.Add("Not processed");
                    lvi.BackColor = Color.LightGray;
                    lvi.Tag = mv;
                    lvMovies.Items.Add(lvi);
                }
                else
                {
                    Debug.WriteLine($"Skipping unparsed likely TV show: {Path.GetFileName(mv.FileNamePath)}");
                }
            }
        }

        private async Task ShowMovieScanSummary(int totalVideoFiles, int validMovies, int unparsedFiles, int likelyTVShowsSkipped)
        {
            await Task.Delay(100); // Brief delay for UI responsiveness
            
            string summary = $"Movie Scan Results:\n" +
                           $"Total video files: {totalVideoFiles}\n" +
                           $"Valid movies: {validMovies}\n" +
                           $"Unparsed files: {unparsedFiles}\n" +
                           $"TV shows skipped: {likelyTVShowsSkipped}";
            
            if (unparsedFiles > 0)
            {
                summary += "\n\nTip: Review unparsed files (gray) and remove them if they're not movies.";
            }
            
            MessageBox.Show(summary, "Movie Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Movie processing functionality - process movie files
        /// </summary>
        private async void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (lvMovies.Items.Count == 0)
            {
                MessageBox.Show("No movies to process. Please scan for movie files first.", 
                    "No Files", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Pre-processing validation
            if (!ValidateBeforeProcessing(false))
                return;

            // Ask for user confirmation
            int validItems = lvMovies.Items.Cast<ListViewItem>()
                .Count(item => item.BackColor != Color.LightGray);
            
            var result = MessageBox.Show(
                $"Process {validItems} movie(s)?\n\nThis will:\n" +
                "1. Copy files to the destination server\n" +
                "2. Organize movies alphabetically\n" +
                "3. Verify file sizes match\n" +
                "4. Delete original files if verification succeeds\n\n" +
                "Continue?", 
                "Confirm Movie Processing", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
                
            if (result != DialogResult.Yes)
                return;

            // Initialize processing with cancellation support
            using var cts = new CancellationTokenSource();
            var progress = new Progress<ProcessingProgress>(UpdateProcessingProgress);
            
            try
            {
                await ProcessMovieFilesAsync(validItems, progress, cts.Token);
            }
            catch (OperationCanceledException)
            {
                statusLabel.Text = "Movie processing cancelled by user";
                LoggingService.LogInfo("Movie processing cancelled by user", "Processing");
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Movie processing failed with error";
                LoggingService.LogError("Movie processing failed", ex, "Processing");
                MessageBox.Show($"Movie processing failed: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ProcessMovieFilesAsync(int totalItems, IProgress<ProcessingProgress> progress, CancellationToken cancellationToken)
        {
            int processedCount = 0;
            int successfulCopies = 0;
            int successfulDeletions = 0;
            int errors = 0;
            
            statusProgress.Maximum = totalItems;
            DateTime startTime = DateTime.Now;
            
            var itemsToProcess = lvMovies.Items.Cast<ListViewItem>()
                .Where(item => item.BackColor != Color.LightGray)
                .ToList();

            // Phase 1: Copy files
            LoggingService.LogInfo($"Starting to process {totalItems} movie files", "Processing");
            
            foreach (var lvi in itemsToProcess)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                MovieFile mv = (MovieFile)lvi.Tag;
                processedCount++;
                
                var currentProgress = new ProcessingProgress
                {
                    CurrentFile = Path.GetFileName(mv.FileNamePath),
                    ProcessedCount = processedCount,
                    TotalCount = totalItems,
                    Phase = ProcessingPhase.Copying,
                    Message = $"Copying movie {processedCount}/{totalItems}: {Path.GetFileName(mv.FileNamePath)}"
                };
                
                progress.Report(currentProgress);
                
                try
                {
                    string destinationDir = Path.GetDirectoryName(mv.NewDestDirectory);
                    if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                    {
                        LoggingService.LogDebug($"Creating directory: {destinationDir}", "Processing");
                        Directory.CreateDirectory(destinationDir);
                    }

                    // Enhanced retry logic with async
                    bool retVal = await RetryFileOperationAsync(
                        () => CopyFileAsync(mv.FileNamePath, mv.NewDestDirectory),
                        maxRetries: 5,
                        cancellationToken);
                    
                    // Update UI on the UI thread
                    Invoke(new Action(() => {
                        if (retVal)
                        {
                            lvi.BackColor = Color.Yellow;
                            successfulCopies++;
                            LoggingService.LogDebug($"Successfully copied movie: {Path.GetFileName(mv.FileNamePath)}", "Processing");
                        }
                        else
                        {
                            lvi.BackColor = Color.Red;
                            lvi.SubItems[4].Text = "Copy failed after retries";
                            errors++;
                            LoggingService.LogError($"Failed to copy movie after retries: {mv.FileNamePath}", null, "Processing");
                        }
                    }));
                }
                catch (Exception ex)
                {
                    // Update UI on the UI thread
                    Invoke(new Action(() => {
                        lvi.BackColor = Color.Red;
                        lvi.SubItems[4].Text = $"Error: {ex.Message}";
                        errors++;
                    }));
                    LoggingService.LogError($"Error processing movie {mv.FileNamePath}", ex, "Processing");
                }
                
                // Update UI
                Invoke(new Action(() => {
                    lvMovies.EnsureVisible(itemsToProcess.IndexOf(lvi));
                    lvMovies.Refresh();
                }));
            }
            
            // Phase 2: Verify and cleanup
            LoggingService.LogInfo("Starting movie verification and cleanup phase", "Processing");
            processedCount = 0;
            
            foreach (var lvi in itemsToProcess.Where(item => item.BackColor == Color.Yellow))
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                MovieFile mv = (MovieFile)lvi.Tag;
                processedCount++;
                
                var currentProgress = new ProcessingProgress
                {
                    CurrentFile = Path.GetFileName(mv.FileNamePath),
                    ProcessedCount = processedCount,
                    TotalCount = successfulCopies,
                    Phase = ProcessingPhase.Verifying,
                    Message = $"Verifying and cleaning up movie {processedCount}/{successfulCopies}"
                };
                
                progress.Report(currentProgress);
                
                try
                {
                    await Task.Run(() => {
                        FileInfo fiLocal = new FileInfo(mv.FileNamePath);
                        FileInfo fiRemote = new FileInfo(mv.NewDestDirectory);
                        
                        // Prepare the UI updates on the background thread but don't execute them yet
                        string statusMessage;
                        Color statusColor;
                        
                        if (!fiRemote.Exists)
                        {
                            statusMessage = "Remote file not found";
                            statusColor = Color.Red;
                            errors++;
                            LoggingService.LogError($"Remote movie file not found: {mv.NewDestDirectory}", null, "Processing");
                        }
                        else if (fiLocal.Length == fiRemote.Length)
                        {
                            File.Delete(mv.FileNamePath);
                            statusMessage = "Completed - Original deleted";
                            statusColor = Color.LightGreen;
                            successfulDeletions++;
                            LoggingService.LogInfo($"Successfully processed movie: {Path.GetFileName(mv.FileNamePath)}", "Processing");
                        }
                        else
                        {
                            statusMessage = $"Size mismatch - Local: {fiLocal.Length}, Remote: {fiRemote.Length}";
                            statusColor = Color.Orange;
                            errors++;
                            LoggingService.LogWarning($"Size mismatch for movie {mv.FileNamePath}: Local={fiLocal.Length}, Remote={fiRemote.Length}", "Processing");
                        }
                        
                        // Now update the UI on the UI thread
                        Invoke(new Action(() => {
                            lvi.SubItems[4].Text = statusMessage;
                            lvi.BackColor = statusColor;
                        }));
                        
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Update UI on the UI thread
                    Invoke(new Action(() => {
                        lvi.SubItems[4].Text = $"Cleanup error: {ex.Message}";
                        lvi.BackColor = Color.Red;
                        errors++;
                    }));
                    LoggingService.LogError($"Cleanup error for movie {mv.FileNamePath}", ex, "Processing");
                }
            }
            
            // Final summary
            TimeSpan duration = DateTime.Now - startTime;
            string finalMessage = $"Movie processing completed in {duration.Minutes}m {duration.Seconds}s - " +
                                $"Copied: {successfulCopies}, Deleted: {successfulDeletions}, Errors: {errors}";
            
            progress.Report(new ProcessingProgress
            {
                Phase = ProcessingPhase.Complete,
                Message = finalMessage,
                ProcessedCount = totalItems,
                TotalCount = totalItems
            });
            
            LoggingService.LogInfo($"Movie processing completed: Copied={successfulCopies}, Deleted={successfulDeletions}, Errors={errors}, Duration={duration}", "Processing");
            
            // Show completion summary
            string summary = $"Movie Processing Complete!\n\n" +
                           $"Movies processed: {totalItems}\n" +
                           $"Successfully copied: {successfulCopies}\n" +
                           $"Original files deleted: {successfulDeletions}\n" +
                           $"Errors: {errors}\n" +
                           $"Duration: {duration.Minutes}m {duration.Seconds}s";
                           
            MessageBox.Show(summary, "Movie Processing Complete", 
                MessageBoxButtons.OK, 
                errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        /// <summary>
        /// Movie cleaner scan functionality - scan for movies that need title cleanup
        /// </summary>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Movie cleaner functionality is under development.\n\n" +
                          "This feature will scan for movie files that have messy titles and help clean them up automatically.\n\n" +
                          "Current features available:\n" +
                          "• Movie scan and processing (working)\n" +
                          "• TV episode scan and processing (working)\n" +
                          "• Auto-monitoring (working)", 
                          "Movie Cleaner - Coming Soon", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Movie year processing functionality - process movies with year detection
        /// </summary>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Movie year processing functionality is under development.\n\n" +
                          "This feature will help organize movies by year and handle year detection issues.\n\n" +
                          "Current movie processing already includes:\n" +
                          "• Year extraction from filenames\n" +
                          "• Alphabetical organization\n" +
                          "• Quality tag removal\n\n" +
                          "Use the regular Movie scan and process buttons for full movie functionality.", 
                          "Movie Year Processing - Coming Soon", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsbAutoMonitor_Click(object sender, EventArgs e)
        {
            miAutoMonitor_Click(sender, e);
        }

        private void tsbForceDateCheck_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Force date check functionality is currently disabled.", 
                "Feature Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void miShowHelp_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        private void miAbout_Click(object sender, EventArgs e)
        {
            string aboutText = $@"Torrent File Renamer
Version 2.0 Enhanced

Features:
• TV Episode Organization (S01E01 format)
• Movie Organization (Alphabetical)
• Smart File Type Detection
• Automatic HandBrake Monitoring
• Multi-select Operations
• Path Validation & Space Checking
• Network Drive Support
• Plex Compatibility Validation

© 2024 Enhanced for .NET 8
Built with intelligent media detection";

            MessageBox.Show(aboutText, "About Torrent File Renamer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}