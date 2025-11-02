using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Service for scanning directories for media files
/// </summary>
public interface IScanningService
{
    /// <summary>
    /// Scans a directory for TV episode files
    /// </summary>
    /// <param name="sourcePath">Source directory to scan</param>
    /// <param name="destinationPath">Destination directory for processed files</param>
    /// <param name="fileExtensions">File extensions to include (e.g., ".mkv", ".mp4")</param>
    /// <param name="progress">Progress reporting callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of scanned file episode models</returns>
    Task<List<FileEpisodeModel>> ScanForTvEpisodesAsync(
        string sourcePath,
        string destinationPath,
        string[] fileExtensions,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans a directory for movie files
    /// </summary>
    /// <param name="sourcePath">Source directory to scan</param>
    /// <param name="destinationPath">Destination directory for processed files</param>
    /// <param name="fileExtensions">File extensions to include</param>
    /// <param name="progress">Progress reporting callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of scanned movie file models</returns>
    Task<List<MovieFileModel>> ScanForMoviesAsync(
        string sourcePath,
        string destinationPath,
        string[] fileExtensions,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);
}