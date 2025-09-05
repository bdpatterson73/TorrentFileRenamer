using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TorrentFileRenamer
{
    internal class FileEpisode
    {
        private string _fullFilePath;
        public FileEpisode(string fullFilePath, string outputDirectory)
        {
            _fullFilePath = fullFilePath.ToUpper();
            OutputDirectory= outputDirectory;
            DirectoryName = Path.GetDirectoryName(_fullFilePath).Trim();
            Extension = Path.GetExtension(_fullFilePath).ToUpper().Trim();
            Filename = _fullFilePath.Replace(DirectoryName, "").Replace("\\", "");
            
            // Initialize collections for multi-episode support
            EpisodeNumbers = new List<int>();
            
            ParseFileDetails();
            //NewFileNamePath = DirectoryName +"\\" + NewFileName;
            NewFileNamePath = OutputDirectory + "\\" + ShowName + "\\" + "Season " + SeasonNumber.ToString() + "\\" + NewFileName;
            NewDirectoryName = OutputDirectory + "\\" + ShowName + "\\" + "Season " + SeasonNumber.ToString() + "\\";
        }
        
        public string FullFilePath
        {
            get { return _fullFilePath; }
        }

        public string OutputDirectory { get; set; }
        public string Extension { get; set; }
        public string Filename { get; set; }

        public string NewDirectoryName { get; set; }
        public string DirectoryName { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }

        // New properties for multi-episode support
        public List<int> EpisodeNumbers { get; set; }
        public bool IsMultiEpisode => EpisodeNumbers.Count > 1;

        public string ShowName { get; set; }

        public string NewFileName { get; set; }

        public string NewFileNamePath { get; set; }
        
        private void ParseFileDetails()
        {
            Debug.WriteLine($"Parsing file: {Filename}");
            
            // Try multiple regex patterns in order of preference
            if (TryParseSxxExxFormat()) 
            {
                Debug.WriteLine($"Successfully parsed with SxxExx format: Show='{ShowName}', Season={SeasonNumber}, Episodes=[{string.Join(",", EpisodeNumbers)}]");
                GenerateNewFileName();
            }
            else if (TryParseNumberxNumberFormat())
            {
                Debug.WriteLine($"Successfully parsed with xxYYY format: Show='{ShowName}', Season={SeasonNumber}, Episodes=[{string.Join(",", EpisodeNumbers)}]");
                GenerateNewFileName();
            }
            else if (TryParseSeasonEpisodeWordsFormat())
            {
                Debug.WriteLine($"Successfully parsed with Season Episode format: Show='{ShowName}', Season={SeasonNumber}, Episodes=[{string.Join(",", EpisodeNumbers)}]");
                GenerateNewFileName();
            }
            else if (TryParseEpisodeOnlyFormat())
            {
                Debug.WriteLine($"Successfully parsed with Episode-only format: Show='{ShowName}', Season={SeasonNumber}, Episodes=[{string.Join(",", EpisodeNumbers)}]");
                GenerateNewFileName();
            }
            else
            {
                Debug.WriteLine($"No match found for file: {Filename}");
                NewFileName = "NO NAME";
            }
        }

        /// <summary>
        /// Parse SxxExx format (e.g., S01E03, S01E03-04, S1E3)
        /// </summary>
        private bool TryParseSxxExxFormat()
        {
            // Find the main SxxExx pattern - be very specific about what constitutes valid episode info
            Regex mainPattern = new Regex(@"S(?<season>\d{1,2})E(?<episode>\d{1,2})", RegexOptions.IgnoreCase);
            Match mainMatch = mainPattern.Match(Filename);
            
            if (!mainMatch.Success)
                return false;

            try
            {
                SeasonNumber = Convert.ToInt32(mainMatch.Groups["season"].Value);
                int firstEpisode = Convert.ToInt32(mainMatch.Groups["episode"].Value);
                
                // Clear any previous episode numbers and start fresh
                EpisodeNumbers.Clear();
                
                // Set the primary episode number - allow episode 0
                EpisodeNumber = firstEpisode; // Allow episode 0 for special episodes
                EpisodeNumbers.Add(EpisodeNumber);

                // Look for additional episodes ONLY in very specific patterns immediately following
                // We need to be much more restrictive to avoid picking up quality info like "720p"
                string textAfterMatch = Filename.Substring(mainMatch.Index + mainMatch.Length);
                
                // Only look for multi-episode patterns that are clearly intentional:
                // - S01E01E02 (no spaces/separators between)
                // - S01E01-E02 (clear separator with E prefix)
                // - S01E01-02 (clear separator)
                // Do NOT match random numbers that appear later in quality info
                
                Regex strictMultiEpisodePattern = new Regex(@"^(?:(?:E(?<episode>\d{1,2}))|(?:-E(?<episode>\d{1,2}))|(?:-(?<episode>\d{1,2})))(?=\s|$|[^0-9])", RegexOptions.IgnoreCase);
                Match multiMatch = strictMultiEpisodePattern.Match(textAfterMatch);
                
                if (multiMatch.Success)
                {
                    var episodeMatches = Regex.Matches(multiMatch.Value, @"(?<episode>\d{1,2})", RegexOptions.IgnoreCase);
                    
                    foreach (Match epMatch in episodeMatches)
                    {
                        int epNum = Convert.ToInt32(epMatch.Groups["episode"].Value);
                        
                        // Only add if it's a reasonable episode number and consecutive or very close
                        // Allow episode 0 for special episodes
                        if (epNum >= 0 && epNum <= 50 && !EpisodeNumbers.Contains(epNum))
                        {
                            // Additional validation: episode should be close to the first episode
                            if (Math.Abs(epNum - firstEpisode) <= 5) // Within 5 episodes
                            {
                                EpisodeNumbers.Add(epNum);
                            }
                        }
                    }
                }

                // Extract show name - everything before the SxxExx pattern
                int matchIndex = mainMatch.Index;
                if (matchIndex > 0)
                {
                    ShowName = Filename.Substring(0, matchIndex).Replace(".", " ").Replace("-", " ").Replace("_", " ").Trim();
                    ShowName = Regex.Replace(ShowName, @"\s+", " "); // Replace multiple spaces with single space
                    
                    // Clean up common artifacts
                    ShowName = CleanShowName(ShowName);
                }
                else
                {
                    ShowName = "Unknown Show";
                }

                Debug.WriteLine($"Parsed SxxExx: Season={SeasonNumber}, Episodes=[{string.Join(",", EpisodeNumbers)}], Show='{ShowName}'");
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error parsing SxxExx format: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Parse xxYYY format (e.g., 1x03, 1x03-04, 12x05)
        /// </summary>
        private bool TryParseNumberxNumberFormat()
        {
            // Pattern for xxYYY format with optional multi-episode support
            // Supports: 1x03, 12x05, 1x03-04, 1x03-1x04
            Regex regex = new Regex(@"(?<season>\d{1,2})\s*[xX]\s*(?<episode>\d{1,2})(?:[-\s]*(?:\d{1,2}[xX])?(?<episode2>\d{1,2}))?(?:[-\s]*(?:\d{1,2}[xX])?(?<episode3>\d{1,2}))?", RegexOptions.IgnoreCase);

            Match match = regex.Match(Filename);
            if (match.Success)
            {
                try
                {
                    SeasonNumber = Convert.ToInt32(match.Groups["season"].Value);
                    EpisodeNumber = Convert.ToInt32(match.Groups["episode"].Value);
                    EpisodeNumbers.Add(EpisodeNumber);

                    // Check for additional episodes
                    if (match.Groups["episode2"].Success && !string.IsNullOrEmpty(match.Groups["episode2"].Value))
                    {
                        int ep2 = Convert.ToInt32(match.Groups["episode2"].Value);
                        if (!EpisodeNumbers.Contains(ep2))
                            EpisodeNumbers.Add(ep2);
                    }
                    if (match.Groups["episode3"].Success && !string.IsNullOrEmpty(match.Groups["episode3"].Value))
                    {
                        int ep3 = Convert.ToInt32(match.Groups["episode3"].Value);
                        if (!EpisodeNumbers.Contains(ep3))
                            EpisodeNumbers.Add(ep3);
                    }

                    // Extract show name
                    string seasonPattern = $"{SeasonNumber}X";
                    int matchIndex = Filename.ToUpper().IndexOf(seasonPattern);
                    if (matchIndex == -1)
                    {
                        seasonPattern = $"{SeasonNumber}x";
                        matchIndex = Filename.IndexOf(seasonPattern, StringComparison.OrdinalIgnoreCase);
                    }
                    
                    if (matchIndex > 0)
                    {
                        ShowName = Filename.Substring(0, matchIndex).Replace(".", " ").Replace("-", " ").Replace("_", " ").Trim();
                        ShowName = Regex.Replace(ShowName, @"\s+", " "); // Replace multiple spaces with single space
                        
                        // Clean up common artifacts
                        ShowName = CleanShowName(ShowName);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error parsing xxYYY format: {e.Message}");
                }
            }
            return false;
        }

        /// <summary>
        /// Parse formats with "Season" and "Episode" words
        /// </summary>
        private bool TryParseSeasonEpisodeWordsFormat()
        {
            // Pattern for Season X Episode Y format
            Regex regex = new Regex(@"Season\s*(?<season>\d{1,2})\s*Episode\s*(?<episode>\d{1,2})(?:[-\s]*(?<episode2>\d{1,2}))?", RegexOptions.IgnoreCase);

            Match match = regex.Match(Filename);
            if (match.Success)
            {
                try
                {
                    SeasonNumber = Convert.ToInt32(match.Groups["season"].Value);
                    EpisodeNumber = Convert.ToInt32(match.Groups["episode"].Value);
                    EpisodeNumbers.Add(EpisodeNumber);

                    if (match.Groups["episode2"].Success && !string.IsNullOrEmpty(match.Groups["episode2"].Value))
                    {
                        int ep2 = Convert.ToInt32(match.Groups["episode2"].Value);
                        if (!EpisodeNumbers.Contains(ep2))
                            EpisodeNumbers.Add(ep2);
                    }

                    // Extract show name
                    int matchIndex = Filename.ToUpper().IndexOf("SEASON");
                    if (matchIndex > 0)
                    {
                        ShowName = Filename.Substring(0, matchIndex).Replace(".", " ").Replace("-", " ").Replace("_", " ").Trim();
                        ShowName = Regex.Replace(ShowName, @"\s+", " ");
                        ShowName = CleanShowName(ShowName);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error parsing Season Episode format: {e.Message}");
                }
            }
            return false;
        }

        /// <summary>
        /// Try to parse episode-only formats (fallback when season info might be in folder name)
        /// </summary>
        private bool TryParseEpisodeOnlyFormat()
        {
            // This is a fallback method that tries to extract episode info when season might be unclear
            // Pattern for Episode XX or Ep XX
            Regex regex = new Regex(@"(?:Episode|Ep)\s*(?<episode>\d{1,2})(?:[-\s]*(?<episode2>\d{1,2}))?", RegexOptions.IgnoreCase);

            Match match = regex.Match(Filename);
            if (match.Success)
            {
                try
                {
                    // Try to extract season from directory path or set default to 1
                    SeasonNumber = ExtractSeasonFromDirectory() ?? 1;
                    
                    EpisodeNumber = Convert.ToInt32(match.Groups["episode"].Value);
                    EpisodeNumbers.Add(EpisodeNumber);

                    if (match.Groups["episode2"].Success && !string.IsNullOrEmpty(match.Groups["episode2"].Value))
                    {
                        int ep2 = Convert.ToInt32(match.Groups["episode2"].Value);
                        if (!EpisodeNumbers.Contains(ep2))
                            EpisodeNumbers.Add(ep2);
                    }

                    // Extract show name
                    int matchIndex = Filename.ToUpper().IndexOf("EPISODE");
                    if (matchIndex == -1)
                        matchIndex = Filename.ToUpper().IndexOf("EP");
                        
                    if (matchIndex > 0)
                    {
                        ShowName = Filename.Substring(0, matchIndex).Replace(".", " ").Replace("-", " ").Replace("_", " ").Trim();
                        ShowName = Regex.Replace(ShowName, @"\s+", " ");
                        ShowName = CleanShowName(ShowName);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error parsing episode-only format: {e.Message}");
                }
            }
            return false;
        }

        /// <summary>
        /// Try to extract season number from directory path
        /// </summary>
        private int? ExtractSeasonFromDirectory()
        {
            try
            {
                Regex seasonRegex = new Regex(@"Season\s*(\d{1,2})", RegexOptions.IgnoreCase);
                Match match = seasonRegex.Match(DirectoryName);
                if (match.Success)
                {
                    return Convert.ToInt32(match.Groups[1].Value);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error extracting season from directory: {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Generate the new filename based on parsed information
        /// </summary>
        private void GenerateNewFileName()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ShowName))
                {
                    ShowName = "Unknown Show";
                }

                string episodeString;
                if (IsMultiEpisode)
                {
                    // Handle multi-episode files: S01E03-E04 or S01E03E04
                    var validEpisodes = EpisodeNumbers.Where(ep => ep >= 0).OrderBy(x => x).ToList(); // Allow episode 0
                    
                    if (validEpisodes.Count >= 2)
                    {
                        if (validEpisodes.Count == 2 && validEpisodes[1] - validEpisodes[0] == 1)
                        {
                            // Consecutive episodes: S01E03-E04
                            episodeString = $"E{validEpisodes[0]:D2}-E{validEpisodes[1]:D2}";
                        }
                        else
                        {
                            // Non-consecutive or more than 2 episodes: S01E03E05E07
                            episodeString = string.Join("", validEpisodes.Select(ep => $"E{ep:D2}"));
                        }
                    }
                    else if (validEpisodes.Count == 1)
                    {
                        // Only one valid episode found, treat as single episode
                        EpisodeNumber = validEpisodes[0];
                        episodeString = $"E{EpisodeNumber:D2}";
                    }
                    else
                    {
                        // No valid episodes found, use the primary episode number (allow 0)
                        episodeString = $"E{EpisodeNumber:D2}";
                    }
                }
                else
                {
                    // Single episode - allow episode 0 for special episodes
                    episodeString = $"E{EpisodeNumber:D2}";
                }

                NewFileName = $"{ShowName}.S{SeasonNumber:D2}{episodeString}{Extension}";
                NewFileName = NewFileName.Replace(" ", ".");
                
                Debug.WriteLine($"Generated filename: {NewFileName}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error generating new filename: {e.Message}");
                NewFileName = "NO NAME";
            }
        }

        /// <summary>
        /// Clean up show names by removing common artifacts
        /// </summary>
        private string CleanShowName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;
                
            // Remove common quality indicators and other artifacts
            string[] artifactsToRemove = {
                "720P", "1080P", "480P", "4K", "UHD", "HDR",
                "HDTV", "WEB-DL", "WEBDL", "WEBRip", "BluRay", "BRRip", "DVDRip",
                "x264", "x265", "H264", "H265", "HEVC", "AVC",
                "AAC", "AC3", "DTS", "MP3", "FLAC",
                "PROPER", "REPACK", "EXTENDED", "UNRATED", "DIRECTORS.CUT",
                "BONE", "RARBG", "YTS", "YIFY", "ETRG", "ION10", "PSA",
                "AMZN", "HULU", "DSNP", "HMAX", "NFLX", "ATVP", "PCOK"
            };
            
            string cleanName = name;
            
            // Remove artifacts first
            foreach (string artifact in artifactsToRemove)
            {
                cleanName = Regex.Replace(cleanName, $@"\b{Regex.Escape(artifact)}\b", "", RegexOptions.IgnoreCase);
            }
            
            // Remove years (4 digit numbers that look like years, typically 1900-2099)
            cleanName = Regex.Replace(cleanName, @"\b(19|20)\d{2}\b", "", RegexOptions.IgnoreCase);
            
            // Remove resolution indicators (numbers followed by 'p' like 720p, 1080p)
            cleanName = Regex.Replace(cleanName, @"\b\d{3,4}p\b", "", RegexOptions.IgnoreCase);
            
            // Remove standalone large numbers that are likely quality indicators
            cleanName = Regex.Replace(cleanName, @"\b\d{3,}\b", "", RegexOptions.IgnoreCase);
            
            // Remove common separators and clean up
            cleanName = Regex.Replace(cleanName, @"[._-]+", " ");
            cleanName = Regex.Replace(cleanName, @"\s+", " ").Trim();
            
            // Remove leading/trailing punctuation
            cleanName = cleanName.Trim('.', '-', '_', ' ', '[', ']', '(', ')');
            
            return cleanName;
        }

        /// <summary>
        /// Test parsing logic for debugging - can be removed in production
        /// </summary>
        public static void TestFilenameParsing(string testFilename, string outputDir)
        {
            try
            {
                FileEpisode testEpisode = new FileEpisode(testFilename, outputDir);
                
                Debug.WriteLine($"=== PARSING TEST ===");
                Debug.WriteLine($"Original: {testFilename}");
                Debug.WriteLine($"Show Name: '{testEpisode.ShowName}'");
                Debug.WriteLine($"Season: {testEpisode.SeasonNumber}");
                Debug.WriteLine($"Episode: {testEpisode.EpisodeNumber}");
                Debug.WriteLine($"All Episodes: [{string.Join(", ", testEpisode.EpisodeNumbers)}]");
                Debug.WriteLine($"Is Multi-Episode: {testEpisode.IsMultiEpisode}");
                Debug.WriteLine($"New Filename: {testEpisode.NewFileName}");
                Debug.WriteLine($"Full Path: {testEpisode.NewFileNamePath}");
                Debug.WriteLine($"Episode 0 Support: {(testEpisode.EpisodeNumber == 0 ? "✅ Working" : "N/A")}");
                Debug.WriteLine($"===================");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test parsing error: {ex.Message}");
            }
        }
    }
}
