using System.Diagnostics;

namespace TorrentFileRenamer.Core.Utilities
{
    public class FileOperationProgress
    {
        public event EventHandler<FileProgressEventArgs>? ProgressChanged;

        private const int DefaultBufferSize = 1024 * 1024;

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

                    ReportProgress(sourceFile, destinationFile, 0, totalBytes, 0, stopwatch.Elapsed);

                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        copiedBytes += bytesRead;

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
                double finalSpeed = CalculateSpeed(copiedBytes, stopwatch.Elapsed);
                ReportProgress(sourceFile, destinationFile, copiedBytes, totalBytes, finalSpeed, stopwatch.Elapsed, true);

                return true;
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (File.Exists(destinationFile))
                        File.Delete(destinationFile);
                }
                catch
                {
                }

                throw;
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(destinationFile))
                        File.Delete(destinationFile);
                }
                catch
                {
                }

                Debug.WriteLine($"File copy error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CopyFileWithRetryAsync(string sourceFile, string destinationFile,
            int maxRetries = 3, int bufferSize = DefaultBufferSize, CancellationToken cancellationToken = default)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

        public string FormattedSpeed => FormatBytes(SpeedBytesPerSecond) + "/s";

        public string FormattedProgress =>
            $"{FormatBytes(CopiedBytes)} / {FormatBytes(TotalBytes)} ({PercentComplete:F1}%)";

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