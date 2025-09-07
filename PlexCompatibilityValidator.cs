using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Validates media file naming and structure for Plex Media Server compatibility
    /// </summary>
    public static class PlexCompatibilityValidator
    {
        // Plex naming requirements
        private const int MAX_FILENAME_LENGTH = 255;
        private const int MAX_PATH_LENGTH = 260; // Windows limitation
        private const int RECOMMENDED_SHOW_NAME_LENGTH = 100;

        // Invalid characters for file/folder names
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        // Problematic characters that may cause issues with Plex
        private static readonly char[] ProblematicChars = { ':', '*', '?', '"', '<', '>', '|', '[', ']', '{', '}' };

        /// <summary>
        /// Validates TV episode naming for Plex compatibility
        /// </summary>
        public static PlexValidationResult ValidateTVEpisode(string showName, int seasonNumber, int episodeNumber, 
            string fileName, string fullPath, bool isAutoMode = false)
        {
            var issues = new List<string>();
            var warnings = new List<string>();

            // 1. Check show name validity
            if (string.IsNullOrWhiteSpace(showName) || showName == "Unknown Show")
            {
                issues.Add("Show name is missing or unknown");
            }
            else
            {
                // Check for invalid characters in show name
                if (showName.Any(c => InvalidFileNameChars.Contains(c) || ProblematicChars.Contains(c)))
                {
                    issues.Add($"Show name contains invalid characters: '{showName}'");
                }

                // Check show name length
                if (showName.Length > RECOMMENDED_SHOW_NAME_LENGTH)
                {
                    warnings.Add($"Show name is very long ({showName.Length} chars). May cause path length issues.");
                }

                // Check for problematic patterns
                if (IsProblematicShowName(showName))
                {
                    warnings.Add($"Show name may cause Plex detection issues: '{showName}'");
                }
            }

            // 2. Check season/episode validity
            if (seasonNumber < 0)
            {
                issues.Add($"Invalid season number: {seasonNumber}");
            }
            if (episodeNumber < 0)
            {
                issues.Add($"Invalid episode number: {episodeNumber}");
            }

            // 3. Check filename format
            if (!IsValidPlexTVEpisodeFormat(fileName))
            {
                issues.Add($"Filename doesn't follow Plex naming convention: '{fileName}'");
            }

            // 4. Check path length
            if (fullPath.Length > MAX_PATH_LENGTH)
            {
                issues.Add($"Full path too long ({fullPath.Length} chars). Windows limit is {MAX_PATH_LENGTH}.");
            }

            // 5. Check for potential encoding issues
            if (HasEncodingIssues(fileName) || HasEncodingIssues(showName))
            {
                warnings.Add("Filename contains characters that may cause encoding issues");
            }

            // 6. Check file extension
            string extension = Path.GetExtension(fileName);
            if (!IsValidVideoExtension(extension))
            {
                warnings.Add($"Unusual video file extension: {extension}");
            }

            return new PlexValidationResult
            {
                IsValid = issues.Count == 0,
                Issues = issues,
                Warnings = warnings,
                SuggestedAction = DetermineAction(issues, warnings, isAutoMode),
                CanAutoFix = CanAutoFixIssues(issues)
            };
        }

        /// <summary>
        /// Attempts to fix common Plex compatibility issues
        /// </summary>
        public static string FixShowNameForPlex(string showName)
        {
            if (string.IsNullOrWhiteSpace(showName))
                return "Unknown Show";

            string fixedName = showName;

            // Remove/replace invalid characters
            foreach (char c in InvalidFileNameChars.Concat(ProblematicChars))
            {
                fixedName = fixedName.Replace(c, ' ');
            }

            // Clean up multiple spaces
            fixedName = Regex.Replace(fixedName, @"\s+", " ").Trim();

            // Remove leading/trailing dots and spaces
            fixedName = fixedName.Trim(' ', '.', '-', '_');

            // Handle empty result
            if (string.IsNullOrWhiteSpace(fixedName))
                fixedName = "Unknown Show";

            // Truncate if too long
            if (fixedName.Length > RECOMMENDED_SHOW_NAME_LENGTH)
            {
                fixedName = fixedName.Substring(0, RECOMMENDED_SHOW_NAME_LENGTH).Trim();
            }

            return fixedName;
        }

        /// <summary>
        /// Suggests a Plex-compatible filename for TV episodes
        /// </summary>
        public static string SuggestPlexTVFilename(string showName, int seasonNumber, int episodeNumber, 
            string extension, List<int>? additionalEpisodes = null)
        {
            string cleanShowName = FixShowNameForPlex(showName).Replace(" ", ".");
            string seasonPart = $"S{seasonNumber:D2}";
            
            string episodePart;
            if (additionalEpisodes != null && additionalEpisodes.Count > 0)
            {
                // Multi-episode format
                var allEpisodes = new List<int> { episodeNumber };
                allEpisodes.AddRange(additionalEpisodes);
                allEpisodes = allEpisodes.Distinct().OrderBy(x => x).ToList();
                
                if (allEpisodes.Count == 2 && allEpisodes[1] - allEpisodes[0] == 1)
                {
                    // Consecutive episodes: S01E01-E02
                    episodePart = $"E{allEpisodes[0]:D2}-E{allEpisodes[1]:D2}";
                }
                else
                {
                    // Multiple episodes: S01E01E02E03
                    episodePart = string.Join("", allEpisodes.Select(ep => $"E{ep:D2}"));
                }
            }
            else
            {
                episodePart = $"E{episodeNumber:D2}";
            }

            return $"{cleanShowName}.{seasonPart}{episodePart}{extension}";
        }

        /// <summary>
        /// Prompts user for manual filename correction
        /// </summary>
        public static string PromptForCorrection(string originalName, string suggestedName, List<string> issues)
        {
            // This would typically show a dialog - for now return the suggestion
            // In a real implementation, you'd show a dialog with:
            // - Original name
            // - Issues found
            // - Suggested correction
            // - Allow user to manually edit
            return suggestedName;
        }

        #region Helper Methods

        private static bool IsValidPlexTVEpisodeFormat(string fileName)
        {
            // Plex accepts various formats, but prefers: Show.Name.S01E01.ext
            var patterns = new[]
            {
                @"^.+\.S\d{1,2}E\d{1,2}.*\.\w{2,4}$", // Show.Name.S01E01.ext
                @"^.+\.S\d{1,2}E\d{1,2}-E\d{1,2}.*\.\w{2,4}$", // Multi-episode
                @"^.+\.S\d{1,2}E\d{1,2}E\d{1,2}.*\.\w{2,4}$", // Multi-episode alternative
            };

            return patterns.Any(pattern => Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase));
        }

        private static bool IsProblematicShowName(string showName)
        {
            // Check for patterns that might confuse Plex
            var problematicPatterns = new[]
            {
                @"^\d{4}$", // Just a year
                @"^(Season|Episode|EP|S\d|E\d)", // Starts with episode indicators
                @"(HDTV|720p|1080p|WEB-DL|BluRay)", // Contains quality indicators
                @"^(The\s+)*Unknown", // Variations of "Unknown"
            };

            return problematicPatterns.Any(pattern => 
                Regex.IsMatch(showName, pattern, RegexOptions.IgnoreCase));
        }

        private static bool HasEncodingIssues(string text)
        {
            // Check for common encoding problem indicators
            return text.Any(c => c > 127 && c < 160) || // Extended ASCII range that often causes issues
                   text.Contains("?") || // Often a sign of encoding problems
                   Regex.IsMatch(text, @"[^\x00-\x7F]+"); // Non-ASCII characters
        }

        private static bool IsValidVideoExtension(string extension)
        {
            var validExtensions = new[] { ".mp4", ".mkv", ".avi", ".m4v", ".mov", ".wmv", ".flv", ".webm" };
            return validExtensions.Contains(extension.ToLower());
        }

        private static PlexValidationAction DetermineAction(List<string> issues, List<string> warnings, bool isAutoMode)
        {
            if (issues.Count > 0)
            {
                return isAutoMode ? PlexValidationAction.SkipInAutoMode : PlexValidationAction.PromptUser;
            }
            
            if (warnings.Count > 0)
            {
                return isAutoMode ? PlexValidationAction.ProcessWithWarnings : PlexValidationAction.ShowWarnings;
            }

            return PlexValidationAction.ProcessNormally;
        }

        private static bool CanAutoFixIssues(List<string> issues)
        {
            // We can auto-fix character issues, but not missing show names or invalid numbers
            return issues.All(issue => 
                issue.Contains("invalid characters") || 
                issue.Contains("naming convention") ||
                issue.Contains("encoding issues"));
        }

        #endregion
    }

    /// <summary>
    /// Result of Plex compatibility validation
    /// </summary>
    public class PlexValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public PlexValidationAction SuggestedAction { get; set; }
        public bool CanAutoFix { get; set; }
    }

    /// <summary>
    /// Actions to take based on validation results
    /// </summary>
    public enum PlexValidationAction
    {
        ProcessNormally,
        ShowWarnings,
        PromptUser,
        SkipInAutoMode,
        ProcessWithWarnings
    }
}