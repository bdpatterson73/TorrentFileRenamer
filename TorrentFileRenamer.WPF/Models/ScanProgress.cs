namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// Represents progress information during scanning operations
/// </summary>
public class ScanProgress
{
    /// <summary>
    /// Current file being scanned
    /// </summary>
    public string CurrentFile { get; set; } = string.Empty;

    /// <summary>
    /// Number of files processed so far
    /// </summary>
    public int FilesProcessed { get; set; }

    /// <summary>
    /// Total number of files to process
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Percentage complete (0-100)
    /// </summary>
    public int PercentComplete => TotalFiles > 0 ? (FilesProcessed * 100) / TotalFiles : 0;

    /// <summary>
    /// Whether the operation is complete
    /// </summary>
    public bool IsComplete => FilesProcessed >= TotalFiles && TotalFiles > 0;
}