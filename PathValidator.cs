using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Utility class for path validation and disk space checking
    /// </summary>
    public static class PathValidator
    {
        /// <summary>
        /// Validates that a path exists and is accessible
        /// </summary>
        public static ValidationResult ValidateSourcePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return new ValidationResult(false, "Source path cannot be empty");

            try
            {
                if (!Directory.Exists(path))
                    return new ValidationResult(false, "Source directory does not exist");

                // Try to read the directory to check permissions
                Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                return new ValidationResult(true, "Source path is valid");
            }
            catch (UnauthorizedAccessException)
            {
                return new ValidationResult(false, "Access denied to source directory");
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, $"Error accessing source path: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates destination path and checks available space
        /// </summary>
        public static ValidationResult ValidateDestinationPath(string path, long requiredSpaceBytes = 0)
        {
            if (string.IsNullOrWhiteSpace(path))
                return new ValidationResult(false, "Destination path cannot be empty");

            try
            {
                // Check if path is a valid path format
                if (!IsValidPath(path))
                    return new ValidationResult(false, "Invalid path format");

                // Check if directory exists, if not try to create it
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        return new ValidationResult(false, $"Cannot create destination directory: {ex.Message}");
                    }
                }

                // Check write permissions
                if (!HasWritePermission(path))
                    return new ValidationResult(false, "No write permission to destination directory");

                // Check available disk space
                var spaceInfo = GetDiskSpaceInfo(path);
                if (spaceInfo.HasValue)
                {
                    if (requiredSpaceBytes > 0 && spaceInfo.Value.FreeBytes < requiredSpaceBytes)
                    {
                        return new ValidationResult(false, 
                            $"Insufficient disk space. Required: {FormatBytes(requiredSpaceBytes)}, Available: {FormatBytes(spaceInfo.Value.FreeBytes)}");
                    }
                    
                    return new ValidationResult(true, 
                        $"Destination path is valid. Available space: {FormatBytes(spaceInfo.Value.FreeBytes)}");
                }

                return new ValidationResult(true, "Destination path is valid");
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, $"Error validating destination path: {ex.Message}");
            }
        }

        /// <summary>
        /// Estimates total size of files to be processed
        /// </summary>
        public static long EstimateRequiredSpace(string sourcePath, string fileExtension)
        {
            try
            {
                string[] files = Directory.GetFiles(sourcePath, $"*{fileExtension}", SearchOption.AllDirectories);
                long totalSize = 0;

                foreach (string file in files.Take(100)) // Sample first 100 files for performance
                {
                    FileInfo fi = new FileInfo(file);
                    totalSize += fi.Length;
                }

                // If we sampled less than all files, extrapolate
                if (files.Length > 100)
                {
                    totalSize = (long)(totalSize * ((double)files.Length / 100));
                }

                return totalSize;
            }
            catch (Exception)
            {
                return 0; // Return 0 if estimation fails
            }
        }

        /// <summary>
        /// Checks if a path has valid format
        /// </summary>
        private static bool IsValidPath(string path)
        {
            try
            {
                Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks write permission to a directory
        /// </summary>
        private static bool HasWritePermission(string directoryPath)
        {
            try
            {
                string testFile = Path.Combine(directoryPath, $"temp_test_{Guid.NewGuid()}.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets disk space information for a path
        /// </summary>
        private static (long TotalBytes, long FreeBytes)? GetDiskSpaceInfo(string path)
        {
            try
            {
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(path));
                return (drive.TotalSize, drive.AvailableFreeSpace);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Formats bytes to human readable format
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }

        /// <summary>
        /// Checks if a path is a network path
        /// </summary>
        public static bool IsNetworkPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return path.StartsWith(@"\\") || path.StartsWith(@"//");
        }

        /// <summary>
        /// Validates file extension format
        /// </summary>
        public static ValidationResult ValidateFileExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return new ValidationResult(false, "File extension cannot be empty");

            // Add dot if missing
            if (!extension.StartsWith("."))
                extension = "." + extension;

            // Check for invalid characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (extension.Any(c => invalidChars.Contains(c)))
                return new ValidationResult(false, "File extension contains invalid characters");

            return new ValidationResult(true, "File extension is valid");
        }
    }

    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string Message { get; }

        public ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }
    }
}