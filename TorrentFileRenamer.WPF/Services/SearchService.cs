using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Implementation of search and filtering service
/// </summary>
public class SearchService : ISearchService
{
    /// <inheritdoc/>
    public IEnumerable<MovieFileModel> SearchMovies(
        IEnumerable<MovieFileModel> movies,
        SearchCriteria criteria)
    {
        if (movies == null) return Enumerable.Empty<MovieFileModel>();

        var query = movies.AsEnumerable();

        // Text search
        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            var searchText = criteria.SearchText.ToLowerInvariant();
            query = query.Where(m =>
                (criteria.SearchInFileName && m.FileName.ToLowerInvariant().Contains(searchText)) ||
                (criteria.SearchInMovieName && m.MovieName.ToLowerInvariant().Contains(searchText)) ||
                (criteria.SearchInYear && m.MovieYear.ToLowerInvariant().Contains(searchText))
            );
        }

        // Confidence filter
        if (criteria.MinConfidence > 0 || criteria.MaxConfidence < 100)
        {
            query = query.Where(m =>
                m.Confidence >= criteria.MinConfidence &&
                m.Confidence <= criteria.MaxConfidence
            );
        }

        // File size filter
        if (criteria.MinFileSize > 0 || criteria.MaxFileSize < long.MaxValue)
        {
            query = query.Where(m =>
                m.FileSize >= criteria.MinFileSize &&
                m.FileSize <= criteria.MaxFileSize
            );
        }

        // Extension filter
        if (criteria.SelectedExtensions.Count > 0)
        {
            query = query.Where(m =>
                criteria.SelectedExtensions.Contains(m.Extension, StringComparer.OrdinalIgnoreCase)
            );
        }

        // Status filter
        if (criteria.SelectedStatuses.Count > 0)
        {
            query = query.Where(m => criteria.SelectedStatuses.Contains(m.Status));
        }

        return query;
    }

    /// <inheritdoc/>
    public IEnumerable<FileEpisodeModel> SearchEpisodes(
        IEnumerable<FileEpisodeModel> episodes,
        SearchCriteria criteria)
    {
        if (episodes == null) return Enumerable.Empty<FileEpisodeModel>();

        var query = episodes.AsEnumerable();

        // Text search
        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            var searchText = criteria.SearchText.ToLowerInvariant();
            query = query.Where(e =>
                (criteria.SearchInFileName && e.CoreEpisode.Filename.ToLowerInvariant().Contains(searchText)) ||
                (criteria.SearchInShowName && e.ShowName.ToLowerInvariant().Contains(searchText))
            );
        }

        // File size filter
        if (criteria.MinFileSize > 0 || criteria.MaxFileSize < long.MaxValue)
        {
            query = query.Where(e =>
            {
                var fileInfo = new System.IO.FileInfo(e.SourcePath);
                return fileInfo.Exists &&
                       fileInfo.Length >= criteria.MinFileSize &&
                       fileInfo.Length <= criteria.MaxFileSize;
            });
        }

        // Extension filter
        if (criteria.SelectedExtensions.Count > 0)
        {
            query = query.Where(e =>
                criteria.SelectedExtensions.Contains(e.Extension, StringComparer.OrdinalIgnoreCase)
            );
        }

        // Status filter
        if (criteria.SelectedStatuses.Count > 0)
        {
            query = query.Where(e => criteria.SelectedStatuses.Contains(e.Status));
        }

        return query;
    }

    /// <inheritdoc/>
    public FileStatistics CalculateMovieStatistics(IEnumerable<MovieFileModel> movies)
    {
        var movieList = movies?.ToList() ?? new List<MovieFileModel>();

        var stats = new FileStatistics
        {
            TotalFiles = movieList.Count,
            ProcessedFiles = movieList.Count(m => m.Status == ProcessingStatus.Completed),
            PendingFiles = movieList.Count(m => m.Status == ProcessingStatus.Pending),
            ErrorFiles = movieList.Count(m => m.Status == ProcessingStatus.Failed),
            UnprocessedFiles = movieList.Count(m => m.Status == ProcessingStatus.Unparsed),
            AverageConfidence = movieList.Any() ? movieList.Average(m => m.Confidence) : 0,
            TotalFileSize = movieList.Sum(m => m.FileSize),
            LastUpdated = DateTime.Now
        };

        return stats;
    }

    /// <inheritdoc/>
    public FileStatistics CalculateEpisodeStatistics(IEnumerable<FileEpisodeModel> episodes)
    {
        var episodeList = episodes?.ToList() ?? new List<FileEpisodeModel>();

        var stats = new FileStatistics
        {
            TotalFiles = episodeList.Count,
            ProcessedFiles = episodeList.Count(e => e.Status == ProcessingStatus.Completed),
            PendingFiles = episodeList.Count(e => e.Status == ProcessingStatus.Pending),
            ErrorFiles = episodeList.Count(e => e.Status == ProcessingStatus.Failed),
            UnprocessedFiles = episodeList.Count(e => e.Status == ProcessingStatus.Unparsed),
            AverageConfidence = 0, // Episodes don't have confidence scores
            TotalFileSize = episodeList.Sum(e =>
            {
                try
                {
                    var fileInfo = new System.IO.FileInfo(e.SourcePath);
                    return fileInfo.Exists ? fileInfo.Length : 0;
                }
                catch
                {
                    return 0;
                }
            }),
            LastUpdated = DateTime.Now
        };

        return stats;
    }

    /// <inheritdoc/>
    public List<string> GetMovieExtensions(IEnumerable<MovieFileModel> movies)
    {
        return movies?
            .Select(m => m.Extension)
            .Where(ext => !string.IsNullOrEmpty(ext))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(ext => ext)
            .ToList() ?? new List<string>();
    }

    /// <inheritdoc/>
    public List<string> GetEpisodeExtensions(IEnumerable<FileEpisodeModel> episodes)
    {
        return episodes?
            .Select(e => e.Extension)
            .Where(ext => !string.IsNullOrEmpty(ext))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(ext => ext)
            .ToList() ?? new List<string>();
    }

    /// <inheritdoc/>
    public string HighlightSearchText(string text, string searchText)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchText))
            return text;

        // Simple highlighting - can be enhanced with XAML formatting
        var index = text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            return $"{text.Substring(0, index)}**{text.Substring(index, searchText.Length)}**{text.Substring(index + searchText.Length)}";
        }

        return text;
    }
}