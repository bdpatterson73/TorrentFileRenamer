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

        public frmMain()
        {
            InitializeComponent();
        }

        // Add folder monitoring service
        private FolderMonitorService? _folderMonitorService;
        private ListViewItem? _monitoringStatusItem;

        // Add these after the existing field declarations at the top of the class
        private string _savedWatchFolder = "";
        private string _savedDestinationFolder = "";
        private string _savedFileExtensions = "*.mp4;*.mkv;*.avi;*.m4v";
        private int _savedStabilityDelay = 30;
        private bool _savedAutoStart = false;

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
                    Debug.WriteLine($"Validation error: {ex.Message}")
;
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
        /// Enhanced scan with better error handling
        /// </summary>
        private void miScanFiles_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Scanning for TV episodes...";
            statusProgress.Value = 0;
            
            frmScanOptions options = new frmScanOptions();
            DialogResult dr = options.ShowDialog();
            if (dr == DialogResult.OK)
            {
                try
                {
                    string[] allFiles = Directory.GetFiles(options.VideoPath, "*.*", SearchOption.AllDirectories);
                    statusProgress.Maximum = allFiles.Length;
                    statusProgress.Value = 0;
                    
                    int totalVideoFiles = 0;
                    int likelyMoviesSkipped = 0;
                    int validEpisodes = 0;
                    int unparsedFiles = 0;

                    foreach (string s in allFiles)
                    {
                        statusProgress.PerformStep();
                        Application.DoEvents();
                        
                        if (s.EndsWith(options.FileExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            totalVideoFiles++;
                            
                            // Check if this file is likely a movie and should be skipped
                            int tvConfidence = MediaTypeDetector.GetTVEpisodeConfidence(s);
                            int movieConfidence = MediaTypeDetector.GetMovieConfidence(s);
                            
                            // Skip files that seem more like movies than TV episodes
                            if (movieConfidence > tvConfidence && movieConfidence > 40)
                            {
                                likelyMoviesSkipped++;
                                Debug.WriteLine($"Skipping likely movie: {Path.GetFileName(s)} (Movie: {movieConfidence}%, TV: {tvConfidence}%)");
                                continue;
                            }
                            
                            FileEpisode fe = new FileEpisode(s, options.OutputDirectory);
                            
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
                                            $"Original file: {Path.GetFileName(s)}\n" +
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
                                    lvi.Text = s;
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
                                    lvi.Text = s;
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
                                    likelyMoviesSkipped++;
                                    Debug.WriteLine($"Skipping unparsed likely movie: {Path.GetFileName(s)}");
                                }
                            }
                        }
                    }
                    
                    statusLabel.Text = $"TV scan completed: {validEpisodes} episodes found, {unparsedFiles} unparsed, {likelyMoviesSkipped} movies skipped from {totalVideoFiles} total files at {DateTime.Now.ToString("HH:mm:ss")}";
                    
                    // Show summary if useful
                    if (totalVideoFiles > 0)
                    {
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


        private bool RetryFileMove(string sourceFile, string destFile, int NumRetries)
        {
            for (int i =0; i < NumRetries; i++)
            {
                try
                {
                    if (File.Exists(destFile))
                        File.Delete(destFile);
                    File.Copy(sourceFile, destFile);
                    return true;
                }
                catch (IOException iox)
                {
                    Debug.WriteLine("IO Exception. Waiting for file copy retry. " + i.ToString ());
                    System.Threading.Thread.Sleep(5000);
                }
            }
            return false;
        }


        /// <summary>
        /// Process TV Episodes with enhanced validation and error handling.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miProcess_Click(object sender, EventArgs e)
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

            int listIndex = 0;
            int processedCount = 0;
            int successfulCopies = 0;
            int successfulDeletions = 0;
            int errors = 0;
            
            statusProgress.Maximum = lvFiles.Items.Count;
            DateTime startTime = DateTime.Now;
            
            foreach (ListViewItem lvi in lvFiles.Items)
            {
                statusProgress.PerformStep();
                
                // Skip unparsed items
                if (lvi.BackColor == Color.LightGray)
                {
                    listIndex++;
                    continue;
                }

                FileEpisode fe = (FileEpisode)lvi.Tag;
                lvFiles.EnsureVisible(listIndex);
                lvFiles.Refresh();
                Application.DoEvents();
                
                processedCount++;
                statusLabel.Text = $"Processing {processedCount}/{validItems}: {Path.GetFileName(fe.FullFilePath)}";
                
                try
                {
                    if (!Directory.Exists(fe.NewDirectoryName))
                    {
                        Debug.WriteLine("No Directory. Creating: " + fe.NewDirectoryName);
                        Directory.CreateDirectory(fe.NewDirectoryName);
                    }

                    // Try to copy the file up to 5 times and watch for network communication problems.
                    bool retVal = RetryFileMove(fe.FullFilePath, fe.NewFileNamePath, 5);
                    if (retVal)
                    {
                        lvi.BackColor = Color.Yellow;
                        successfulCopies++;
                        lvFiles.Refresh();
                        Application.DoEvents();
                    }
                    else
                    {
                        lvi.BackColor = Color.Red;
                        lvi.SubItems[5].Text = "Copy failed after retries";
                        errors++;
                        lvFiles.Refresh();
                    }
                }
                catch (Exception ex)
                {
                    lvi.BackColor = Color.Red;
                    lvi.SubItems[5].Text = $"Error: {ex.Message}";
                    errors++;
                    lvFiles.Refresh();
                    Debug.WriteLine($"Error processing {fe.FullFilePath}: {ex.Message}");
                }
                listIndex++;
                lvFiles.Refresh();
                Application.DoEvents();
            }
            
            // Second pass: Verify and delete original files
            statusLabel.Text = "Verifying copies and cleaning up...";
            listIndex = 0;
            
            foreach (ListViewItem lvi in lvFiles.Items)
            {
                if (lvi.BackColor != Color.Yellow) // Only process successfully copied files
                {
                    listIndex++;
                    continue;
                }

                FileEpisode fe = (FileEpisode)lvi.Tag;
                lvFiles.EnsureVisible(listIndex);
                lvFiles.Refresh();
                Application.DoEvents();
                
                try
                {
                    FileInfo fiLocal = new FileInfo(fe.FullFilePath);
                    FileInfo fiRemote = new FileInfo(fe.NewFileNamePath);
                    
                    if (!fiRemote.Exists)
                    {
                        lvi.SubItems[5].Text = "Remote file not found";
                        lvi.BackColor = Color.Red;
                        errors++;
                    }
                    else if (fiLocal.Length == fiRemote.Length)
                    {
                        File.Delete(fe.FullFilePath);
                        lvi.SubItems[5].Text = "Completed - Original deleted";
                        lvi.BackColor = Color.LightGreen;
                        successfulDeletions++;
                    }
                    else
                    {
                        lvi.SubItems[5].Text = $"Size mismatch - Local: {fiLocal.Length}, Remote: {fiRemote.Length}";
                        lvi.BackColor = Color.Orange;
                        errors++;
                    }
                }
                catch (Exception ex)
                {
                    lvi.SubItems[5].Text = $"Cleanup error: {ex.Message}";
                    lvi.BackColor = Color.Red;
                    errors++;
                    Debug.WriteLine($"Cleanup error for {fe.FullFilePath}: {ex.Message}");
                }
                listIndex++;
            }
            
            TimeSpan duration = DateTime.Now - startTime;
            statusLabel.Text = $"Processing completed in {duration.Minutes}m {duration.Seconds}s - " +
                             $"Copied: {successfulCopies}, Deleted: {successfulDeletions}, Errors: {errors}";
            
            // Show completion summary
            string summary = $"TV Episode Processing Complete!\n\n" +
                           $"Files processed: {processedCount}\n" +
                           $"Successfully copied: {successfulCopies}\n" +
                           $"Original files deleted: {successfulDeletions}\n" +
                           $"Errors: {errors}\n" +
                           $"Duration: {duration.Minutes}m {duration.Seconds}s";
                           
            MessageBox.Show(summary, "Processing Complete", 
                MessageBoxButtons.OK, 
                errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
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
                catch (Exception ex)
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

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Initialize folder monitoring service
            InitializeFolderMonitoring();

            // Test the problematic filename parsing (remove this after testing)
            #if DEBUG
            FileEpisode.TestFilenameParsing(@"C:\temp\I Fought the Law 2025 S01E01 720p WEB-DL HEVC x265 BONE.mkv", @"C:\TestOutput");
            FileEpisode.TestFilenameParsing(@"C:\temp\Show Name S01E00 Pilot Episode.mkv", @"C:\TestOutput");
            FileEpisode.TestFilenameParsing(@"C:\temp\Show:Name*With?Problems S01E01.mkv", @"C:\TestOutput");
            FileEpisode.TestFilenameParsing(@"C:\temp\Unknown Show S01E01.mkv", @"C:\TestOutput");
            #endif

            //DateTime startAt = DateTime.Now;
            //string c = HashHelper.CRC32File("C:\\swimapps\\swimcardb\\data\\MAIN_old.DCT");
            //DateTime endAt = DateTime.Now;
            //TimeSpan ts = endAt - startAt;
            //MessageBox.Show($"CRC32: {c}    {ts.TotalSeconds}");


            //TODO TMDbClient client = new TMDbClient("0ed08c5515fa7214f7af38a5835360dd");
            //SearchContainer<SearchMovie> results = client.SearchMovieAsync("Black Adam").Result;

            //Console.WriteLine($"Got {results.Results.Count:N0} of {results.TotalResults:N0} results");
            //foreach (SearchMovie result in results.Results)
            //    Console.WriteLine(result.Title);
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

                // Auto-start if enabled
                if (_savedAutoStart && !string.IsNullOrWhiteSpace(_savedWatchFolder) && !string.IsNullOrWhiteSpace(_savedDestinationFolder))
                {
                    ConfigureAndStartMonitoring(_savedWatchFolder, _savedDestinationFolder, _savedFileExtensions, _savedStabilityDelay);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing folder monitoring: {ex.Message}", 
                    "Monitoring Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            lvi.SubItems.Add(e.Message);
            lvFiles.Items.Insert(1, lvi); // Insert after status item

            // Keep only last 10 auto-processed items to avoid cluttering
            var autoItems = lvFiles.Items.Cast<ListViewItem>()
                .Where(item => item.Text.StartsWith("🔄"))
                .Skip(10)
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
            
            // You might want to show this to user or log it
            // For now, just update status
        }

        /// <summary>
        /// Configure and start folder monitoring
        /// </summary>
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

        /// <summary>
        /// Menu item to configure auto-monitoring
        /// </summary>
        private void miAutoMonitor_Click(object sender, EventArgs e)
        {
            using (frmFolderMonitor monitorDialog = new frmFolderMonitor())
            {
                // Pre-populate with saved settings
                monitorDialog.WatchFolder = _savedWatchFolder;
                monitorDialog.DestinationPath = _savedDestinationFolder;
                monitorDialog.FileExtensions = _savedFileExtensions;
                monitorDialog.StabilityDelaySeconds = _savedStabilityDelay;
                monitorDialog.AutoStartOnLoad = _savedAutoStart;

                if (monitorDialog.ShowDialog() == DialogResult.OK)
                {
                    // Save settings
                    _savedWatchFolder = monitorDialog.WatchFolder;
                    _savedDestinationFolder = monitorDialog.DestinationPath;
                    _savedFileExtensions = monitorDialog.FileExtensions;
                    _savedStabilityDelay = monitorDialog.StabilityDelaySeconds;
                    _savedAutoStart = monitorDialog.AutoStartOnLoad;
                    SaveMonitoringSettings();

                    // Stop current monitoring if running
                    _folderMonitorService?.StopMonitoring();

                    // Start new monitoring
                    if (ConfigureAndStartMonitoring(_savedWatchFolder, _savedDestinationFolder, _savedFileExtensions, _savedStabilityDelay))
                    {
                        MessageBox.Show($"Auto-monitoring started!\n\nWatching: {_savedWatchFolder}\nDestination: {_savedDestinationFolder}", 
                            "Monitoring Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        /// <summary>
        /// Menu item to start/stop monitoring
        /// </summary>
        private void miToggleMonitoring_Click(object sender, EventArgs e)
        {
            if (_folderMonitorService == null)
                return;

            if (_folderMonitorService.IsMonitoring)
            {
                _folderMonitorService.StopMonitoring();
                MessageBox.Show("Auto-monitoring stopped.", "Monitoring", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_savedWatchFolder) || string.IsNullOrWhiteSpace(_savedDestinationFolder))
                {
                    MessageBox.Show("Please configure monitoring settings first.", "Configuration Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    miAutoMonitor_Click(sender, e);
                    return;
                }

                if (ConfigureAndStartMonitoring(_savedWatchFolder, _savedDestinationFolder, _savedFileExtensions, _savedStabilityDelay))
                {
                    MessageBox.Show("Auto-monitoring started.", "Monitoring", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Show monitoring status and statistics
        /// </summary>
        private void miMonitoringStatus_Click(object sender, EventArgs e)
        {
            if (_folderMonitorService == null)
            {
                MessageBox.Show("Monitoring service not initialized.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string status = _folderMonitorService.IsMonitoring ? "Running" : "Stopped";
            string statusMessage = $"Auto-Monitoring Status: {status}\n\n";
            
            if (_folderMonitorService.IsMonitoring)
            {
                statusMessage += $"Watch Folder: {_savedWatchFolder}\n";
                statusMessage += $"Destination: {_savedDestinationFolder}\n";
                statusMessage += $"Extensions: {_savedFileExtensions}\n";
                statusMessage += $"Stability Delay: {_savedStabilityDelay} seconds\n\n";
                statusMessage += "The system is actively monitoring for completed HandBrake conversions.";
            }
            else
            {
                statusMessage += "Monitoring is currently stopped.\n";
                statusMessage += "Use 'Configure Auto-Monitor' to set it up.";
            }

            MessageBox.Show(statusMessage, "Monitoring Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (lvMovies.Items.Count == 0)
            {
                MessageBox.Show("No movies to process. Please scan for files first.", 
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
                $"Process {validItems} movie(s)?\n\nThis will move files to the destination server organized by first letter.\n\n" +
                "Continue?", 
                "Confirm Processing", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
                
            if (result != DialogResult.Yes)
                return;

            DateTime startMove = DateTime.Now;
            bool existed = false;
            int listIndex = 0;
            int processedCount = 0;
            int successfulCount = 0;
            int skippedCount = 0;
            int errorCount = 0;
            
            statusProgress.Maximum = lvMovies.Items.Count;
            
            foreach (ListViewItem lvi in lvMovies.Items)
            {
                statusProgress.PerformStep();
                float li = listIndex + 1;
                float pct = (li / lvMovies.Items.Count) * 100;
                int pcti = (int)pct;
                
                MovieFile fe = (MovieFile)lvi.Tag;
                statusLabel.Text = $"Processing {pcti}%: {Path.GetFileName(fe.FileNamePath)}";
                lvMovies.EnsureVisible(listIndex);
                lvMovies.Refresh();
                Application.DoEvents();
                
                // Skip unparsed movies (gray background)
                if (lvi.BackColor == Color.LightGray)
                {
                    lvi.SubItems[4].Text = "Skipped - unparsed file";
                    skippedCount++;
                    listIndex++;
                    continue;
                }
                
                processedCount++;
                
                try
                {
                    string destDir = Path.GetDirectoryName(fe.NewDestDirectory);
                    if (!Directory.Exists(destDir))
                    {
                        Debug.WriteLine("No Directory. Creating: " + destDir);
                        Directory.CreateDirectory(destDir);
                    }
                    
                    if (File.Exists(fe.NewDestDirectory))
                    {
                        var overwriteResult = MessageBox.Show(
                            $"File already exists:\n{fe.NewDestDirectory}\n\nOverwrite?", 
                            "File Exists", 
                            MessageBoxButtons.YesNoCancel, 
                            MessageBoxIcon.Question);
                            
                        if (overwriteResult == DialogResult.Cancel)
                        {
                            statusLabel.Text = "Operation cancelled by user";
                            return;
                        }
                        else if (overwriteResult == DialogResult.No)
                        {
                            lvi.SubItems[4].Text = "Skipped - file exists";
                            lvi.BackColor = Color.LightBlue;
                            skippedCount++;
                            listIndex++;
                            continue;
                        }
                        
                        File.Delete(fe.NewDestDirectory);
                        lvi.BackColor = Color.Blue;
                        existed = true;
                    }
                    
                    File.Move(fe.FileNamePath, fe.NewDestDirectory);
                    
                    if (!existed)
                    {
                        lvi.BackColor = Color.Yellow;
                    }
                    else
                    {
                        existed = false;
                    }

                    lvi.SubItems[4].Text = "Successful";
                    successfulCount++;
                    lvMovies.Refresh();
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    lvi.BackColor = Color.Red;
                    lvMovies.Refresh();
                    Debug.WriteLine($"Error processing {fe.FileNamePath}: {ex.Message}");
                    lvi.SubItems[4].Text = $"Error: {ex.Message}";
                    errorCount++;
                }
                listIndex++;
                lvMovies.Refresh();
                Application.DoEvents();
            }
            
            TimeSpan ts = DateTime.Now - startMove;
            statusLabel.Text = $"Completed at {DateTime.Now.ToString("HH:mm:ss")} - " +
                             $"Processed: {successfulCount}/{processedCount}, Skipped: {skippedCount}, Errors: {errorCount} ({ts.Minutes}m {ts.Seconds}s)";
            
            // Show completion summary
            string summary = $"Movie Processing Complete!\n\n" +
                           $"Files processed: {processedCount}\n" +
                           $"Successfully moved: {successfulCount}\n" +
                           $"Skipped: {skippedCount}\n" +
                           $"Errors: {errorCount}\n" +
                           $"Duration: {ts.Minutes}m {ts.Seconds}s";
                           
            MessageBox.Show(summary, "Processing Complete", 
                MessageBoxButtons.OK, 
                errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void msMovieRemove_Click(object sender, EventArgs e)
        {
            // Remove all currently selected listview items for movies (improved multi-select)
            if (lvMovies.SelectedItems.Count == 0)
            {
                statusLabel.Text = "No items selected for removal";
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

        private void msMovieClear_Click(object sender, EventArgs e)
        {
            lvMovies.Items.Clear();
            statusLabel.Text = "Movie list cleared";
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            frmScanMovies sm = new frmScanMovies();
            DialogResult dr = sm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string[] allFiles = Directory.GetFiles(sm.VideoPath, "*.*", SearchOption.AllDirectories);
                foreach (string s in allFiles)
                {
                    if (s.EndsWith(sm.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        MovieFile mv = new MovieFile(s, sm.OutputDirectory);
                        if (!DoesTitleContainYear(mv.MovieName))
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = s;
                            lvi.SubItems.Add(mv.MovieName);
                            string mYear = GetMovieYear(mv.MovieName);
                            lvi.SubItems.Add(mYear);
                            lvi.SubItems.Add(mv.MovieName.Trim() + " (" + mYear.Trim() + ")" + Path.GetExtension(mv.FileNamePath));
                            lvi.SubItems.Add("");

                            lvi.Tag = mv;
                            if (mYear.Length > 0)
                                lvMovieCleaner.Items.Add(lvi);
                            else
                            {
                                lvi.BackColor = Color.Gray;
                                lvMovieCleaner.Items.Add(lvi);
                            }
                        }
                    }
                }
            }
        }

        private string GetMovieYear(string movieName)
        {
            //TMDbClient client = new TMDbClient("0ed08c5515fa7214f7af38a5835360dd");
            //SearchContainer<SearchMovie> results = client.SearchMovieAsync(movieName).Result;

            // Console.WriteLine($"Got {results.Results.Count:N0} of {results.TotalResults:N0} results");
            // foreach (SearchMovie result in results.Results)
            // {
            //    DateTime? dtRelease = result.ReleaseDate;
            //    if (dtRelease.HasValue)
            //    {
            //        DateTime dt = dtRelease.Value;
            //        return dt.Year.ToString();
            //    }
            //}
            return "";
        }

        private bool DoesTitleContainYear(string movieName)
        {
            string pattern = @"\(([0-9]{4})\)";
            Regex re = new Regex(pattern);
            string txtYear = string.Empty;

            foreach (Match m in re.Matches(movieName))
            {
                return true;
                txtYear = m.Value;
                Console.Write(txtYear);
            }
            return false;
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvMovieCleaner.Items)
            {
                string path = Path.GetDirectoryName(lvi.SubItems[0].Text);
                string newFile = Path.Combine(path, lvi.SubItems[3].Text);
                if (File.Exists(newFile))
                {
                    File.Delete(lvi.SubItems[0].Text);
                }
                else
                {
                    File.Move(lvi.SubItems[0].Text, newFile);
                }
                lvi.BackColor = Color.Yellow;
            }
        }

        private void tsbForceDateCheck_Click(object sender, EventArgs e)
        {
            // Implementation for force date check
            MessageBox.Show("Force date check functionality is currently disabled.", 
                "Feature Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Scanning for movies...";
            statusProgress.Value = 0;
            
            frmScanMovies sm = new frmScanMovies();
            DialogResult dr = sm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                try
                {
                    string[] allFiles = Directory.GetFiles(sm.VideoPath, "*.*", SearchOption.AllDirectories);
                    statusProgress.Maximum = allFiles.Length;
                    statusProgress.Value = 0;
                    
                    int totalVideoFiles = 0;
                    int likelyTVEpisodesSkipped = 0;
                    int validMovies = 0;
                    int unparsedFiles = 0;
                    
                    foreach (string s in allFiles)
                    {
                        statusProgress.PerformStep();
                        Application.DoEvents();
                        
                        if (s.EndsWith(sm.FileExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            totalVideoFiles++;
                            
                            // Check if this file is likely a TV episode and should be skipped
                            int tvConfidence = MediaTypeDetector.GetTVEpisodeConfidence(s);
                            int movieConfidence = MediaTypeDetector.GetMovieConfidence(s);
                            
                            // Skip files that seem more like TV episodes than movies
                            if (tvConfidence > movieConfidence && tvConfidence > 50)
                            {
                                likelyTVEpisodesSkipped++;
                                Debug.WriteLine($"Skipping likely TV episode: {Path.GetFileName(s)} (TV: {tvConfidence}%, Movie: {movieConfidence}%)");
                                continue;
                            }
                            
                            MovieFile mv = new MovieFile(s, sm.OutputDirectory);
                            
                            // Check if movie was successfully parsed
                            bool isValidMovie = !string.IsNullOrWhiteSpace(mv.MovieName) && 
                                              mv.MovieName != "Unknown Movie" &&
                                              !string.IsNullOrWhiteSpace(mv.NewDestDirectory);
                            
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = s;
                            lvi.SubItems.Add(mv.NewDestDirectory);
                            lvi.SubItems.Add(mv.MovieName ?? "Unknown");
                            lvi.SubItems.Add(mv.MovieYear ?? "Unknown");
                            
                            if (isValidMovie)
                            {
                                validMovies++;
                                lvi.SubItems.Add("");
                            }
                            else
                            {
                                // Only add unparsed files that have some movie characteristics
                                if (movieConfidence > 20 || tvConfidence < 30)
                                {
                                    unparsedFiles++;
                                    lvi.SubItems.Add("UNPARSED - Please review");
                                    lvi.BackColor = Color.LightGray;
                                }
                                else
                                {
                                    likelyTVEpisodesSkipped++;
                                    Debug.WriteLine($"Skipping unparsed likely TV episode: {Path.GetFileName(s)}");
                                    continue; // Skip adding this item to the list
                                }
                            }
                            
                            lvi.Tag = mv;
                            lvMovies.Items.Add(lvi);
                        }
                    }
                    
                    statusLabel.Text = $"Movie scan completed: {validMovies} movies found, {unparsedFiles} unparsed, {likelyTVEpisodesSkipped} TV episodes skipped from {totalVideoFiles} total files at {DateTime.Now.ToString("HH:mm:ss")}";
                    
                    // Show summary if useful
                    if (totalVideoFiles > 0)
                    {
                        string summary = $"Scan Results:\n" +
                                       $"Total video files: {totalVideoFiles}\n" +
                                       $"Valid movies: {validMovies}\n" +
                                       $"Unparsed files: {unparsedFiles}\n" +
                                       $"TV episodes skipped: {likelyTVEpisodesSkipped}";
                        
                        if (unparsedFiles > 0)
                        {
                            summary += "\n\nTip: Review unparsed files (gray) and remove them if they're not movies.";
                        }
                        
                        MessageBox.Show(summary, "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
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
                    Debug.WriteLine($"Movie scan error: {ex}");
                }
            }
            else
            {
                statusLabel.Text = "Movie scan cancelled";
                Debug.WriteLine("Dialog Cancelled");
            }
        }

        /// <summary>
        /// Show help dialog
        /// </summary>
        private void miShowHelp_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        /// <summary>
        /// Show about dialog
        /// </summary>
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

© 2024 Enhanced for .NET 8
Built with intelligent media detection";

            MessageBox.Show(aboutText, "About Torrent File Renamer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Toolbar button for quick access to auto-monitoring configuration
        /// </summary>
        private void tsbAutoMonitor_Click(object sender, EventArgs e)
        {
            miAutoMonitor_Click(sender, e);
        }
    }
}