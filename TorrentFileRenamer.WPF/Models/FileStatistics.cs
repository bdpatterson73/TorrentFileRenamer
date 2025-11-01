using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// Statistics model for dashboard display
/// </summary>
public class FileStatistics : ObservableObject
{
    private int _totalFiles;
    private int _processedFiles;
    private int _pendingFiles;
    private int _errorFiles;
    private int _unprocessedFiles;
    private double _averageConfidence;
    private long _totalFileSize;
    private DateTime _lastUpdated = DateTime.Now;

    /// <summary>
  /// Total number of files
    /// </summary>
    public int TotalFiles
    {
        get => _totalFiles;
        set
  {
     if (SetProperty(ref _totalFiles, value))
         {
       OnPropertyChanged(nameof(ProgressPercentage));
          }
        }
    }

    /// <summary>
  /// Number of successfully processed files
  /// </summary>
    public int ProcessedFiles
    {
        get => _processedFiles;
   set
        {
            if (SetProperty(ref _processedFiles, value))
   {
     OnPropertyChanged(nameof(ProgressPercentage));
    }
  }
    }

    /// <summary>
    /// Number of files pending processing
    /// </summary>
    public int PendingFiles
    {
        get => _pendingFiles;
        set => SetProperty(ref _pendingFiles, value);
    }

    /// <summary>
  /// Number of files with errors
    /// </summary>
    public int ErrorFiles
    {
     get => _errorFiles;
     set => SetProperty(ref _errorFiles, value);
    }

    /// <summary>
    /// Number of unparsed files
    /// </summary>
    public int UnprocessedFiles
    {
        get => _unprocessedFiles;
        set => SetProperty(ref _unprocessedFiles, value);
    }

    /// <summary>
    /// Average confidence score across all files
    /// </summary>
    public double AverageConfidence
    {
        get => _averageConfidence;
  set => SetProperty(ref _averageConfidence, value);
    }

/// <summary>
    /// Total file size in bytes
  /// </summary>
    public long TotalFileSize
    {
      get => _totalFileSize;
        set => SetProperty(ref _totalFileSize, value);
    }

    /// <summary>
    /// When these statistics were last updated
    /// </summary>
    public DateTime LastUpdated
    {
get => _lastUpdated;
     set => SetProperty(ref _lastUpdated, value);
    }

    /// <summary>
    /// Processing progress percentage
    /// </summary>
public double ProgressPercentage
    {
        get
        {
      if (TotalFiles == 0) return 0;
            return (double)ProcessedFiles / TotalFiles * 100;
}
    }

    /// <summary>
/// Formatted progress text
/// </summary>
    public string ProgressText => $"{ProcessedFiles}/{TotalFiles} files processed ({ProgressPercentage:F1}%)";

    /// <summary>
    /// Formatted file size text
  /// </summary>
    public string FileSizeText
 {
        get
        {
       const long KB = 1024;
         const long MB = KB * 1024;
      const long GB = MB * 1024;
            const long TB = GB * 1024;

  if (TotalFileSize >= TB)
          return $"{TotalFileSize / (double)TB:F2} TB";
        if (TotalFileSize >= GB)
      return $"{TotalFileSize / (double)GB:F2} GB";
       if (TotalFileSize >= MB)
       return $"{TotalFileSize / (double)MB:F2} MB";
     if (TotalFileSize >= KB)
        return $"{TotalFileSize / (double)KB:F2} KB";
        return $"{TotalFileSize} bytes";
        }
 }

    /// <summary>
    /// Resets all statistics
    /// </summary>
    public void Reset()
    {
        TotalFiles = 0;
        ProcessedFiles = 0;
     PendingFiles = 0;
        ErrorFiles = 0;
        UnprocessedFiles = 0;
  AverageConfidence = 0;
     TotalFileSize = 0;
        LastUpdated = DateTime.Now;
    }
}
