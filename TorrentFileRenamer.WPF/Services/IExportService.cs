using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Service for exporting media file data to various formats
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports movie files to the specified format
    /// </summary>
    Task<bool> ExportMoviesAsync(
  IEnumerable<MovieFileModel> movies,
        ExportOptions options,
        IProgress<int>? progress = null);

    /// <summary>
    /// Exports TV episode files to the specified format
    /// </summary>
    Task<bool> ExportEpisodesAsync(
  IEnumerable<FileEpisodeModel> episodes,
        ExportOptions options,
        IProgress<int>? progress = null);

    /// <summary>
    /// Generates a summary report for movie files
    /// </summary>
    Task<string> GenerateMovieSummaryAsync(
        IEnumerable<MovieFileModel> movies,
        FileStatistics statistics);

    /// <summary>
    /// Generates a summary report for TV episode files
    /// </summary>
    Task<string> GenerateEpisodeSummaryAsync(
        IEnumerable<FileEpisodeModel> episodes,
      FileStatistics statistics);

    /// <summary>
    /// Gets the default file extension for the export format
/// </summary>
    string GetFileExtension(ExportFormat format);

    /// <summary>
    /// Gets the filter string for save file dialog
    /// </summary>
    string GetFileFilter(ExportFormat format);
}
