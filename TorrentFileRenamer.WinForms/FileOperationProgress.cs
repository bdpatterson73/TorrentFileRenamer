using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Provides file copy operations with progress feedback and transfer statistics
    /// </summary>
    public class FileOperationProgress
    {
        public event EventHandler<FileProgressEventArgs>? ProgressChanged;

        private const int DefaultBufferSize = 1024 * 1024; // 1MB buffer for large files

        /// <summary>
        /// Copies a file from source to destination with progress reporting
        /// </summary>
        /// <param name="sourceFile">Source file path</param>
        /// <param name="destinationFile">Destination file path</param>
        /// <param name="bufferSize">Buffer size for copying (default 1MB)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if copy was successful</returns>
        public async Task<bool> CopyFileAsync(string sourceFile, string destinationFile,
            int bufferSize = DefaultBufferSize, CancellationToken cancellationToken = default)
        {
            try
            {
                var fileInfo = new FileInfo(sourceFile);
                if (!fileInfo.Exists)
                    return false;

                var stopwatch = Stopwatch.StartNew();
                long totalBytes = fileInfo.Length;
                long copiedBytes = 0;
                DateTime lastUpdate = DateTime.Now;

                // Ensure destination directory exists
                var destDir = Path.GetDirectoryName(destinationFile);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
                using (var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize))
                {
                    var buffer = new byte[bufferSize];
                    int bytesRead;

                    // Report initial progress
                    ReportProgress(sourceFile, destinationFile, 0, totalBytes, 0, stopwatch.Elapsed);

                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        copiedBytes += bytesRead;

                        // Report progress every 100ms or on completion
                        var now = DateTime.Now;
                        if ((now - lastUpdate).TotalMilliseconds >= 100 || copiedBytes >= totalBytes)
                        {
                            double currentSpeed = CalculateSpeed(copiedBytes, stopwatch.Elapsed);
                            ReportProgress(sourceFile, destinationFile, copiedBytes, totalBytes, currentSpeed, stopwatch.Elapsed);
                            lastUpdate = now;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                stopwatch.Stop();

                // Report final completion
                double finalSpeed = CalculateSpeed(copiedBytes, stopwatch.Elapsed);
                ReportProgress(sourceFile, destinationFile, copiedBytes, totalBytes, finalSpeed, stopwatch.Elapsed, true);

                return true;
            }
            catch (OperationCanceledException)
            {
                // Clean up partial file if cancelled
                try
                {
                    if (File.Exists(destinationFile))
                        File.Delete(destinationFile);
                }
                catch
                {
                    /* Ignore cleanup errors */
                }

                throw;
            }
            catch (Exception ex)
            {
                // Clean up partial file on error
                try
                {
                    if (File.Exists(destinationFile))
                        File.Delete(destinationFile);
                }
                catch
                {
                    /* Ignore cleanup errors */
                }

                Debug.WriteLine($"File copy error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Copy file with multiple retry attempts and progress reporting
        /// </summary>
        public async Task<bool> CopyFileWithRetryAsync(string sourceFile, string destinationFile,
            int maxRetries = 3, int bufferSize = DefaultBufferSize, CancellationToken cancellationToken = default)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Report retry attempt
                    if (attempt > 1)
                    {
                        ReportProgress(sourceFile, destinationFile, 0, 0, 0, TimeSpan.Zero,
                            false, $"Retry attempt {attempt}/{maxRetries}");
                    }

                    return await CopyFileAsync(sourceFile, destinationFile, bufferSize, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    ReportProgress(sourceFile, destinationFile, 0, 0, 0, TimeSpan.Zero,
                        false, $"Copy attempt {attempt} failed, retrying in 5 seconds...");
                    await Task.Delay(5000, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (attempt >= maxRetries)
                    {
                        ReportProgress(sourceFile, destinationFile, 0, 0, 0, TimeSpan.Zero,
                            false, $"Copy failed after {maxRetries} attempts: {ex.Message}");
                        return false;
                    }

                    await Task.Delay(5000, cancellationToken);
                }
            }

            return false;
        }

        private void ReportProgress(string sourceFile, string destinationFile, long copiedBytes, long totalBytes,
            double speedBytesPerSecond, TimeSpan elapsed, bool isComplete = false, string customMessage = "")
        {
            var args = new FileProgressEventArgs
            {
                SourceFile = sourceFile,
                DestinationFile = destinationFile,
                CopiedBytes = copiedBytes,
                TotalBytes = totalBytes,
                PercentComplete = totalBytes > 0 ? (double)copiedBytes / totalBytes * 100 : 0,
                SpeedBytesPerSecond = speedBytesPerSecond,
                ElapsedTime = elapsed,
                IsComplete = isComplete,
                CustomMessage = customMessage
            };

            ProgressChanged?.Invoke(this, args);
        }

        private static double CalculateSpeed(long bytes, TimeSpan elapsed)
        {
            if (elapsed.TotalSeconds <= 0)
                return 0;
            return bytes / elapsed.TotalSeconds;
        }
    }

    /// <summary>
    /// Event arguments for file progress reporting
    /// </summary>
    public class FileProgressEventArgs : EventArgs
    {
        public string SourceFile { get; set; } = "";
        public string DestinationFile { get; set; } = "";
        public long CopiedBytes { get; set; }
        public long TotalBytes { get; set; }
        public double PercentComplete { get; set; }
        public double SpeedBytesPerSecond { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public bool IsComplete { get; set; }
        public string CustomMessage { get; set; } = "";

        /// <summary>
        /// Get formatted speed string (e.g., "15.2 MB/s")
        /// </summary>
        public string FormattedSpeed => FormatBytes(SpeedBytesPerSecond) + "/s";

        /// <summary>
        /// Get formatted progress string (e.g., "256 MB / 1.2 GB (21.3%)")
        /// </summary>
        public string FormattedProgress =>
            $"{FormatBytes(CopiedBytes)} / {FormatBytes(TotalBytes)} ({PercentComplete:F1}%)";

        /// <summary>
        /// Get estimated time remaining based on current speed
        /// </summary>
        public TimeSpan EstimatedTimeRemaining
        {
            get
            {
                if (SpeedBytesPerSecond <= 0 || IsComplete || CopiedBytes >= TotalBytes)
                    return TimeSpan.Zero;

                long remainingBytes = TotalBytes - CopiedBytes;
                double secondsRemaining = remainingBytes / SpeedBytesPerSecond;
                return TimeSpan.FromSeconds(secondsRemaining);
            }
        }

        /// <summary>
        /// Get formatted time remaining (e.g., "2m 35s")
        /// </summary>
        public string FormattedTimeRemaining
        {
            get
            {
                var remaining = EstimatedTimeRemaining;
                if (remaining == TimeSpan.Zero)
                    return "";

                if (remaining.TotalHours >= 1)
                    return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
                else if (remaining.TotalMinutes >= 1)
                    return $"{remaining.Minutes}m {remaining.Seconds}s";
                else
                    return $"{remaining.Seconds}s";
            }
        }

        private static string FormatBytes(double bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            return $"{size:0.#} {sizes[order]}";
        }
    }
}