using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Utility class to help distinguish between TV episodes and movies
    /// </summary>
    public static class MediaTypeDetector
    {
        /// <summary>
        /// Determines if a filename is likely a TV episode based on patterns
        /// </summary>
        public static bool IsLikelyTVEpisode(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            string upperFilename = filename.ToUpper();

            // Strong TV episode indicators
            string[] tvPatterns = {
                @"S\d{1,2}\s*E\d{1,2}",           // S01E01, S1E1, etc.
                @"\d{1,2}[xX]\d{1,2}",            // 1x01, 12x05, etc.
                @"SEASON\s*\d{1,2}\s*EPISODE\s*\d{1,2}", // Season 1 Episode 1
                @"EP\d{1,2}",                     // EP01, EP1
                @"EPISODE\s*\d{1,2}",             // Episode 01
                @"S\d{1,2}",                      // S01 (season only)
                @"\d{4}\.\d{2}\.\d{2}",           // Date format episodes (2023.01.15)
            };

            foreach (string pattern in tvPatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, pattern))
                    return true;
            }

            // Check for common TV show structures in path
            string[] pathTvIndicators = {
                "SEASON", "EPISODE", "EPISODES", "SERIES", "TV", "SHOW"
            };

            foreach (string indicator in pathTvIndicators)
            {
                if (upperFilename.Contains(indicator))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a filename is likely a movie based on patterns and exclusions
        /// </summary>
        public static bool IsLikelyMovie(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            // If it looks like a TV episode, it's probably not a movie
            if (IsLikelyTVEpisode(filename))
                return false;

            string upperFilename = filename.ToUpper();

            // Movie indicators (years, movie-specific terms)
            string[] moviePatterns = {
                @"\(\d{4}\)",                     // (2021)
                @"\[\d{4}\]",                     // [2021]
                @"\b(19|20)\d{2}\b",              // 1999, 2021 (standalone year)
            };

            foreach (string pattern in moviePatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, pattern))
                    return true;
            }

            // Movie-specific terms that rarely appear in TV shows
            string[] movieIndicators = {
                "DVDRIP", "BDRIP", "WEBRIP", "CAMRIP", "HDCAM", "HDTS",
                "THEATRICAL", "EXTENDED", "DIRECTORS.CUT", "DC",
                "UNRATED", "REMASTERED", "CRITERION", "LIMITED",
                "IMAX", "3D"
            };

            foreach (string indicator in movieIndicators)
            {
                if (upperFilename.Contains(indicator))
                    return true;
            }

            // If filename is very short and has a year, likely a movie
            string filenameOnly = System.IO.Path.GetFileNameWithoutExtension(filename);
            if (filenameOnly.Length < 50 && System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"\b(19|20)\d{2}\b"))
                return true;

            return false;
        }

        /// <summary>
        /// Gets a confidence score for TV episode classification (0-100)
        /// </summary>
        public static int GetTVEpisodeConfidence(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return 0;

            int confidence = 0;
            string upperFilename = filename.ToUpper();

            // High confidence patterns
            if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"S\d{1,2}\s*E\d{1,2}"))
                confidence += 90;
            else if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"\d{1,2}[xX]\d{1,2}"))
                confidence += 85;
            else if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"SEASON\s*\d{1,2}\s*EPISODE\s*\d{1,2}"))
                confidence += 80;
            else if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"EP\d{1,2}"))
                confidence += 60;
            else if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"EPISODE\s*\d{1,2}"))
                confidence += 70;

            // Medium confidence indicators
            if (upperFilename.Contains("SEASON"))
                confidence += 30;
            if (upperFilename.Contains("EPISODE"))
                confidence += 25;
            if (upperFilename.Contains("SERIES"))
                confidence += 20;

            // Negative indicators (reduce confidence)
            if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"\(\d{4}\)"))
                confidence -= 20;
            if (upperFilename.Contains("THEATRICAL") || upperFilename.Contains("DIRECTORS.CUT"))
                confidence -= 30;

            return Math.Max(0, Math.Min(100, confidence));
        }

        /// <summary>
        /// Gets a confidence score for movie classification (0-100)
        /// </summary>
        public static int GetMovieConfidence(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return 0;

            // If it has high TV confidence, movie confidence should be low
            int tvConfidence = GetTVEpisodeConfidence(filename);
            if (tvConfidence > 70)
                return Math.Max(0, 20 - tvConfidence);

            int confidence = 0;
            string upperFilename = filename.ToUpper();

            // Year indicators
            if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"\(\d{4}\)"))
                confidence += 40;
            else if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"\[\d{4}\]"))
                confidence += 35;
            else if (System.Text.RegularExpressions.Regex.IsMatch(upperFilename, @"\b(19|20)\d{2}\b"))
                confidence += 25;

            // Movie-specific terms
            string[] movieTerms = { "THEATRICAL", "EXTENDED", "DIRECTORS.CUT", "UNRATED", "IMAX", "3D", "REMASTERED" };
            foreach (string term in movieTerms)
            {
                if (upperFilename.Contains(term))
                    confidence += 20;
            }

            // File structure hints
            string filenameOnly = System.IO.Path.GetFileNameWithoutExtension(filename);
            if (filenameOnly.Length < 50) // Shorter names often indicate movies
                confidence += 10;

            return Math.Max(0, Math.Min(100, confidence));
        }
    }
}