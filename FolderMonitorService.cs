using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Service class for monitoring folders and automatically processing TV show files
    /// </summary>
    public class FolderMonitorService : IDisposable
    {
        private FileSystemWatcher? _fileWatcher;
        private System.Windows.Forms.Timer? _stabilityTimer;
        private Dictionary<string, DateTime> _pendingFiles;
        private bool _isMonitoring = false;
        private readonly object _lockObject = new object();

        // Configuration properties
        public string WatchFolder { get; set; } = "";
        public string DestinationFolder { get; set; } = "";
        public string[] FileExtensions { get; set; } = Array.Empty<string>();
        public int StabilityDelaySeconds { get; set; } = 30;

        // Events
        public event EventHandler<string>? StatusChanged;
        public event EventHandler<FileProcessedEventArgs>? FileProcessed;
        public event EventHandler<FileFoundEventArgs>? FileFound;
        public event EventHandler<Exception>? ErrorOccurred;

        public bool IsMonitoring => _isMonitoring;

        public FolderMonitorService()
        {
            _pendingFiles = new Dictionary<string, DateTime>();
            _stabilityTimer = new System.Windows.Forms.Timer();
            _stabilityTimer.Interval = 5000; // Check every 5 seconds
            _stabilityTimer.Tick += StabilityTimer_Tick;
        }

        public bool StartMonitoring()
        {
            try
            {
                if (_isMonitoring)
                {
                    StatusChanged?.Invoke(this, "Already monitoring");
                    return true;
                }

                if (string.IsNullOrWhiteSpace(WatchFolder) || !Directory.Exists(WatchFolder))
                {
                    StatusChanged?.Invoke(this, "Invalid watch folder");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(DestinationFolder))
                {
                    StatusChanged?.Invoke(this, "Invalid destination folder");
                    return false;
                }

                // Validate destination folder
                var validation = PathValidator.ValidateDestinationPath(DestinationFolder);
                if (!validation.IsValid)
                {
                    StatusChanged?.Invoke(this, $"Destination validation failed: {validation.Message}");
                    return false;
                }

                // Setup file system watcher
                _fileWatcher = new FileSystemWatcher(WatchFolder)
                {
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName,
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true
                };

                _fileWatcher.Created += FileWatcher_Created;
                _fileWatcher.Changed += FileWatcher_Changed;
                _fileWatcher.Error += FileWatcher_Error;

                _stabilityTimer.Start();
                _isMonitoring = true;

                StatusChanged?.Invoke(this, $"Started monitoring: {WatchFolder}");
                
                // Check for existing files
                CheckExistingFiles();
                
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                StatusChanged?.Invoke(this, $"Failed to start monitoring: {ex.Message}");
                return false;
            }
        }

        public void StopMonitoring()
        {
            try
            {
                if (!_isMonitoring)
                    return;

                _fileWatcher?.Dispose();
                _fileWatcher = null;
                _stabilityTimer?.Stop();
                
                lock (_lockObject)
                {
                    _pendingFiles.Clear();
                }

                _isMonitoring = false;
                StatusChanged?.Invoke(this, "Monitoring stopped");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        private void CheckExistingFiles()
        {
            try
            {
                foreach (string extension in FileExtensions)
                {
                    string pattern = extension.StartsWith("*") ? extension : "*" + extension;
                    string[] files = Directory.GetFiles(WatchFolder, pattern, SearchOption.TopDirectoryOnly);

                    foreach (string file in files)
                    {
                        if (IsVideoFile(file) && IsFileTVShow(file))
                        {
                            FileFound?.Invoke(this, new FileFoundEventArgs(file, "Existing file found"));
                            AddPendingFile(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        private void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            HandleFileEvent(e.FullPath, "Created");
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            HandleFileEvent(e.FullPath, "Changed");
        }

        private void FileWatcher_Error(object sender, ErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e.GetException());
            StatusChanged?.Invoke(this, $"File watcher error: {e.GetException().Message}");
        }

        private void HandleFileEvent(string filePath, string eventType)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;

                if (!IsVideoFile(filePath))
                    return;

                if (!IsFileTVShow(filePath))
                {
                    Debug.WriteLine($"Skipping non-TV file: {Path.GetFileName(filePath)}");
                    return;
                }

                FileFound?.Invoke(this, new FileFoundEventArgs(filePath, eventType));
                AddPendingFile(filePath);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                Debug.WriteLine($"Error handling file event for {filePath}: {ex.Message}");
            }
        }

        private void AddPendingFile(string filePath)
        {
            lock (_lockObject)
            {
                _pendingFiles[filePath] = DateTime.Now;
                Debug.WriteLine($"Added to pending: {Path.GetFileName(filePath)}");
            }
        }

        private void StabilityTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                List<string> readyFiles = new List<string>();
                DateTime cutoffTime = DateTime.Now.AddSeconds(-StabilityDelaySeconds);

                lock (_lockObject)
                {
                    var filesToCheck = _pendingFiles.Where(kvp => kvp.Value <= cutoffTime).ToList();
                    
                    foreach (var kvp in filesToCheck)
                    {
                        string filePath = kvp.Key;
                        
                        if (!File.Exists(filePath))
                        {
                            _pendingFiles.Remove(filePath);
                            continue;
                        }

                        if (IsFileStable(filePath))
                        {
                            readyFiles.Add(filePath);
                            _pendingFiles.Remove(filePath);
                        }
                    }
                }

                // Process ready files outside of lock
                foreach (string filePath in readyFiles)
                {
                    ProcessTVShowFile(filePath);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        private bool IsVideoFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLower();
            return FileExtensions.Any(ext => 
                string.Equals(ext.TrimStart('*'), extension, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsFileTVShow(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                int tvConfidence = MediaTypeDetector.GetTVEpisodeConfidence(fileName);
                int movieConfidence = MediaTypeDetector.GetMovieConfidence(fileName);

                // Must have good TV confidence and better than movie confidence
                return tvConfidence > 50 && tvConfidence > movieConfidence;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking if file is TV show: {ex.Message}");
                return false;
            }
        }

        private bool IsFileStable(string filePath)
        {
            try
            {
                // Check if file is still being written to by trying to open it exclusively
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // If we can open exclusively, file is stable
                    return stream.Length > 0; // Also ensure file has content
                }
            }
            catch (IOException)
            {
                // If we can't open exclusively, file is still being written
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking file stability: {ex.Message}");
                return false;
            }
        }

        private async void ProcessTVShowFile(string filePath)
        {
            try
            {
                StatusChanged?.Invoke(this, $"Processing: {Path.GetFileName(filePath)}");

                // Use existing FileEpisode class to parse and process
                FileEpisode episode = new FileEpisode(filePath, DestinationFolder);

                // Allow episode 0 for special episodes - only exclude if parsing completely failed
                if (episode.EpisodeNumber < 0 || string.IsNullOrWhiteSpace(episode.ShowName) || episode.ShowName == "Unknown Show")
                {
                    FileProcessed?.Invoke(this, new FileProcessedEventArgs(filePath, false, "Could not parse episode information"));
                    return;
                }

                // Create destination directory if needed
                if (!Directory.Exists(episode.NewDirectoryName))
                {
                    Directory.CreateDirectory(episode.NewDirectoryName);
                }

                // Check if destination file already exists
                if (File.Exists(episode.NewFileNamePath))
                {
                    // For automatic processing, skip existing files
                    FileProcessed?.Invoke(this, new FileProcessedEventArgs(filePath, false, "Destination file already exists"));
                    return;
                }

                // Copy file with retry logic
                bool success = await CopyFileWithRetry(filePath, episode.NewFileNamePath);
                
                if (success)
                {
                    // Verify copy
                    if (VerifyFileCopy(filePath, episode.NewFileNamePath))
                    {
                        // Delete original
                        File.Delete(filePath);
                        FileProcessed?.Invoke(this, new FileProcessedEventArgs(filePath, true, 
                            $"Successfully moved to: {episode.NewFileNamePath}"));
                        StatusChanged?.Invoke(this, $"Completed: {episode.ShowName} S{episode.SeasonNumber:D2}E{episode.EpisodeNumber:D2}");
                    }
                    else
                    {
                        // Copy verification failed - delete bad copy
                        if (File.Exists(episode.NewFileNamePath))
                            File.Delete(episode.NewFileNamePath);
                        FileProcessed?.Invoke(this, new FileProcessedEventArgs(filePath, false, "File copy verification failed"));
                    }
                }
                else
                {
                    FileProcessed?.Invoke(this, new FileProcessedEventArgs(filePath, false, "File copy failed after retries"));
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                FileProcessed?.Invoke(this, new FileProcessedEventArgs(filePath, false, $"Error: {ex.Message}"));
            }
        }

        private async Task<bool> CopyFileWithRetry(string source, string destination, int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (File.Exists(destination))
                        File.Delete(destination);
                    
                    File.Copy(source, destination);
                    return true;
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    Debug.WriteLine($"Copy attempt {attempt} failed, retrying in 5 seconds...");
                    await Task.Delay(5000); // Wait 5 seconds before retry
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Copy attempt {attempt} failed with error: {ex.Message}");
                    if (attempt >= maxRetries)
                        break;
                    await Task.Delay(5000);
                }
            }
            return false;
        }

        private bool VerifyFileCopy(string originalFile, string copiedFile)
        {
            try
            {
                if (!File.Exists(copiedFile))
                    return false;

                FileInfo originalInfo = new FileInfo(originalFile);
                FileInfo copiedInfo = new FileInfo(copiedFile);

                return originalInfo.Length == copiedInfo.Length;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            StopMonitoring();
            _stabilityTimer?.Dispose();
            _fileWatcher?.Dispose();
        }
    }

    // Event argument classes
    public class FileProcessedEventArgs : EventArgs
    {
        public string FilePath { get; }
        public bool Success { get; }
        public string Message { get; }
        public DateTime ProcessedAt { get; }

        public FileProcessedEventArgs(string filePath, bool success, string message)
        {
            FilePath = filePath;
            Success = success;
            Message = message;
            ProcessedAt = DateTime.Now;
        }
    }

    public class FileFoundEventArgs : EventArgs
    {
        public string FilePath { get; }
        public string EventType { get; }
        public DateTime FoundAt { get; }

        public FileFoundEventArgs(string filePath, string eventType)
        {
            FilePath = filePath;
            EventType = eventType;
            FoundAt = DateTime.Now;
        }
    }
}