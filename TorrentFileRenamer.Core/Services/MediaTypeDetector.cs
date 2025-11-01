using System.Text.RegularExpressions;

namespace TorrentFileRenamer.Core.Services
{
    /// <summary>
    /// Utility class to help distinguish between TV episodes and movies
    /// </summary>
    public static class MediaTypeDetector
    {
        public static bool IsLikelyTVEpisode(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
     return false;

   string upperFilename = filename.ToUpper();

         string[] tvPatterns = {
            @"S\d{1,2}\s*E\d{1,2}",
      @"\d{1,2}[xX]\d{1,2}",
      @"SEASON\s*\d{1,2}\s*EPISODE\s*\d{1,2}",
 @"EP\d{1,2}",
  @"EPISODE\s*\d{1,2}",
                @"S\d{1,2}",
      @"\d{4}\.\d{2}\.\d{2}",
      };

        foreach (string pattern in tvPatterns)
      {
                if (Regex.IsMatch(upperFilename, pattern))
           return true;
            }

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

public static bool IsLikelyMovie(string filename)
        {
         if (string.IsNullOrWhiteSpace(filename))
          return false;

            if (IsLikelyTVEpisode(filename))
                return false;

  string upperFilename = filename.ToUpper();

         string[] moviePatterns = {
              @"\(\d{4}\)",
     @"\[\d{4}\]",
        @"\b(19|20)\d{2}\b",
            };

        foreach (string pattern in moviePatterns)
 {
                if (Regex.IsMatch(upperFilename, pattern))
             return true;
            }

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

            string filenameOnly = Path.GetFileNameWithoutExtension(filename);
 if (filenameOnly.Length < 50 && Regex.IsMatch(upperFilename, @"\b(19|20)\d{2}\b"))
return true;

     return false;
        }

        public static int GetTVEpisodeConfidence(string filename)
     {
 if (string.IsNullOrWhiteSpace(filename))
         return 0;

      int confidence = 0;
  string upperFilename = filename.ToUpper();

    if (Regex.IsMatch(upperFilename, @"S\d{1,2}\s*E\d{1,2}"))
         confidence += 90;
  else if (Regex.IsMatch(upperFilename, @"\d{1,2}[xX]\d{1,2}"))
     confidence += 85;
         else if (Regex.IsMatch(upperFilename, @"SEASON\s*\d{1,2}\s*EPISODE\s*\d{1,2}"))
          confidence += 80;
   else if (Regex.IsMatch(upperFilename, @"EP\d{1,2}"))
        confidence += 60;
            else if (Regex.IsMatch(upperFilename, @"EPISODE\s*\d{1,2}"))
          confidence += 70;

            if (upperFilename.Contains("SEASON"))
         confidence += 30;
    if (upperFilename.Contains("EPISODE"))
             confidence += 25;
  if (upperFilename.Contains("SERIES"))
   confidence += 20;

            if (Regex.IsMatch(upperFilename, @"\(\d{4}\)"))
    confidence -= 20;
            if (upperFilename.Contains("THEATRICAL") || upperFilename.Contains("DIRECTORS.CUT"))
      confidence -= 30;

          return Math.Max(0, Math.Min(100, confidence));
        }

        public static int GetMovieConfidence(string filename)
        {
if (string.IsNullOrWhiteSpace(filename))
          return 0;

int tvConfidence = GetTVEpisodeConfidence(filename);
         if (tvConfidence > 70)
      return Math.Max(0, 20 - tvConfidence);

        int confidence = 0;
   string upperFilename = filename.ToUpper();

   if (Regex.IsMatch(upperFilename, @"\(\d{4}\)"))
         confidence += 40;
   else if (Regex.IsMatch(upperFilename, @"\[\d{4}\]"))
       confidence += 35;
else if (Regex.IsMatch(upperFilename, @"\b(19|20)\d{2}\b"))
     confidence += 25;

 string[] movieTerms = { "THEATRICAL", "EXTENDED", "DIRECTORS.CUT", "UNRATED", "IMAX", "3D", "REMASTERED" };
            foreach (string term in movieTerms)
            {
        if (upperFilename.Contains(term))
    confidence += 20;
      }

       string filenameOnly = Path.GetFileNameWithoutExtension(filename);
            if (filenameOnly.Length < 50)
    confidence += 10;

     return Math.Max(0, Math.Min(100, confidence));
 }
    }
}
