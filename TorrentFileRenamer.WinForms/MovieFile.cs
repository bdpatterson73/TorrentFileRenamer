using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TorrentFileRenamer
{
    internal class MovieFile
    {
        private string? _fileNamePath;
        private string _newParentRootDir;
        
        public MovieFile(string? fileNamePath, string newParentRootDir)
        {
            _fileNamePath = fileNamePath;
            _newParentRootDir = newParentRootDir;
            ProcessFile();
        }

        public string FileNamePath
        {
            get
            {
                return _fileNamePath;
            }
        }
        
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public string? MovieName { get; set; }
        public string? MovieYear { get; set; }
        public string NewDestDirectory { get; set; }

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

        /// <summary>
        /// Clean up movie names by removing quality indicators and other artifacts
        /// </summary>
        private string CleanMovieName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;
                
            // Remove common quality indicators and other artifacts
            string[] artifactsToRemove = {
                // Video quality
                "720P", "1080P", "480P", "4K", "UHD", "HDR", "2160P",
                // Source
                "HDTV", "WEB-DL", "WEBRip", "BluRay", "BRRip", "DVDRip", "CAMRip", "TS", "TC",
                "DVDSCR", "R5", "HDRIP", "BDRIP", "WEBRIP",
                // Codecs
                "x264", "x265", "H264", "H265", "HEVC", "AVC", "XVID", "DIVX",
                // Audio
                "AAC", "AC3", "DTS", "MP3", "FLAC", "DD5.1", "ATMOS",
                // Release info
                "PROPER", "REPACK", "EXTENDED", "UNRATED", "DIRECTORS.CUT", "DC", "THEATRICAL",
                "INTERNAL", "LIMITED", "FESTIVAL", "SCREENER",
                // Groups (common ones)
                "YIFY", "RARBG", "ETRG", "FGT", "SPARKS", "CMRG", "AMIABLE", "GECKOS",
                // Other
                "MULTI", "DUAL", "COMPLETE", "RERiP", "STV"
            };
            
            string cleanName = name;
            foreach (string artifact in artifactsToRemove)
            {
                // Remove whole word matches only
                cleanName = Regex.Replace(cleanName, $@"\b{Regex.Escape(artifact)}\b", "", RegexOptions.IgnoreCase);
            }
            
            // Remove common patterns like [GROUP], {GROUP}, etc.
            cleanName = Regex.Replace(cleanName, @"[\[\{].+?[\]\}]", "", RegexOptions.IgnoreCase);
            
            // Clean up extra spaces
            cleanName = Regex.Replace(cleanName, @"\s+", " ").Trim();
            
            return cleanName;
        }

        /// <summary>
        /// Extract movie year using multiple patterns
        /// </summary>
        private string ExtractMovieYear(string movieName)
        {
            // Pattern 1: Year in parentheses (2021)
            Regex yearInParentheses = new Regex(@"\((\d{4})\)", RegexOptions.IgnoreCase);
            Match match = yearInParentheses.Match(movieName);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Pattern 2: Year in brackets [2021]
            Regex yearInBrackets = new Regex(@"\[(\d{4})\]", RegexOptions.IgnoreCase);
            match = yearInBrackets.Match(movieName);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Pattern 3: Standalone year at the end
            Regex yearAtEnd = new Regex(@"\b(\d{4})\b\s*$", RegexOptions.IgnoreCase);
            match = yearAtEnd.Match(movieName);
            if (match.Success)
            {
                int year = int.Parse(match.Groups[1].Value);
                // Only accept reasonable movie years
                if (year >= 1900 && year <= DateTime.Now.Year + 5)
                {
                    return match.Groups[1].Value;
                }
            }

            // Pattern 4: Year anywhere in the string (less preferred)
            Regex yearAnywhere = new Regex(@"\b(19\d{2}|20\d{2})\b", RegexOptions.IgnoreCase);
            match = yearAnywhere.Match(movieName);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "";
        }

        /// <summary>
        /// Remove year from title using the specific year found
        /// </summary>
        private string RemoveYearFromTitle(string title, string year)
        {
            // Remove year in various formats
            string[] patterns = {
                $@"\(\s*{Regex.Escape(year)}\s*\)",  // (2021)
                $@"\[\s*{Regex.Escape(year)}\s*\]",  // [2021]
                $@"\b{Regex.Escape(year)}\b\s*$",    // 2021 at end
                $@"\b{Regex.Escape(year)}\b"         // 2021 anywhere
            };

            string result = title;
            foreach (string pattern in patterns)
            {
                result = Regex.Replace(result, pattern, "", RegexOptions.IgnoreCase).Trim();
                if (result != title) break; // Stop at first successful replacement
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
                    // For unknown movies, create a folder with just the filename (without extension)
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(FileName ?? "Unknown File");
                    string unknownMovieFolder = SanitizeForDirectoryName(fileNameWithoutExt);
                    NewDestDirectory = Path.Combine(_newParentRootDir, "Unknown", unknownMovieFolder, FileName ?? "Unknown File");
                    return;
                }

                // Clean up the movie name for directory creation
                string dirName = MovieName;
                
                // Handle articles (The, A, An) - move to end
                dirName = HandleArticles(dirName);
                
                // Get first letter for alphabetical organization
                char firstLetter = GetFirstLetterForSorting(dirName);
                string letterFolder = firstLetter.ToString().ToUpper();
                
                // Handle numbers and special characters
                if (char.IsDigit(firstLetter))
                {
                    letterFolder = "0-9";
                }
                else if (!char.IsLetter(firstLetter))
                {
                    letterFolder = "#";
                }

                // Create the movie folder name - use cleaned movie name with year if available
                string movieFolderName;
                if (!string.IsNullOrEmpty(MovieYear))
                {
                    movieFolderName = $"{MovieName} ({MovieYear})";
                }
                else
                {
                    movieFolderName = MovieName;
                }
                
                // Create the new filename - THIS IS THE KEY CHANGE!
                string newFileName;
                string extension = Path.GetExtension(FileName ?? "");
                
                if (!string.IsNullOrEmpty(MovieYear))
                {
                    newFileName = $"{MovieName} ({MovieYear}){extension}";
                }
                else
                {
                    newFileName = $"{MovieName}{extension}";
                }
                
                // Sanitize names for file system compatibility
                movieFolderName = SanitizeForDirectoryName(movieFolderName);
                newFileName = SanitizeForFileName(newFileName);

                // Create the full directory path: Root\Letter\MovieFolder\NewFileName
                string parentDir = Path.Combine(_newParentRootDir, letterFolder);
                string movieDir = Path.Combine(parentDir, movieFolderName);
                
                // Use the new formatted filename instead of the original
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

        /// <summary>
        /// Sanitize a string to be safe for use as a directory name
        /// </summary>
        private string SanitizeForDirectoryName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Unknown";
                
            // Remove invalid characters for Windows file system
            char[] invalidChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray();
            string sanitized = name;
            
            foreach (char c in invalidChars)
            {
                sanitized = sanitized.Replace(c, ' ');
            }
            
            // Replace multiple spaces with single space
            sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
            
            // Remove leading/trailing periods and spaces (Windows doesn't like these)
            sanitized = sanitized.Trim('.', ' ');
            
            // Ensure the name isn't empty after sanitization
            if (string.IsNullOrWhiteSpace(sanitized))
                return "Unknown";
                
            // Limit length to reasonable size (Windows max path considerations)
            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100).Trim();
            
            return sanitized;
        }

        /// <summary>
        /// Sanitize a string to be safe for use as a file name
        /// </summary>
        private string SanitizeForFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Unknown";
            
            // Get the extension separately
            string extension = Path.GetExtension(name);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(name);
            
            // Apply same sanitization as directory names
            nameWithoutExt = SanitizeForDirectoryName(nameWithoutExt);
            
            return nameWithoutExt + extension;
        }

        /// <summary>
        /// Handle articles by moving them to the end (The Matrix -> Matrix, The)
        /// </summary>
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

        /// <summary>
        /// Get the first letter for sorting, handling special cases
        /// </summary>
        private char GetFirstLetterForSorting(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return 'U'; // Unknown
                
            char firstChar = title.Trim().ToUpper()[0];
            
            // If it's a letter or number, return it
            if (char.IsLetterOrDigit(firstChar))
                return firstChar;
                
            // For special characters, put in # folder
            return '#';
        }

        // Keep these methods for backward compatibility but mark them as obsolete
        [Obsolete("Use ExtractMovieYear instead")]
        private bool DoesTitleContainYear(string movieName)
        {
            return !string.IsNullOrEmpty(ExtractMovieYear(movieName));
        }

        [Obsolete("Use RemoveYearFromTitle instead")]
        private string StripDate(string movieName)
        {
            string year = ExtractMovieYear(movieName);
            return string.IsNullOrEmpty(year) ? movieName : RemoveYearFromTitle(movieName, year);
        }
    }
}
