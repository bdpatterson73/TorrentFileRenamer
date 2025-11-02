using System.Diagnostics;
using System.Text.Json;

namespace TorrentFileRenamer.Core.Configuration
{
    public static class LoggingService
    {
        private static readonly string LogFilePath = Path.Combine(
       Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
          "TorrentFileRenamer", "logs", $"log_{DateTime.Now:yyyy-MM-dd}.txt");

        static LoggingService()
 {
            try
          {
           var logDir = Path.GetDirectoryName(LogFilePath);
       if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir!);
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
     File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
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

  public static List<string> GetRecentLogs(int maxLines = 100)
        {
try
     {
   if (!File.Exists(LogFilePath))
         return new List<string>();

       var lines = File.ReadAllLines(LogFilePath);
return lines.Skip(Math.Max(0, lines.Length - maxLines)).ToList();
            }
   catch
  {
      return new List<string> { "Unable to read log file" };
  }
        }

   public static void CleanupOldLogs(int daysToKeep = 30)
{
        try
  {
   var logDir = Path.GetDirectoryName(LogFilePath);
 if (!Directory.Exists(logDir))
         return;

         var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
    var oldFiles = Directory.GetFiles(logDir, "log_*.txt")
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
        public MonitoringSettings Monitoring { get; set; } = new();
   public PlexSettings PlexSettings { get; set; } = new();

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
