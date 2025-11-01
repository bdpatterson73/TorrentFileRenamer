using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TorrentFileRenamer
{
    /// <summary>
    /// Utility class for scan summary operations that were previously in Form1.cs
    /// </summary>
    public static class ScanSummaryUtilities
    {
        /// <summary>
        /// Shows a summary dialog for TV episode scan results
        /// </summary>
        /// <param name="totalVideoFiles">Total video files found</param>
        /// <param name="validEpisodes">Valid episodes parsed</param>
        /// <param name="unparsedFiles">Files that couldn't be parsed</param>
        /// <param name="likelyMoviesSkipped">Movies that were skipped</param>
        public static async Task ShowTVScanSummary(int totalVideoFiles, int validEpisodes, int unparsedFiles, int likelyMoviesSkipped)
        {
            await Task.Delay(100); // Brief delay for UI responsiveness
            
            string summary = $"TV Scan Results:\n" +
                           $"Total video files: {totalVideoFiles}\n" +
                           $"Valid TV episodes: {validEpisodes}\n" +
                           $"Unparsed files: {unparsedFiles}\n" +
                           $"Movies skipped: {likelyMoviesSkipped}";
            
            if (unparsedFiles > 0)
            {
                summary += "\n\nTip: Review unparsed files (gray) and remove them if they're not TV episodes.";
            }
            
            //TODO MessageBox.Show(summary, "TV Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows a summary dialog for movie scan results
        /// </summary>
        /// <param name="totalVideoFiles">Total video files found</param>
        /// <param name="validMovies">Valid movies parsed</param>
        /// <param name="unparsedFiles">Files that couldn't be parsed</param>
        /// <param name="likelyTVShowsSkipped">TV shows that were skipped</param>
        public static async Task ShowMovieScanSummary(int totalVideoFiles, int validMovies, int unparsedFiles, int likelyTVShowsSkipped)
        {
            await Task.Delay(100); // Brief delay for UI responsiveness
            
            string summary = $"Movie Scan Results:\n" +
                           $"Total video files: {totalVideoFiles}\n" +
                           $"Valid movies: {validMovies}\n" +
                           $"Unparsed files: {unparsedFiles}\n" +
                           $"TV shows skipped: {likelyTVShowsSkipped}";
            
            if (unparsedFiles > 0)
            {
                summary += "\n\nTip: Review unparsed files (gray) and remove them if they're not movies.";
            }
            
           //TODO  MessageBox.Show(summary, "Movie Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows a processing completion summary for TV episodes
        /// </summary>
        /// <param name="totalItems">Total items processed</param>
        /// <param name="successfulCopies">Number of successful copies</param>
        /// <param name="successfulDeletions">Number of successful deletions</param>
        /// <param name="errors">Number of errors encountered</param>
        /// <param name="duration">Total processing duration</param>
        public static void ShowTVProcessingCompleteSummary(int totalItems, int successfulCopies, int successfulDeletions, int errors, TimeSpan duration)
        {
            string summary = $"TV Episode Processing Complete!\n\n" +
                           $"Files processed: {totalItems}\n" +
                           $"Successfully copied: {successfulCopies}\n" +
                           $"Original files deleted: {successfulDeletions}\n" +
                           $"Errors: {errors}\n" +
                           $"Duration: {duration.Minutes}m {duration.Seconds}s";
                           
            //MessageBox.Show(summary, "TV Processing Complete", MessageBoxButtons.OK, errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows a processing completion summary for movies
        /// </summary>
        /// <param name="totalItems">Total items processed</param>
        /// <param name="successfulCopies">Number of successful copies</param>
        /// <param name="successfulDeletions">Number of successful deletions</param>
        /// <param name="errors">Number of errors encountered</param>
        /// <param name="duration">Total processing duration</param>
        public static void ShowMovieProcessingCompleteSummary(int totalItems, int successfulCopies, int successfulDeletions, int errors, TimeSpan duration)
        {
            string summary = $"Movie Processing Complete!\n\n" +
                           $"Movies processed: {totalItems}\n" +
                           $"Successfully copied: {successfulCopies}\n" +
                           $"Original files deleted: {successfulDeletions}\n" +
                           $"Errors: {errors}\n" +
                           $"Duration: {duration.Minutes}m {duration.Seconds}s";
                           
            MessageBox.Show(summary, "Movie Processing Complete", 
                MessageBoxButtons.OK, 
                errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
    }
}