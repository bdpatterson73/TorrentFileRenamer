using System.IO;
using TorrentFileRenamer.Core.Models;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Implementation of IScanningService for scanning directories for media files
/// </summary>
public class ScanningService : IScanningService
{
    private const int BatchSize = 50; // Process files in batches for better performance

    /// <inheritdoc/>
    public async Task<List<FileEpisodeModel>> ScanForTvEpisodesAsync(
        string sourcePath,
        string destinationPath,
        string[] fileExtensions,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Source path cannot be empty", nameof(sourcePath));

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Destination path cannot be empty", nameof(destinationPath));

        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

        LoggingService.LogInfo($"Starting TV episode scan: Source={sourcePath}, Dest={destinationPath}, Extensions=[{string.Join(", ", fileExtensions)}]", "Scanning");

        var results = new List<FileEpisodeModel>();

        // Find all matching files - do this on a background thread
        var allFiles = await Task.Run(() =>
        {
            var files = new List<string>();
            foreach (var extension in fileExtensions)
            {
                try
                {
                    var matchingFiles = Directory.GetFiles(sourcePath, $"*{extension}", SearchOption.AllDirectories);
                    files.AddRange(matchingFiles);
                    LoggingService.LogDebug($"Found {matchingFiles.Length} files with extension {extension}", "Scanning");
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Skip directories we don't have access to
                    LoggingService.LogWarning($"Access denied to some directories with extension {extension}: {ex.Message}", "Scanning");
                }
            }

            return files;
        }, cancellationToken);

        LoggingService.LogInfo($"Found {allFiles.Count} total files to scan for TV episodes", "Scanning");

        if (allFiles.Count == 0)
            return results;

        var scanProgress = new ScanProgress
        {
            TotalFiles = allFiles.Count,
            FilesProcessed = 0
        };

        int parsedCount = 0;
        int unparsedCount = 0;

        // Process files in batches to improve performance and UI responsiveness
        for (int i = 0; i < allFiles.Count; i += BatchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batch = allFiles.Skip(i).Take(BatchSize).ToList();

            // Process the batch on a background thread
            var batchResults = await Task.Run(() =>
            {
                var batchModels = new List<FileEpisodeModel>();
                foreach (var file in batch)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var episode = new FileEpisode(file, destinationPath);
                        var model = new FileEpisodeModel(episode);
                        batchModels.Add(model);

                        if (model.IsParsed)
                            parsedCount++;
                        else
                            unparsedCount++;
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing
                        LoggingService.LogError($"Error scanning file {Path.GetFileName(file)}", ex, "Scanning");
                    }
                }

                return batchModels;
            }, cancellationToken);

            results.AddRange(batchResults);

            // Update progress after each batch
            scanProgress.FilesProcessed = Math.Min(i + BatchSize, allFiles.Count);
            scanProgress.CurrentFile = batch.Count > 0 ? Path.GetFileName(batch[0]) : "";
            progress?.Report(scanProgress);

            // Small delay to allow UI to update
            await Task.Delay(10, cancellationToken);
        }

        // Report completion
        scanProgress.FilesProcessed = allFiles.Count;
        scanProgress.CurrentFile = "Scan complete";
        progress?.Report(scanProgress);

        LoggingService.LogInfo($"TV episode scan complete: {parsedCount} parsed, {unparsedCount} unparsed out of {allFiles.Count} total files", "Scanning");

        return results;
    }

    /// <inheritdoc/>
    public async Task<List<MovieFileModel>> ScanForMoviesAsync(
        string sourcePath,
        string destinationPath,
        string[] fileExtensions,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Source path cannot be empty", nameof(sourcePath));

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Destination path cannot be empty", nameof(destinationPath));

        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

        LoggingService.LogInfo($"Starting movie scan: Source={sourcePath}, Dest={destinationPath}, Extensions=[{string.Join(", ", fileExtensions)}]", "Scanning");

        var results = new List<MovieFileModel>();

        // Find all matching files - do this on a background thread
        var allFiles = await Task.Run(() =>
        {
            var files = new List<string>();
            foreach (var extension in fileExtensions)
            {
                try
                {
                    var matchingFiles = Directory.GetFiles(sourcePath, $"*{extension}", SearchOption.AllDirectories);
                    files.AddRange(matchingFiles);
                    LoggingService.LogDebug($"Found {matchingFiles.Length} files with extension {extension}", "Scanning");
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Skip directories we don't have access to
                    LoggingService.LogWarning($"Access denied to some directories with extension {extension}: {ex.Message}", "Scanning");
                }
            }

            return files;
        }, cancellationToken);

        LoggingService.LogInfo($"Found {allFiles.Count} total files to scan for movies", "Scanning");

        if (allFiles.Count == 0)
            return results;

        var scanProgress = new ScanProgress
        {
            TotalFiles = allFiles.Count,
            FilesProcessed = 0
        };

        int parsedCount = 0;
        int unparsedCount = 0;

        // Process files in batches to improve performance and UI responsiveness
        for (int i = 0; i < allFiles.Count; i += BatchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batch = allFiles.Skip(i).Take(BatchSize).ToList();

            // Process the batch on a background thread
            var batchResults = await Task.Run(() =>
            {
                var batchModels = new List<MovieFileModel>();
                foreach (var file in batch)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var movie = new MovieFile(file, destinationPath);
                        var model = new MovieFileModel(movie, file);
                        batchModels.Add(model);

                        if (model.IsParsed)
                            parsedCount++;
                        else
                            unparsedCount++;
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing
                        LoggingService.LogError($"Error scanning movie file {Path.GetFileName(file)}", ex, "Scanning");
                    }
                }

                return batchModels;
            }, cancellationToken);

            results.AddRange(batchResults);

            // Update progress after each batch
            scanProgress.FilesProcessed = Math.Min(i + BatchSize, allFiles.Count);
            scanProgress.CurrentFile = batch.Count > 0 ? Path.GetFileName(batch[0]) : "";
            progress?.Report(scanProgress);

            // Small delay to allow UI to update
            await Task.Delay(10, cancellationToken);
        }

        // Report completion
        scanProgress.FilesProcessed = allFiles.Count;
        scanProgress.CurrentFile = "Scan complete";
        progress?.Report(scanProgress);

        LoggingService.LogInfo($"Movie scan complete: {parsedCount} parsed, {unparsedCount} unparsed out of {allFiles.Count} total files", "Scanning");

        return results;
    }
}