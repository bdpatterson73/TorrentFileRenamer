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

    public LogViewerViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Initialize commands
        RefreshCommand = new RelayCommand(ExecuteRefresh);
        ClearLogsCommand = new RelayCommand(ExecuteClearLogs);
        ExportLogsCommand = new RelayCommand(ExecuteExportLogs);
        CloseCommand = new RelayCommand(ExecuteClose);

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

    public List<string> LogLevels { get; } = new() { "All", "INFO", "WARN", "ERROR", "DEBUG" };

    public bool? DialogResult { get; private set; }

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand ClearLogsCommand { get; }
    public ICommand ExportLogsCommand { get; }
    public ICommand CloseCommand { get; }

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

    #endregion

    #region Private Methods

    private void LoadLogs()
    {
        try
        {
            var logs = LoggingService.GetRecentLogs(500);
            var logEntries = new List<LogEntryModel>();

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

            LogEntries = new ObservableCollection<LogEntryModel>(logEntries);
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessage("Load Error", $"Failed to load logs: {ex.Message}");
        }
    }

    private LogEntryModel? ParseLogEntry(string logLine)
    {
        try
        {
            // Expected format: [yyyy-MM-dd HH:mm:ss] [LEVEL] Context: Message
            if (string.IsNullOrWhiteSpace(logLine))
                return null;

            var parts = logLine.Split(new[] { "] [", "]: " }, StringSplitOptions.None);
            if (parts.Length < 3)
                return null;

            var timestampStr = parts[0].TrimStart('[');
            var level = parts[1].Trim();
            var context = parts[2].Trim();
            var message = parts.Length > 3 ? string.Join(": ", parts.Skip(3)) : "";

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