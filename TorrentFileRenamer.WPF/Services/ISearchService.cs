using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Service for searching and filtering media files
/// </summary>
public interface ISearchService
{
    /// <summary>
/// Searches movie files based on criteria
    /// </summary>
    IEnumerable<MovieFileModel> SearchMovies(
        IEnumerable<MovieFileModel> movies,
        SearchCriteria criteria);

    /// <summary>
    /// Searches TV episode files based on criteria
    /// </summary>
    IEnumerable<FileEpisodeModel> SearchEpisodes(
        IEnumerable<FileEpisodeModel> episodes,
        SearchCriteria criteria);

  /// <summary>
    /// Calculates statistics for a collection of movie files
    /// </summary>
    FileStatistics CalculateMovieStatistics(IEnumerable<MovieFileModel> movies);

    /// <summary>
    /// Calculates statistics for a collection of TV episode files
    /// </summary>
    FileStatistics CalculateEpisodeStatistics(IEnumerable<FileEpisodeModel> episodes);

    /// <summary>
    /// Gets all unique file extensions from movie files
    /// </summary>
    List<string> GetMovieExtensions(IEnumerable<MovieFileModel> movies);

    /// <summary>
    /// Gets all unique file extensions from TV episode files
  /// </summary>
    List<string> GetEpisodeExtensions(IEnumerable<FileEpisodeModel> episodes);

    /// <summary>
 /// Highlights search text within a string
    /// </summary>
    string HighlightSearchText(string text, string searchText);
}
