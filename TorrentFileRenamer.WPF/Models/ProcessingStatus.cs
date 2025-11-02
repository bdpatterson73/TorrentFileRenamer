namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// Represents the processing status of a file
/// </summary>
public enum ProcessingStatus
{
    /// <summary>
    /// File has been scanned but not yet processed
    /// </summary>
    Pending,

    /// <summary>
    /// File is currently being processed
    /// </summary>
    Processing,

    /// <summary>
    /// File was successfully processed
    /// </summary>
    Completed,

    /// <summary>
    /// File processing failed
    /// </summary>
    Failed,

    /// <summary>
    /// File name could not be parsed
    /// </summary>
    Unparsed
}