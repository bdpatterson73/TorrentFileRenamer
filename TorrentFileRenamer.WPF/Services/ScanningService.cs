using System.IO;
using TorrentFileRenamer.Core.Models;
using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Implementation of IScanningService for scanning directories for media files
/// </summary>
public class ScanningService : IScanningService
{
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

        var results = new List<FileEpisodeModel>();

        // Find all matching files
        var allFiles = new List<string>();
        foreach (var extension in fileExtensions)
 {
   var files = Directory.GetFiles(sourcePath, $"*{extension}", SearchOption.AllDirectories);
        allFiles.AddRange(files);
    }

        if (allFiles.Count == 0)
            return results;

        // Process files
    var scanProgress = new ScanProgress
{
  TotalFiles = allFiles.Count,
     FilesProcessed = 0
  };

   for (int i = 0; i < allFiles.Count; i++)
    {
  cancellationToken.ThrowIfCancellationRequested();

  var file = allFiles[i];
   scanProgress.CurrentFile = Path.GetFileName(file);
    scanProgress.FilesProcessed = i;

     progress?.Report(scanProgress);

       // Use Task.Run to avoid blocking UI thread during parsing
    await Task.Run(() =>
     {
     try
      {
        var episode = new FileEpisode(file, destinationPath);
    var model = new FileEpisodeModel(episode);
   results.Add(model);
    }
  catch (Exception ex)
    {
      // Log error but continue processing
   System.Diagnostics.Debug.WriteLine($"Error scanning file {file}: {ex.Message}");
  }
       }, cancellationToken);
        }

// Report completion
    scanProgress.FilesProcessed = allFiles.Count;
        progress?.Report(scanProgress);

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

        var results = new List<MovieFileModel>();

        // Find all matching files
        var allFiles = new List<string>();
        foreach (var extension in fileExtensions)
        {
            var files = Directory.GetFiles(sourcePath, $"*{extension}", SearchOption.AllDirectories);
        allFiles.AddRange(files);
        }

        if (allFiles.Count == 0)
            return results;

        // Process files
      var scanProgress = new ScanProgress
        {
       TotalFiles = allFiles.Count,
            FilesProcessed = 0
    };

        for (int i = 0; i < allFiles.Count; i++)
        {
         cancellationToken.ThrowIfCancellationRequested();

            var file = allFiles[i];
          scanProgress.CurrentFile = Path.GetFileName(file);
   scanProgress.FilesProcessed = i;

 progress?.Report(scanProgress);

        // Use Task.Run to avoid blocking UI thread during parsing
       await Task.Run(() =>
            {
                try
  {
    var movie = new MovieFile(file, destinationPath);
var model = new MovieFileModel(movie, file);
              results.Add(model);
        }
   catch (Exception ex)
        {
    // Log error but continue processing
        System.Diagnostics.Debug.WriteLine($"Error scanning file {file}: {ex.Message}");
   }
            }, cancellationToken);
        }

    // Report completion
     scanProgress.FilesProcessed = allFiles.Count;
   progress?.Report(scanProgress);

        return results;
    }
}
