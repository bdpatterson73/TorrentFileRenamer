using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Service for processing and copying media files
/// </summary>
public interface IFileProcessingService
{
    /// <summary>
    /// Processes a single file by copying it to the destination with verification
    /// </summary>
    /// <param name="episode">The episode to process</param>
    /// <param name="progress">Progress reporting callback (0-100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ProcessFileAsync(
        FileEpisodeModel episode,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes multiple files
    /// </summary>
    /// <param name="episodes">Episodes to process</param>
    /// <param name="fileProgress">Progress reporting for individual file (filename, percentage)</param>
    /// <param name="overallProgress">Progress reporting for overall operation (current file index, total files)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of successfully processed files</returns>
    Task<int> ProcessFilesAsync(
        IEnumerable<FileEpisodeModel> episodes,
        IProgress<(string fileName, int percentage)>? fileProgress = null,
        IProgress<(int current, int total)>? overallProgress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a single movie file by copying it to the destination with verification
    /// </summary>
    /// <param name="movie">The movie to process</param>
    /// <param name="progress">Progress reporting callback (0-100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ProcessMovieAsync(
        MovieFileModel movie,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes multiple movie files
    /// </summary>
    /// <param name="movies">Movies to process</param>
    /// <param name="fileProgress">Progress reporting for individual file (filename, percentage)</param>
    /// <param name="overallProgress">Progress reporting for overall operation (current file index, total files)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of successfully processed files</returns>
    Task<int> ProcessMoviesAsync(
        IEnumerable<MovieFileModel> movies,
        IProgress<(string fileName, int percentage)>? fileProgress = null,
        IProgress<(int current, int total)>? overallProgress = null,
        CancellationToken cancellationToken = default);
}