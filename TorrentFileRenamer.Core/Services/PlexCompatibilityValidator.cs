using System.Text.RegularExpressions;

namespace TorrentFileRenamer.Core.Services
{
    public static class PlexCompatibilityValidator
    {
        private const int MAX_FILENAME_LENGTH = 255;
        private const int MAX_PATH_LENGTH = 260;
        private const int RECOMMENDED_SHOW_NAME_LENGTH = 100;

        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        private static readonly char[] ProblematicChars = { ':', '*', '?', '"', '<', '>', '|', '[', ']', '{', '}' };

        public static PlexValidationResult ValidateTVEpisode(string showName, int seasonNumber, int episodeNumber,
            string fileName, string fullPath, bool isAutoMode = false)
        {
            var issues = new List<string>();
            var warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(showName) || showName == "Unknown Show")
            {
                issues.Add("Show name is missing or unknown");
            }
            else
            {
                if (showName.Any(c => InvalidFileNameChars.Contains(c) || ProblematicChars.Contains(c)))
                {
                    issues.Add($"Show name contains invalid characters: '{showName}'");
                }

                if (showName.Length > RECOMMENDED_SHOW_NAME_LENGTH)
                {
                    warnings.Add($"Show name is very long ({showName.Length} chars). May cause path length issues.");
                }

                if (IsProblematicShowName(showName))
                {
                    warnings.Add($"Show name may cause Plex detection issues: '{showName}'");
                }
            }

            if (seasonNumber < 0)
            {
                issues.Add($"Invalid season number: {seasonNumber}");
            }

            if (episodeNumber < 0)
            {
                issues.Add($"Invalid episode number: {episodeNumber}");
            }

            if (!IsValidPlexTVEpisodeFormat(fileName))
            {
                issues.Add($"Filename doesn't follow Plex naming convention: '{fileName}'");
            }

            if (fullPath.Length > MAX_PATH_LENGTH)
            {
                issues.Add($"Full path too long ({fullPath.Length} chars). Windows limit is {MAX_PATH_LENGTH}.");
            }

            if (HasEncodingIssues(fileName) || HasEncodingIssues(showName))
            {
                warnings.Add("Filename contains characters that may cause encoding issues");
            }

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

        public static string FixShowNameForPlex(string showName)
        {
            if (string.IsNullOrWhiteSpace(showName))
                return "Unknown Show";

            string fixedName = showName;

            foreach (char c in InvalidFileNameChars.Concat(ProblematicChars))
            {
                fixedName = fixedName.Replace(c, ' ');
            }

            fixedName = Regex.Replace(fixedName, @"\s+", " ").Trim();
            fixedName = fixedName.Trim(' ', '.', '-', '_');

            if (string.IsNullOrWhiteSpace(fixedName))
                fixedName = "Unknown Show";

            if (fixedName.Length > RECOMMENDED_SHOW_NAME_LENGTH)
            {
                fixedName = fixedName.Substring(0, RECOMMENDED_SHOW_NAME_LENGTH).Trim();
            }

            return fixedName;
        }

        public static string SuggestPlexTVFilename(string showName, int seasonNumber, int episodeNumber,
            string extension, List<int>? additionalEpisodes = null)
        {
            string cleanShowName = FixShowNameForPlex(showName).Replace(" ", ".");
            string seasonPart = $"S{seasonNumber:D2}";

            string episodePart;
            if (additionalEpisodes != null && additionalEpisodes.Count > 0)
            {
                var allEpisodes = new List<int> { episodeNumber };
                allEpisodes.AddRange(additionalEpisodes);
                allEpisodes = allEpisodes.Distinct().OrderBy(x => x).ToList();

                if (allEpisodes.Count == 2 && allEpisodes[1] - allEpisodes[0] == 1)
                {
                    episodePart = $"E{allEpisodes[0]:D2}-E{allEpisodes[1]:D2}";
                }
                else
                {
                    episodePart = string.Join("", allEpisodes.Select(ep => $"E{ep:D2}"));
                }
            }
            else
            {
                episodePart = $"E{episodeNumber:D2}";
            }

            return $"{cleanShowName}.{seasonPart}{episodePart}{extension}";
        }

        private static bool IsValidPlexTVEpisodeFormat(string fileName)
        {
            var patterns = new[]
            {
                @"^.+\.S\d{1,2}E\d{1,2}.*\.\w{2,4}$",
                @"^.+\.S\d{1,2}E\d{1,2}-E\d{1,2}.*\.\w{2,4}$",
                @"^.+\.S\d{1,2}E\d{1,2}E\d{1,2}.*\.\w{2,4}$",
            };

            return patterns.Any(pattern => Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase));
        }

        private static bool IsProblematicShowName(string showName)
        {
            var problematicPatterns = new[]
            {
                @"^\d{4}$",
                @"^(Season|Episode|EP|S\d|E\d)",
                @"(HDTV|720p|1080p|WEB-DL|BluRay)",
                @"^(The\s+)*Unknown",
            };

            return problematicPatterns.Any(pattern =>
                Regex.IsMatch(showName, pattern, RegexOptions.IgnoreCase));
        }

        private static bool HasEncodingIssues(string text)
        {
            return text.Any(c => c > 127 && c < 160) ||
                   text.Contains("?") ||
                   Regex.IsMatch(text, @"[^\x00-\x7F]+");
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
            return issues.All(issue =>
                issue.Contains("invalid characters") ||
                issue.Contains("naming convention") ||
                issue.Contains("encoding issues"));
        }
    }

    public class PlexValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public PlexValidationAction SuggestedAction { get; set; }
        public bool CanAutoFix { get; set; }
    }

    public enum PlexValidationAction
    {
        ProcessNormally,
        ShowWarnings,
        PromptUser,
        SkipInAutoMode,
        ProcessWithWarnings
    }
}