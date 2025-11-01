using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Utility class for file operations that were previously in Form1.cs
    /// </summary>
    public static class FileOperationUtilities
    {
        private const int MAX_PATH = 255;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);

        /// <summary>
        /// Gets the short path name for a given file path
        /// </summary>
        /// <param name="path">The original file path</param>
        /// <returns>The short path name</returns>
        public static string GetShortPath(string path)
        {
            var shortPath = new StringBuilder(MAX_PATH);
            GetShortPathName(path, shortPath, MAX_PATH);
            return shortPath.ToString();
        }

        /// <summary>
        /// Deletes a file with retry logic using short path names
        /// </summary>
        /// <param name="filename">The filename to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool DeleteFileWithRetry(string filename)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    string shortPath = GetShortPath(filename);
                    File.Delete(shortPath);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Delete attempt {i + 1} failed: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }

            return false;
        }

        /// <summary>
        /// Retries a file move operation with specified number of retries
        /// </summary>
        /// <param name="sourceFile">Source file path</param>
        /// <param name="destFile">Destination file path</param>
        /// <param name="numRetries">Number of retries to attempt</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool RetryFileMove(string sourceFile, string destFile, int numRetries)
        {
            for (int i = 0; i < numRetries; i++)
            {
                try
                {
                    if (File.Exists(destFile))
                        File.Delete(destFile);
                    File.Copy(sourceFile, destFile);
                    return true;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"IO Exception on attempt {i + 1}. Waiting for file copy retry: {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
            return false;
        }

        /// <summary>
        /// Retries a file operation asynchronously with exponential backoff
        /// </summary>
        /// <param name="operation">The operation to retry</param>
        /// <param name="maxRetries">Maximum number of retries</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> RetryFileOperationAsync(Func<Task<bool>> operation, int maxRetries, CancellationToken cancellationToken)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return await operation();
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    LoggingService.LogWarning($"File operation attempt {attempt} failed, retrying in {attempt * 2} seconds...", "Processing");
                    await Task.Delay(attempt * 2000, cancellationToken);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"File operation attempt {attempt} failed", ex, "Processing");
                    if (attempt >= maxRetries)
                        throw;
                    await Task.Delay(attempt * 2000, cancellationToken);
                }
            }
            return false;
        }

        /// <summary>
        /// Copies a file asynchronously using FileOperationProgress
        /// </summary>
        /// <param name="source">Source file path</param>
        /// <param name="destination">Destination file path</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> CopyFileAsync(string source, string destination)
        {
            // This method is now replaced by FileOperationProgress.CopyFileAsync
            // Keeping for backward compatibility, but new code should use FileOperationProgress
            var fileOperation = new FileOperationProgress();
            return await fileOperation.CopyFileAsync(source, destination);
        }
    }
}