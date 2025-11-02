using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// Model representing a log entry
/// </summary>
public class LogEntryModel : ObservableObject
{
    private DateTime _timestamp;
    private string _level = "";
    private string _context = "";
    private string _message = "";

    public DateTime Timestamp
    {
        get => _timestamp;
        set => SetProperty(ref _timestamp, value);
    }

    public string Level
    {
        get => _level;
        set => SetProperty(ref _level, value);
    }

    public string Context
    {
        get => _context;
        set => SetProperty(ref _context, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
}