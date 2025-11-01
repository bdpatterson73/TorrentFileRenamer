using System.Text.RegularExpressions;

namespace TorrentFileRenamer.Core.Services
{
    public static class PathValidator
    {
 public static ValidationResult ValidateSourcePath(string path)
        {
      if (string.IsNullOrWhiteSpace(path))
        return new ValidationResult(false, "Source path cannot be empty");

            try
         {
   if (!Directory.Exists(path))
    return new ValidationResult(false, "Source directory does not exist");

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

        public static ValidationResult ValidateDestinationPath(string path, long requiredSpaceBytes = 0)
  {
   if (string.IsNullOrWhiteSpace(path))
   return new ValidationResult(false, "Destination path cannot be empty");

       try
            {
         if (!IsValidPath(path))
     return new ValidationResult(false, "Invalid path format");

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

   if (!HasWritePermission(path))
           return new ValidationResult(false, "No write permission to destination directory");

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

        public static long EstimateRequiredSpace(string sourcePath, string fileExtension)
        {
       try
 {
                string[] files = Directory.GetFiles(sourcePath, $"*{fileExtension}", SearchOption.AllDirectories);
   long totalSize = 0;

   foreach (string file in files.Take(100))
                {
           FileInfo fi = new FileInfo(file);
         totalSize += fi.Length;
                }

             if (files.Length > 100)
    {
            totalSize = (long)(totalSize * ((double)files.Length / 100));
        }

  return totalSize;
     }
       catch (Exception)
       {
       return 0;
     }
        }

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

        private static (long TotalBytes, long FreeBytes)? GetDiskSpaceInfo(string path)
        {
            try
         {
       DriveInfo drive = new DriveInfo(Path.GetPathRoot(path)!);
       return (drive.TotalSize, drive.AvailableFreeSpace);
  }
            catch
            {
      return null;
   }
        }

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

        public static bool IsNetworkPath(string path)
        {
          if (string.IsNullOrWhiteSpace(path))
       return false;

        return path.StartsWith(@"\\") || path.StartsWith(@"//");
        }

        public static ValidationResult ValidateFileExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
 return new ValidationResult(false, "File extension cannot be empty");

         if (!extension.StartsWith("."))
             extension = "." + extension;

      char[] invalidChars = Path.GetInvalidFileNameChars();
        if (extension.Any(c => invalidChars.Contains(c)))
   return new ValidationResult(false, "File extension contains invalid characters");

  return new ValidationResult(true, "File extension is valid");
        }
    }

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
