using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Implementation of IFileProcessingService for copying and verifying files
/// </summary>
public class FileProcessingService : IFileProcessingService
{
    private const int BufferSize = 81920; // 80 KB buffer
    private const int MaxRetries = 3;
    private const int InitialRetryDelayMs = 1000;

    /// <inheritdoc/>
  public async Task<bool> ProcessFileAsync(
        FileEpisodeModel episode,
   IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
  if (episode.Status == ProcessingStatus.Unparsed)
        {
            episode.ErrorMessage = "Cannot process unparsed file";
         return false;
        }

 var sourcePath = episode.SourcePath;
  var destinationPath = episode.DestinationPath;

        try
 {
   episode.Status = ProcessingStatus.Processing;
  episode.ErrorMessage = string.Empty;

  // Ensure destination directory exists
   var destDirectory = Path.GetDirectoryName(destinationPath);
   if (!string.IsNullOrEmpty(destDirectory))
       {
 Directory.CreateDirectory(destDirectory);
}

  // Copy file with retry logic
         bool copySuccess = await CopyFileWithRetryAsync(
     sourcePath,
         destinationPath,
    progress,
   cancellationToken);

  if (!copySuccess)
      {
      episode.Status = ProcessingStatus.Failed;
           episode.ErrorMessage = "File copy failed after retries";
                return false;
         }

  // Verify the copy
   bool verifySuccess = await VerifyFileCopyAsync(sourcePath, destinationPath, cancellationToken);
   if (!verifySuccess)
   {
   // Delete the failed copy
      if (File.Exists(destinationPath))
       {
  File.Delete(destinationPath);
     }

       episode.Status = ProcessingStatus.Failed;
    episode.ErrorMessage = "File verification failed";
 return false;
   }

          episode.Status = ProcessingStatus.Completed;
            return true;
        }
        catch (OperationCanceledException)
  {
  episode.Status = ProcessingStatus.Failed;
episode.ErrorMessage = "Operation cancelled";
  throw;
 }
   catch (Exception ex)
    {
     episode.Status = ProcessingStatus.Failed;
  episode.ErrorMessage = ex.Message;
            Debug.WriteLine($"Error processing file {sourcePath}: {ex.Message}");
     return false;
      }
    }

    /// <inheritdoc/>
    public async Task<int> ProcessFilesAsync(
        IEnumerable<FileEpisodeModel> episodes,
        IProgress<(string fileName, int percentage)>? fileProgress = null,
        IProgress<(int current, int total)>? overallProgress = null,
  CancellationToken cancellationToken = default)
    {
     var episodeList = episodes.ToList();
   int successCount = 0;
  int totalCount = episodeList.Count;

        for (int i = 0; i < episodeList.Count; i++)
     {
      var episode = episodeList[i];
      
    overallProgress?.Report((i + 1, totalCount));

  var fileProgressWrapper = new Progress<int>(percentage =>
   {
    fileProgress?.Report((episode.NewFileName, percentage));
      });

       bool success = await ProcessFileAsync(episode, fileProgressWrapper, cancellationToken);
      if (success)
       {
      successCount++;
     }
      }

      return successCount;
    }

    /// <summary>
 /// Copies a file with retry logic and exponential backoff
 /// </summary>
    private async Task<bool> CopyFileWithRetryAsync(
        string sourcePath,
  string destinationPath,
      IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        int retryCount = 0;
        int delayMs = InitialRetryDelayMs;

   while (retryCount < MaxRetries)
        {
   try
       {
            await CopyFileAsync(sourcePath, destinationPath, progress, cancellationToken);
    return true;
     }
     catch (OperationCanceledException)
       {
       throw;
       }
            catch (Exception ex)
 {
      retryCount++;
    Debug.WriteLine($"Copy attempt {retryCount} failed: {ex.Message}");

         if (retryCount >= MaxRetries)
 {
    return false;
      }

     // Exponential backoff
   await Task.Delay(delayMs, cancellationToken);
              delayMs *= 2;
   }
  }

        return false;
    }

    /// <summary>
    /// Copies a file with progress reporting
 /// </summary>
    private async Task CopyFileAsync(
string sourcePath,
  string destinationPath,
      IProgress<int>? progress,
CancellationToken cancellationToken)
    {
using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);
        using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, useAsync: true);

 var buffer = new byte[BufferSize];
 long totalBytes = sourceStream.Length;
     long totalBytesRead = 0;
        int bytesRead;
        int lastReportedProgress = 0;

      while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
 {
   await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
   totalBytesRead += bytesRead;

       // Report progress
            if (progress != null && totalBytes > 0)
     {
    int currentProgress = (int)((totalBytesRead * 100) / totalBytes);
      if (currentProgress != lastReportedProgress)
           {
    progress.Report(currentProgress);
        lastReportedProgress = currentProgress;
      }
       }
     }
    }

    /// <summary>
    /// Verifies that a file was copied correctly by comparing file sizes and checksums
    /// </summary>
    private async Task<bool> VerifyFileCopyAsync(
   string sourcePath,
        string destinationPath,
 CancellationToken cancellationToken)
  {
 try
   {
        // Quick check: Compare file sizes
            var sourceInfo = new FileInfo(sourcePath);
  var destInfo = new FileInfo(destinationPath);

   if (sourceInfo.Length != destInfo.Length)
            {
           Debug.WriteLine("File size mismatch");
          return false;
     }

          // Thorough check: Compare MD5 hashes
  var sourceHash = await ComputeMD5Async(sourcePath, cancellationToken);
  var destHash = await ComputeMD5Async(destinationPath, cancellationToken);

          bool hashesMatch = sourceHash.SequenceEqual(destHash);
            if (!hashesMatch)
      {
     Debug.WriteLine("File hash mismatch");
       }

            return hashesMatch;
        }
        catch (Exception ex)
    {
       Debug.WriteLine($"Error verifying file: {ex.Message}");
            return false;
      }
    }

    /// <summary>
    /// Computes MD5 hash of a file
    /// </summary>
    private async Task<byte[]> ComputeMD5Async(string filePath, CancellationToken cancellationToken)
    {
 using var md5 = MD5.Create();
 using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);
        
        var buffer = new byte[BufferSize];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
     {
     md5.TransformBlock(buffer, 0, bytesRead, null, 0);
  }

        md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
    return md5.Hash ?? Array.Empty<byte>();
    }

    /// <inheritdoc/>
    public async Task<bool> ProcessMovieAsync(
     MovieFileModel movie,
IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (movie.Status == ProcessingStatus.Unparsed)
     {
        movie.ErrorMessage = "Cannot process unparsed file";
            return false;
   }

        var sourcePath = movie.SourcePath;
        var destinationPath = movie.DestinationPath;

        try
        {
    movie.Status = ProcessingStatus.Processing;
            movie.ErrorMessage = string.Empty;

            // Ensure destination directory exists
   var destDirectory = Path.GetDirectoryName(destinationPath);
      if (!string.IsNullOrEmpty(destDirectory))
       {
                Directory.CreateDirectory(destDirectory);
 }

  // Copy file with retry logic
            bool copySuccess = await CopyFileWithRetryAsync(
          sourcePath,
     destinationPath,
       progress,
           cancellationToken);

    if (!copySuccess)
      {
             movie.Status = ProcessingStatus.Failed;
      movie.ErrorMessage = "File copy failed after retries";
     return false;
   }

      // Verify the copy
            bool verifySuccess = await VerifyFileCopyAsync(sourcePath, destinationPath, cancellationToken);
   if (!verifySuccess)
       {
   // Delete the failed copy
 if (File.Exists(destinationPath))
   {
           File.Delete(destinationPath);
  }

            movie.Status = ProcessingStatus.Failed;
         movie.ErrorMessage = "File verification failed";
   return false;
     }

  movie.Status = ProcessingStatus.Completed;
            return true;
        }
        catch (OperationCanceledException)
   {
        movie.Status = ProcessingStatus.Failed;
    movie.ErrorMessage = "Operation cancelled";
            throw;
    }
        catch (Exception ex)
 {
movie.Status = ProcessingStatus.Failed;
  movie.ErrorMessage = ex.Message;
            Debug.WriteLine($"Error processing file {sourcePath}: {ex.Message}");
        return false;
        }
    }

    /// <inheritdoc/>
  public async Task<int> ProcessMoviesAsync(
     IEnumerable<MovieFileModel> movies,
        IProgress<(string fileName, int percentage)>? fileProgress = null,
        IProgress<(int current, int total)>? overallProgress = null,
        CancellationToken cancellationToken = default)
    {
  var movieList = movies.ToList();
        int successCount = 0;
        int totalCount = movieList.Count;

        for (int i = 0; i < movieList.Count; i++)
        {
            var movie = movieList[i];
       
        overallProgress?.Report((i + 1, totalCount));

         var fileProgressWrapper = new Progress<int>(percentage =>
  {
       fileProgress?.Report((movie.FileName, percentage));
            });

   bool success = await ProcessMovieAsync(movie, fileProgressWrapper, cancellationToken);
            if (success)
     {
  successCount++;
  }
        }

        return successCount;
    }
}
