namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// Export format options
/// </summary>
public enum ExportFormat
{
 Csv,
  Json,
    Excel
}

/// <summary>
/// Options for exporting file data
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Export format
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.Csv;

    /// <summary>
    /// Output file path
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Include file names
    /// </summary>
    public bool IncludeFileName { get; set; } = true;

    /// <summary>
    /// Include new file names
    /// </summary>
    public bool IncludeNewFileName { get; set; } = true;

    /// <summary>
    /// Include movie/show names
    /// </summary>
    public bool IncludeMediaName { get; set; } = true;

    /// <summary>
 /// Include year information
    /// </summary>
 public bool IncludeYear { get; set; } = true;

    /// <summary>
    /// Include season/episode numbers (TV only)
    /// </summary>
    public bool IncludeSeasonEpisode { get; set; } = true;

    /// <summary>
    /// Include confidence scores
    /// </summary>
    public bool IncludeConfidence { get; set; } = true;

    /// <summary>
    /// Include status information
 /// </summary>
    public bool IncludeStatus { get; set; } = true;

    /// <summary>
    /// Include file sizes
    /// </summary>
    public bool IncludeFileSize { get; set; } = true;

    /// <summary>
    /// Include file extensions
    /// </summary>
    public bool IncludeExtension { get; set; } = true;

    /// <summary>
    /// Include full file paths
/// </summary>
    public bool IncludeFullPaths { get; set; } = false;

  /// <summary>
    /// Include error messages
  /// </summary>
    public bool IncludeErrors { get; set; } = true;

    /// <summary>
    /// Include timestamp
    /// </summary>
    public bool IncludeTimestamp { get; set; } = true;

    /// <summary>
    /// Export only selected items
    /// </summary>
    public bool ExportSelectedOnly { get; set; } = false;

    /// <summary>
    /// Creates default export options
  /// </summary>
    public static ExportOptions Default => new ExportOptions();

    /// <summary>
    /// Creates minimal export options (name and status only)
    /// </summary>
    public static ExportOptions Minimal => new ExportOptions
    {
     IncludeFileName = true,
        IncludeNewFileName = true,
        IncludeMediaName = true,
        IncludeYear = false,
    IncludeSeasonEpisode = false,
        IncludeConfidence = false,
        IncludeStatus = true,
    IncludeFileSize = false,
        IncludeExtension = false,
        IncludeFullPaths = false,
        IncludeErrors = false,
        IncludeTimestamp = false
    };

    /// <summary>
    /// Creates detailed export options (all fields)
  /// </summary>
    public static ExportOptions Detailed => new ExportOptions
  {
        IncludeFileName = true,
        IncludeNewFileName = true,
        IncludeMediaName = true,
        IncludeYear = true,
    IncludeSeasonEpisode = true,
        IncludeConfidence = true,
     IncludeStatus = true,
 IncludeFileSize = true,
        IncludeExtension = true,
        IncludeFullPaths = true,
    IncludeErrors = true,
        IncludeTimestamp = true
    };
}
