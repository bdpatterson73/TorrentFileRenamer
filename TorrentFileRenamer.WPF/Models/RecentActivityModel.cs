using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// Represents a recent activity log entry from the auto-monitor
/// </summary>
public class RecentActivityModel : ObservableObject
{
    private DateTime _timestamp;
    private string _fileName = string.Empty;
    private string _activityType = string.Empty;
    private string _status = string.Empty;
    private string _message = string.Empty;
    private bool _isSuccess;

  public DateTime Timestamp
    {
        get => _timestamp;
        set => SetProperty(ref _timestamp, value);
    }

    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    public string ActivityType
    {
        get => _activityType;
   set => SetProperty(ref _activityType, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Message
    {
 get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool IsSuccess
    {
        get => _isSuccess;
        set => SetProperty(ref _isSuccess, value);
    }

    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
}
