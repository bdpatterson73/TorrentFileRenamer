using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.WPF.Models;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for the log viewer dialog
/// </summary>
public class LogViewerViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private ObservableCollection<LogEntryModel> _logEntries = new();
    private LogEntryModel? _selectedLogEntry;
    private string _filterLevel = "All";
    private string _searchText = "";
    private string _logFilePath = "";
    private int _totalLogCount;
    private bool? _dialogResult;

    public LogViewerViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Initialize commands
        RefreshCommand = new RelayCommand(ExecuteRefresh);
        ClearLogsCommand = new RelayCommand(ExecuteClearLogs);
        ExportLogsCommand = new RelayCommand(ExecuteExportLogs);
        CloseCommand = new RelayCommand(ExecuteClose);
        OpenLogFolderCommand = new RelayCommand(ExecuteOpenLogFolder);

        // Set log file path
        _logFilePath = LoggingService.GetLogDirectory();

        // Load logs on startup
        LoadLogs();
    }

    #region Properties

    public ObservableCollection<LogEntryModel> LogEntries
    {
        get => _logEntries;
        set => SetProperty(ref _logEntries, value);
    }

    public LogEntryModel? SelectedLogEntry
    {
        get => _selectedLogEntry;
        set => SetProperty(ref _selectedLogEntry, value);
    }

    public string FilterLevel
    {
        get => _filterLevel;
        set
        {
            if (SetProperty(ref _filterLevel, value))
            {
                LoadLogs();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                LoadLogs();
            }
        }
    }

    public string LogFilePath
    {
        get => _logFilePath;
        set => SetProperty(ref _logFilePath, value);
    }

    public int TotalLogCount
    {
        get => _totalLogCount;
        set
        {
            if (SetProperty(ref _totalLogCount, value))
            {
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    public string StatusMessage => LogEntries.Count > 0
        ? $"Showing {LogEntries.Count} of {TotalLogCount} log entries | Logs: {LogFilePath}"
        : $"No logs found | Log location: {LogFilePath}";

    public List<string> LogLevels { get; } = new() { "All", "INFO", "WARN", "ERROR", "DEBUG" };

    public bool? DialogResult
    {
        get => _dialogResult;
        private set => SetProperty(ref _dialogResult, value);
    }

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand ClearLogsCommand { get; }
    public ICommand ExportLogsCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand OpenLogFolderCommand { get; }

    #endregion

    #region Command Implementations

    private void ExecuteRefresh()
    {
        LoadLogs();
    }

    private void ExecuteClearLogs()
    {
        var result = _dialogService.ShowConfirmation(
            "Clear Logs",
            "Are you sure you want to clear all log entries? This action cannot be undone.");

        if (result)
        {
            LogEntries.Clear();
            _dialogService.ShowMessage("Logs Cleared", "All log entries have been cleared from the display.");
        }
    }

    private void ExecuteExportLogs()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var defaultFileName = $"TorrentFileRenamer_Logs_{timestamp}.txt";
            var fileName = _dialogService.ShowSaveFileDialog(defaultFileName, "Text Files (*.txt)|*.txt|All Files (*.*)|*.*");

            if (!string.IsNullOrEmpty(fileName))
            {
                var lines = LogEntries.Select(l => $"[{l.Timestamp:yyyy-MM-dd HH:mm:ss}] [{l.Level}] {l.Context}: {l.Message}");
                File.WriteAllLines(fileName, lines);
                _dialogService.ShowMessage("Export Complete", $"Logs exported to:\n{fileName}");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessage("Export Failed", $"Failed to export logs: {ex.Message}");
        }
    }

    private void ExecuteClose()
    {
        DialogResult = false;
    }

    private void ExecuteOpenLogFolder()
    {
        try
        {
            var logDir = LoggingService.GetLogDirectory();
            if (Directory.Exists(logDir))
            {
                System.Diagnostics.Process.Start("explorer.exe", logDir);
            }
            else
            {
                _dialogService.ShowMessage("Log Folder Not Found",
                    $"The log directory does not exist yet:\n{logDir}\n\nLogs will be created when the application performs actions.");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessage("Error Opening Folder", $"Failed to open log folder: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods

    private void LoadLogs()
    {
        try
        {
            var logs = LoggingService.GetRecentLogs(500);
            var logEntries = new List<LogEntryModel>();

            // Check if logs contain diagnostic messages
            if (logs.Any() && (logs[0].Contains("No log files found") || logs[0].Contains("Unable to read") || logs[0].Contains("Log files exist but")))
            {
                // Show diagnostic information as log entries
                foreach (var diagnosticMsg in logs)
                {
                    logEntries.Add(new LogEntryModel
                    {
                        Timestamp = DateTime.Now,
                        Level = "INFO",
                        Context = "LogViewer",
                        Message = diagnosticMsg
                    });
                }
            }
            else
            {
                foreach (var log in logs)
                {
                    var entry = ParseLogEntry(log);
                    if (entry != null)
                    {
                        // Apply filters
                        if (FilterLevel != "All" && entry.Level != FilterLevel)
                            continue;

                        if (!string.IsNullOrWhiteSpace(SearchText) &&
                            !entry.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                            !entry.Context.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                            continue;

                        logEntries.Add(entry);
                    }
                }
            }

            TotalLogCount = logs.Count;
            LogEntries = new ObservableCollection<LogEntryModel>(logEntries);
            OnPropertyChanged(nameof(StatusMessage));
        }
        catch (Exception ex)
        {
            TotalLogCount = 0;
            LogEntries = new ObservableCollection<LogEntryModel>(new[]
            {
                new LogEntryModel
                {
                    Timestamp = DateTime.Now,
                    Level = "ERROR",
                    Context = "LogViewer",
                    Message = $"Failed to load logs: {ex.Message}"
                }
            });
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    private LogEntryModel? ParseLogEntry(string logLine)
    {
        try
        {
            // Expected format: [yyyy-MM-dd HH:mm:ss] [LEVEL] Context: Message
            if (string.IsNullOrWhiteSpace(logLine))
                return null;

            // Split on "] [" first to separate timestamp and level
            var firstSplit = logLine.Split(new[] { "] [" }, StringSplitOptions.None);

            if (firstSplit.Length < 2)
                return null;

            // Extract timestamp (remove leading '[')
            var timestampStr = firstSplit[0].TrimStart('[');

            // The rest is: "LEVEL] Context: Message"
            // Split on "]" to separate level from context+message
            var remainder = firstSplit[1];
            var levelEndIndex = remainder.IndexOf(']');

            if (levelEndIndex < 0)
                return null;

            var level = remainder.Substring(0, levelEndIndex).Trim();
            var contextAndMessage = remainder.Substring(levelEndIndex + 1).Trim();

// Split context and message on ": "
            var colonIndex = contextAndMessage.IndexOf(": ");
            string context;
            string message;

            if (colonIndex >= 0)
            {
                context = contextAndMessage.Substring(0, colonIndex).Trim();
                message = contextAndMessage.Substring(colonIndex + 2).Trim();
            }
            else
            {
                // No colon separator, treat everything as context
                context = contextAndMessage;
                message = "";
            }

            if (!DateTime.TryParse(timestampStr, out var timestamp))
                timestamp = DateTime.Now;

            return new LogEntryModel
            {
                Timestamp = timestamp,
                Level = level,
                Context = context,
                Message = message
            };
        }
        catch
        {
            return null;
        }
    }

    #endregion
}