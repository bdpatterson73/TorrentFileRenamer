using System.Diagnostics;
using System.Text.Json;

namespace TorrentFileRenamer.Core.Configuration
{
    public static class LoggingService
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TorrentFileRenamer", "logs");

        private static string GetLogFilePath(DateTime date)
        {
            return Path.Combine(LogDirectory, $"log_{date:yyyy-MM-dd}.txt");
        }

        private static string CurrentLogFilePath => GetLogFilePath(DateTime.Now);

        static LoggingService()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize logging: {ex.Message}");
            }
        }

        public static void LogInfo(string message, string? context = null)
        {
            WriteLog("INFO", message, context);
        }

        public static void LogWarning(string message, string? context = null)
        {
            WriteLog("WARN", message, context);
        }

        public static void LogError(string message, Exception? exception = null, string? context = null)
        {
            string fullMessage = exception != null ? $"{message}: {exception}" : message;
            WriteLog("ERROR", fullMessage, context);
        }

        public static void LogDebug(string message, string? context = null)
        {
#if DEBUG
            WriteLog("DEBUG", message, context);
#endif
        }

        private static void WriteLog(string level, string message, string? context)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {context ?? "General"}: {message}";

                Debug.WriteLine(logEntry);

                Task.Run(() =>
                {
                    try
                    {
                        File.AppendAllText(CurrentLogFilePath, logEntry + Environment.NewLine);
                    }
                    catch
                    {
                    }
                });
            }
            catch
            {
            }
        }

        /// <summary>
        /// Get recent log entries for troubleshooting, reading from multiple log files if needed
        /// </summary>
        public static List<string> GetRecentLogs(int maxLines = 100)
        {
            try
            {
                var allLogs = new List<string>();

                // Get all log files, sorted by date descending (newest first)
                var logFiles = GetLogFiles().OrderByDescending(f => f).ToList();

                if (!logFiles.Any())
                {
                    return new List<string> { "No log files found. Logs are saved to: " + LogDirectory };
                }

                // Read from newest to oldest until we have enough lines
                foreach (var logFile in logFiles)
                {
                    try
                    {
                        if (File.Exists(logFile))
                        {
                            var lines = File.ReadAllLines(logFile);
                            allLogs.AddRange(lines);

                            if (allLogs.Count >= maxLines)
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error reading log file {logFile}: {ex.Message}");
                        allLogs.Add($"ERROR: Could not read {Path.GetFileName(logFile)}: {ex.Message}");
                    }
                }

                // Return the most recent entries
                if (allLogs.Count == 0)
                {
                    return new List<string>
                    {
                        $"Log files exist but could not be read from: {LogDirectory}",
                        $"Log files found: {logFiles.Count}",
                        $"Current log file: {CurrentLogFilePath}",
                        $"First log file: {(logFiles.Any() ? Path.GetFileName(logFiles[0]) : "none")}"
                    };
                }

                // Return most recent entries (last N lines)
                return allLogs.Skip(Math.Max(0, allLogs.Count - maxLines)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetRecentLogs] Exception: {ex}");
                return new List<string>
                {
                    $"Unable to read log files: {ex.Message}",
                    $"Log directory: {LogDirectory}",
                    $"Current log file: {CurrentLogFilePath}",
                    $"Stack trace: {ex.StackTrace}"
                };
            }
        }

        /// <summary>
        /// Get all available log files
        /// </summary>
        public static List<string> GetLogFiles()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    return new List<string>();

                return Directory.GetFiles(LogDirectory, "log_*.txt")
                    .OrderByDescending(f => f)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetLogFiles] Exception: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Get log directory path for troubleshooting
        /// </summary>
        public static string GetLogDirectory()
        {
            return LogDirectory;
        }

        /// <summary>
        /// Get current log file path
        /// </summary>
        public static string GetCurrentLogFilePath()
        {
            return CurrentLogFilePath;
        }

        public static void CleanupOldLogs(int daysToKeep = 30)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var oldFiles = Directory.GetFiles(LogDirectory, "log_*.txt")
                    .Where(f => File.GetCreationTime(f) < cutoffDate);

                foreach (var file in oldFiles)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to cleanup old logs: {ex.Message}");
            }
        }
    }

    public class AppSettings
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TorrentFileRenamer", "settings.json");

        public string DefaultSourcePath { get; set; } = "";
        public string DefaultDestinationPath { get; set; } = "";
        public string DefaultFileExtensions { get; set; } = "*.mp4;*.mkv;*.avi;*.m4v";
        public bool RememberLastPaths { get; set; } = true;
        public bool EnableLogging { get; set; } = true;
        public int LogRetentionDays { get; set; } = 30;
        public bool SimulateMode { get; set; } = false;

        /// <summary>
        /// File verification method: "FileSize" (fast) or "Checksum" (thorough MD5)
        /// </summary>
        public string FileVerificationMethod { get; set; } = "FileSize";

        public MonitoringSettings Monitoring { get; set; } = new();
        public PlexSettings PlexSettings { get; set; } = new();

        // Last used paths for TV Episodes scan
        public string LastTvEpisodeSourcePath { get; set; } = "";
        public string LastTvEpisodeDestinationPath { get; set; } = "";
        public string LastTvEpisodeFileExtensions { get; set; } = ".mkv;.mp4;.avi";

        // Last used paths for Movies scan
        public string LastMovieSourcePath { get; set; } = "";
        public string LastMovieDestinationPath { get; set; } = "";
        public string LastMovieFileExtensions { get; set; } = ".mkv, .mp4, .avi, .m4v";
        public int LastMovieMinimumConfidence { get; set; } = 40;

        // Window state persistence
        public Dictionary<string, WindowState> WindowStates { get; set; } = new();

        // Column widths persistence
        public Dictionary<string, List<double>> ColumnWidths { get; set; } = new();

// Selected tabs persistence
        public Dictionary<string, int> SelectedTabs { get; set; } = new();

        // MRU (Most Recently Used) lists
        public Dictionary<string, List<string>> MruLists { get; set; } = new();

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                    return new AppSettings();

                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to load settings", ex, "AppSettings");
                return new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                var settingsDir = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(settingsDir))
                    Directory.CreateDirectory(settingsDir!);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsFilePath, json);

                LoggingService.LogInfo("Settings saved successfully", "AppSettings");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to save settings", ex, "AppSettings");
            }
        }
    }

    // Window state data
    public class WindowState
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int WindowStateValue { get; set; } = 0; // 0=Normal, 1=Minimized, 2=Maximized
    }

    public class MonitoringSettings
    {
        public string WatchFolder { get; set; } = "";
        public string DestinationFolder { get; set; } = "";
        public string FileExtensions { get; set; } = "*.mp4;*.mkv;*.avi;*.m4v";
        public int StabilityDelaySeconds { get; set; } = 30;
        public bool AutoStartOnLoad { get; set; } = false;
        public bool ProcessSubfolders { get; set; } = false;
        public int MaxAutoMonitorLogEntries { get; set; } = 20;
    }

    public class PlexSettings
    {
        public bool EnablePlexValidation { get; set; } = true;
        public bool AutoFixPlexIssues { get; set; } = true;
        public bool PromptForPlexIssues { get; set; } = true;
        public bool SkipPlexIncompatibleInAutoMode { get; set; } = true;
    }
}