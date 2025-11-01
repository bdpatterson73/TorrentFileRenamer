using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace TorrentFileRenamer.Core.Models
{
    public class MovieFile
    {
        private string? _fileNamePath;
        private string _newParentRootDir;
        
        public MovieFile(string? fileNamePath, string newParentRootDir)
        {
       _fileNamePath = fileNamePath;
   _newParentRootDir = newParentRootDir;
          ProcessFile();
        }

        public string? FileNamePath => _fileNamePath;
    public string? FilePath { get; set; }
      public string? FileName { get; set; }
        public string? MovieName { get; set; }
public string? MovieYear { get; set; }
        public string NewDestDirectory { get; set; } = "";

        private void ProcessFile()
        {
   GetFilePath();
   GetFileName();
        GetMovieName();
GetNewParentDirectory();
        }

 private void GetMovieName()
        {
            if (string.IsNullOrWhiteSpace(FileName))
    {
            MovieName = "Unknown Movie";
  return;
   }

            try
            {
       string extension = Path.GetExtension(_fileNamePath).ToUpper().Trim();
            string newfileName = FileName.Replace(extension, "", StringComparison.OrdinalIgnoreCase).Trim();
        
                // Replace common separators with spaces
           newfileName = newfileName.Replace(".", " ").Replace("_", " ").Replace("-", " ").Trim();
            
   // Clean up quality indicators and other artifacts
         newfileName = CleanMovieName(newfileName);
            
    // Try to extract year using multiple patterns
     string extractedYear = ExtractMovieYear(newfileName);
      if (!string.IsNullOrEmpty(extractedYear))
     {
        MovieYear = extractedYear;
         MovieName = RemoveYearFromTitle(newfileName, extractedYear).Trim();
      }
         else
       {
     MovieName = newfileName.Trim();
       MovieYear = "";
   }

      // Final cleanup
       if (string.IsNullOrWhiteSpace(MovieName))
{
       MovieName = "Unknown Movie";
    }
   else
    {
          // Remove extra spaces
       MovieName = Regex.Replace(MovieName, @"\s+", " ").Trim();
      }

   Debug.WriteLine($"Parsed movie: '{MovieName}' ({MovieYear}) from '{FileName}'");
      }
       catch (Exception ex)
       {
                Debug.WriteLine($"Error parsing movie name from '{FileName}': {ex.Message}");
        MovieName = "Unknown Movie";
   MovieYear = "";
            }
        }

   private string CleanMovieName(string name)
        {
   if (string.IsNullOrWhiteSpace(name))
           return name;
         
 string[] artifactsToRemove = {
   "720P", "1080P", "480P", "4K", "UHD", "HDR", "2160P",
     "HDTV", "WEB-DL", "WEBRip", "BluRay", "BRRip", "DVDRip", "CAMRip", "TS", "TC",
         "DVDSCR", "R5", "HDRIP", "BDRIP", "WEBRIP",
         "x264", "x265", "H264", "H265", "HEVC", "AVC", "XVID", "DIVX",
   "AAC", "AC3", "DTS", "MP3", "FLAC", "DD5.1", "ATMOS",
       "PROPER", "REPACK", "EXTENDED", "UNRATED", "DIRECTORS.CUT", "DC", "THEATRICAL",
      "INTERNAL", "LIMITED", "FESTIVAL", "SCREENER",
         "YIFY", "RARBG", "ETRG", "FGT", "SPARKS", "CMRG", "AMIABLE", "GECKOS",
      "MULTI", "DUAL", "COMPLETE", "RERiP", "STV"
     };
   
            string cleanName = name;
            foreach (string artifact in artifactsToRemove)
  {
   cleanName = Regex.Replace(cleanName, $@"\b{Regex.Escape(artifact)}\b", "", RegexOptions.IgnoreCase);
            }
            
      cleanName = Regex.Replace(cleanName, @"[\[\{].+?[\]\}]", "", RegexOptions.IgnoreCase);
            cleanName = Regex.Replace(cleanName, @"\s+", " ").Trim();
   
return cleanName;
   }

        private string ExtractMovieYear(string movieName)
        {
        Regex yearInParentheses = new Regex(@"\((\d{4})\)", RegexOptions.IgnoreCase);
            Match match = yearInParentheses.Match(movieName);
       if (match.Success)
    return match.Groups[1].Value;

      Regex yearInBrackets = new Regex(@"\[(\d{4})\]", RegexOptions.IgnoreCase);
            match = yearInBrackets.Match(movieName);
         if (match.Success)
       return match.Groups[1].Value;

            Regex yearAtEnd = new Regex(@"\b(\d{4})\b\s*$", RegexOptions.IgnoreCase);
 match = yearAtEnd.Match(movieName);
            if (match.Success)
       {
      int year = int.Parse(match.Groups[1].Value);
           if (year >= 1900 && year <= DateTime.Now.Year + 5)
 return match.Groups[1].Value;
    }

     Regex yearAnywhere = new Regex(@"\b(19\d{2}|20\d{2})\b", RegexOptions.IgnoreCase);
            match = yearAnywhere.Match(movieName);
  if (match.Success)
        return match.Groups[1].Value;

     return "";
        }

        private string RemoveYearFromTitle(string title, string year)
        {
  string[] patterns = {
  $@"\(\s*{Regex.Escape(year)}\s*\)",
        $@"\[\s*{Regex.Escape(year)}\s*\]",
      $@"\b{Regex.Escape(year)}\b\s*$",
                $@"\b{Regex.Escape(year)}\b"
            };

            string result = title;
            foreach (string pattern in patterns)
      {
 result = Regex.Replace(result, pattern, "", RegexOptions.IgnoreCase).Trim();
 if (result != title) break;
       }

  return result;
        }

        private void GetFilePath()
        {
     if (_fileNamePath != null)
                FilePath = Path.GetDirectoryName(_fileNamePath)?.Trim();
        }

        private void GetFileName()
        {
     if (!string.IsNullOrWhiteSpace(FilePath))
FileName = _fileNamePath?.Replace(FilePath, "").Replace("\\", "");
        }

        private void GetNewParentDirectory()
        {
            try
 {
        if (string.IsNullOrWhiteSpace(MovieName) || MovieName == "Unknown Movie")
       {
   string fileNameWithoutExt = Path.GetFileNameWithoutExtension(FileName ?? "Unknown File");
  string unknownMovieFolder = SanitizeForDirectoryName(fileNameWithoutExt);
      NewDestDirectory = Path.Combine(_newParentRootDir, "Unknown", unknownMovieFolder, FileName ?? "Unknown File");
          return;
   }

       string dirName = MovieName;
                dirName = HandleArticles(dirName);
          char firstLetter = GetFirstLetterForSorting(dirName);
        string letterFolder = firstLetter.ToString().ToUpper();
          
   if (char.IsDigit(firstLetter))
     letterFolder = "0-9";
    else if (!char.IsLetter(firstLetter))
     letterFolder = "#";

           string movieFolderName;
  if (!string.IsNullOrEmpty(MovieYear))
            movieFolderName = $"{MovieName} ({MovieYear})";
  else
          movieFolderName = MovieName;
                
     string newFileName;
                string extension = Path.GetExtension(FileName ?? "");
     
           if (!string.IsNullOrEmpty(MovieYear))
      newFileName = $"{MovieName} ({MovieYear}){extension}";
     else
              newFileName = $"{MovieName}{extension}";
           
           movieFolderName = SanitizeForDirectoryName(movieFolderName);
                newFileName = SanitizeForFileName(newFileName);

     string parentDir = Path.Combine(_newParentRootDir, letterFolder);
       string movieDir = Path.Combine(parentDir, movieFolderName);
                NewDestDirectory = Path.Combine(movieDir, newFileName);

            Debug.WriteLine($"Movie directory: {NewDestDirectory}");
         Debug.WriteLine($"New filename: {newFileName}");
  }
   catch (Exception ex)
         {
         Debug.WriteLine($"Error creating directory path: {ex.Message}");
   string fallbackFolder = SanitizeForDirectoryName(Path.GetFileNameWithoutExtension(FileName ?? "Unknown"));
    NewDestDirectory = Path.Combine(_newParentRootDir, "Unknown", fallbackFolder, FileName ?? "Unknown File");
         }
        }

        private string SanitizeForDirectoryName(string name)
        {
    if (string.IsNullOrWhiteSpace(name))
  return "Unknown";
    
            char[] invalidChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray();
            string sanitized = name;
   
            foreach (char c in invalidChars)
        sanitized = sanitized.Replace(c, ' ');
            
         sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
            sanitized = sanitized.Trim('.', ' ');
     
            if (string.IsNullOrWhiteSpace(sanitized))
           return "Unknown";
             
if (sanitized.Length > 100)
          sanitized = sanitized.Substring(0, 100).Trim();
       
         return sanitized;
        }

 private string SanitizeForFileName(string name)
        {
       if (string.IsNullOrWhiteSpace(name))
   return "Unknown";
 
            string extension = Path.GetExtension(name);
  string nameWithoutExt = Path.GetFileNameWithoutExtension(name);
   nameWithoutExt = SanitizeForDirectoryName(nameWithoutExt);
            
     return nameWithoutExt + extension;
        }

     private string HandleArticles(string title)
        {
  string[] articles = { "The ", "A ", "An " };
  
            foreach (string article in articles)
       {
   if (title.StartsWith(article, StringComparison.OrdinalIgnoreCase))
                {
       string remaining = title.Substring(article.Length).Trim();
          return $"{remaining}, {article.Trim()}";
       }
      }

      return title;
        }

        private char GetFirstLetterForSorting(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
      return 'U';
        
            char firstChar = title.Trim().ToUpper()[0];
       
          if (char.IsLetterOrDigit(firstChar))
    return firstChar;
        
            return '#';
        }
  }
}
