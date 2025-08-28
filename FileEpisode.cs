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
            // Pattern for SxxExx format with optional multi-episode support
            // Supports: S01E03, S1E3, S01E03-04, S01E03E04, S01E03-E04
            Regex regex = new Regex(@"S(?<season>\d{1,2})\s*E(?<episode>\d{1,2})(?:(?:[-\s]*E?(?<episode2>\d{1,2}))?(?:[-\s]*E?(?<episode3>\d{1,2}))?)?", RegexOptions.IgnoreCase);

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

                    // Extract show name - look for the pattern and take everything before it
                    string seasonPattern = $"S{SeasonNumber:D2}";
                    int matchIndex = Filename.ToUpper().IndexOf(seasonPattern);
                    if (matchIndex == -1)
                    {
                        seasonPattern = $"S{SeasonNumber}";
                        matchIndex = Filename.ToUpper().IndexOf(seasonPattern);
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
                    Debug.WriteLine($"Error parsing SxxExx format: {e.Message}");
                }
            }
            return false;
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
                    var sortedEpisodes = EpisodeNumbers.OrderBy(x => x).ToList();
                    if (sortedEpisodes.Count == 2 && sortedEpisodes[1] - sortedEpisodes[0] == 1)
                    {
                        // Consecutive episodes: S01E03-E04
                        episodeString = $"E{sortedEpisodes[0]:D2}-E{sortedEpisodes[1]:D2}";
                    }
                    else
                    {
                        // Non-consecutive or more than 2 episodes: S01E03E05E07
                        episodeString = string.Join("", sortedEpisodes.Select(ep => $"E{ep:D2}"));
                    }
                }
                else
                {
                    episodeString = $"E{EpisodeNumber:D2}";
                }

                NewFileName = $"{ShowName}.S{SeasonNumber:D2}{episodeString}{Extension}";
                NewFileName = NewFileName.Replace(" ", ".");
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
                "HDTV", "WEB-DL", "WEBRip", "BluRay", "BRRip", "DVDRip",
                "x264", "x265", "H264", "H265", "HEVC",
                "AAC", "AC3", "DTS", "MP3",
                "PROPER", "REPACK", "EXTENDED", "UNRATED", "DIRECTORS.CUT"
            };
            
            string cleanName = name;
            foreach (string artifact in artifactsToRemove)
            {
                cleanName = Regex.Replace(cleanName, $@"\b{Regex.Escape(artifact)}\b", "", RegexOptions.IgnoreCase);
            }
            
            // Clean up extra spaces
            cleanName = Regex.Replace(cleanName, @"\s+", " ").Trim();
            
            return cleanName;
        }
    }
}
