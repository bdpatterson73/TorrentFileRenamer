using System;
using System.IO;
using System.Threading.Tasks;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Demo class to demonstrate file copy progress functionality
    /// </summary>
    public static class FileProgressDemo
    {
        /// <summary>
        /// Demonstrates file copy with progress reporting
        /// </summary>
        public static async Task DemoFileProgress(string sourceFile, string destinationFile)
        {
            var fileOperation = new FileOperationProgress();
            
            // Subscribe to progress events
            fileOperation.ProgressChanged += (sender, args) => {
                if (args.IsComplete)
                {
                    Console.WriteLine($"? Copy completed! Final size: {args.FormattedProgress}");
                    Console.WriteLine($"??  Total time: {args.ElapsedTime.Minutes}m {args.ElapsedTime.Seconds}s");
                }
                else if (!string.IsNullOrEmpty(args.CustomMessage))
                {
                    Console.WriteLine($"?? {args.CustomMessage}");
                }
                else
                {
                    // Update progress line (overwrite previous line)
                    string progressLine = $"?? {Path.GetFileName(args.SourceFile)}: {args.FormattedProgress} at {args.FormattedSpeed}";
                    if (!string.IsNullOrEmpty(args.FormattedTimeRemaining))
                        progressLine += $" - ETA: {args.FormattedTimeRemaining}";
                    
                    Console.Write($"\r{progressLine}".PadRight(80));
                }
            };
            
            Console.WriteLine($"?? Starting copy of {Path.GetFileName(sourceFile)}...");
            
            bool success = await fileOperation.CopyFileWithRetryAsync(sourceFile, destinationFile);
            
            Console.WriteLine(); // New line after progress
            if (success)
            {
                Console.WriteLine("? File copy completed successfully!");
            }
            else
            {
                Console.WriteLine("? File copy failed!");
            }
        }
        
        /// <summary>
        /// Creates a demo file for testing purposes
        /// </summary>
        public static void CreateDemoFile(string filePath, int sizeInMB = 50)
        {
            Console.WriteLine($"Creating demo file: {filePath} ({sizeInMB} MB)");
            
            var random = new Random();
            byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                for (int i = 0; i < sizeInMB; i++)
                {
                    random.NextBytes(buffer);
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            
            Console.WriteLine($"? Demo file created: {new FileInfo(filePath).Length / (1024 * 1024)} MB");
        }
    }
}