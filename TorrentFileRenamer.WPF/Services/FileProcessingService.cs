using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.Core.Utilities;
using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Implementation of IFileProcessingService for copying and verifying files
/// </summary>
public class FileProcessingService : IFileProcessingService
{
    private const int BufferSize = 1024 * 1024; // 1MB buffer for better performance
    private const int MaxRetries = 3;

    private readonly AppSettings _appSettings;

    public FileProcessingService(AppSettings appSettings)
 {
     _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

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

            // Simulate mode - skip actual file operations
      if (_appSettings.SimulateMode)
      {
 // Simulate processing delay
        await Task.Delay(500, cancellationToken);
    
    // Report simulated progress
       for (int i = 0; i <= 100; i += 20)
          {
          progress?.Report(i);
    await Task.Delay(100, cancellationToken);
   }
       
    episode.Status = ProcessingStatus.Completed;
  episode.ErrorMessage = "[SIMULATED] File would be processed successfully";
         return true;
      }

  // Ensure destination directory exists
   var destDirectory = Path.GetDirectoryName(destinationPath);
   if (!string.IsNullOrEmpty(destDirectory))
   {
 Directory.CreateDirectory(destDirectory);
}

  // Copy file with progress using FileOperationProgress
            var fileOperation = new FileOperationProgress();
       
      // Wire up progress reporting
            if (progress != null)
   {
          fileOperation.ProgressChanged += (sender, args) =>
      {
  progress.Report((int)args.PercentComplete);
 };
         }

         bool copySuccess = await fileOperation.CopyFileWithRetryAsync(
    sourcePath,
           destinationPath,
    MaxRetries,
          BufferSize,
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

  // Delete the original source file after successful copy and verification
    try
          {
      if (File.Exists(sourcePath))
              {
  File.Delete(sourcePath);
      Debug.WriteLine($"Deleted original file: {sourcePath}");
 }
          }
       catch (Exception ex)
          {
          // Log warning but don't fail the operation since copy was successful
     Debug.WriteLine($"Warning: Could not delete original file {sourcePath}: {ex.Message}");
   episode.ErrorMessage = $"Copy successful but could not delete original: {ex.Message}";
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
    /// Verifies that a file was copied correctly by comparing file sizes and optionally checksums
  /// </summary>
 private async Task<bool> VerifyFileCopyAsync(
   string sourcePath,
        string destinationPath,
 CancellationToken cancellationToken)
  {
 try
   {
        // Always check file sizes first (quick check)
var sourceInfo = new FileInfo(sourcePath);
  var destInfo = new FileInfo(destinationPath);

 if (sourceInfo.Length != destInfo.Length)
      {
           Debug.WriteLine("File size mismatch");
      return false;
 }

        // Check verification method setting
        if (_appSettings.FileVerificationMethod == "Checksum")
  {
          // Thorough check: Compare MD5 hashes
  Debug.WriteLine("Performing checksum verification (MD5)...");
            var sourceHash = await ComputeMD5Async(sourcePath, cancellationToken);
      var destHash = await ComputeMD5Async(destinationPath, cancellationToken);

  bool hashesMatch = sourceHash.SequenceEqual(destHash);
            if (!hashesMatch)
     {
          Debug.WriteLine("File hash mismatch");
  return false;
          }
     
            Debug.WriteLine("Checksum verification passed");
        }
      else
        {
          // Fast method: File size only
  Debug.WriteLine("Using fast verification (file size only)");
        }

 return true;
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

            // Simulate mode - skip actual file operations
 if (_appSettings.SimulateMode)
  {
         // Simulate processing delay
    await Task.Delay(500, cancellationToken);
   
       // Report simulated progress
       for (int i = 0; i <= 100; i += 20)
{
progress?.Report(i);
 await Task.Delay(100, cancellationToken);
    }
            
   movie.Status = ProcessingStatus.Completed;
    movie.ErrorMessage = "[SIMULATED] File would be processed successfully";
          return true;
   }

        // Ensure destination directory exists
   var destDirectory = Path.GetDirectoryName(destinationPath);
    if (!string.IsNullOrEmpty(destDirectory))
       {
      Directory.CreateDirectory(destDirectory);
 }

            // Copy file with progress using FileOperationProgress
        var fileOperation = new FileOperationProgress();
      
       // Wire up progress reporting
    if (progress != null)
      {
          fileOperation.ProgressChanged += (sender, args) =>
          {
              progress.Report((int)args.PercentComplete);
       };
       }

            bool copySuccess = await fileOperation.CopyFileWithRetryAsync(
       sourcePath,
     destinationPath,
        MaxRetries,
      BufferSize,
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

 // Delete the original source file after successful copy and verification
       try
       {
    if (File.Exists(sourcePath))
   {
         File.Delete(sourcePath);
         Debug.WriteLine($"Deleted original movie file: {sourcePath}");
       }
       }
       catch (Exception ex)
       {
 // Log warning but don't fail the operation since copy was successful
     Debug.WriteLine($"Warning: Could not delete original movie file {sourcePath}: {ex.Message}");
     movie.ErrorMessage = $"Copy successful but could not delete original: {ex.Message}";
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
