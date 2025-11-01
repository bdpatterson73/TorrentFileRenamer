namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// Represents the current status of the folder monitoring service
/// </summary>
public enum MonitoringStatus
{
    /// <summary>
    /// Monitoring is not running
    /// </summary>
    Stopped,
    
    /// <summary>
    /// Monitoring is starting up
    /// </summary>
    Starting,
    
    /// <summary>
    /// Monitoring is actively running
    /// </summary>
    Running,
    
    /// <summary>
    /// Monitoring is stopping
    /// </summary>
    Stopping,
    
/// <summary>
    /// An error occurred during monitoring
    /// </summary>
    Error
}
