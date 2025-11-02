using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Implementation of export service
/// </summary>
public class ExportService : IExportService
{
    /// <inheritdoc/>
    public async Task<bool> ExportMoviesAsync(
        IEnumerable<MovieFileModel> movies,
        ExportOptions options,
        IProgress<int>? progress = null)
    {
        try
        {
            var movieList = movies.ToList();

            switch (options.Format)
            {
                case ExportFormat.Csv:
                    return await ExportMoviesToCsvAsync(movieList, options, progress);
                case ExportFormat.Json:
                    return await ExportMoviesToJsonAsync(movieList, options, progress);
                case ExportFormat.Xml:
                    return await ExportMoviesToXmlAsync(movieList, options, progress);
                case ExportFormat.Excel:
                    // Excel export requires additional library (e.g., EPPlus)
                    // For now, export as CSV with notice
                    return await ExportMoviesToCsvAsync(movieList, options, progress);
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExportEpisodesAsync(
        IEnumerable<FileEpisodeModel> episodes,
        ExportOptions options,
        IProgress<int>? progress = null)
    {
        try
        {
            var episodeList = episodes.ToList();

            switch (options.Format)
            {
                case ExportFormat.Csv:
                    return await ExportEpisodesToCsvAsync(episodeList, options, progress);
                case ExportFormat.Json:
                    return await ExportEpisodesToJsonAsync(episodeList, options, progress);
                case ExportFormat.Xml:
                    return await ExportEpisodesToXmlAsync(episodeList, options, progress);
                case ExportFormat.Excel:
                    return await ExportEpisodesToCsvAsync(episodeList, options, progress);
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string> GenerateMovieSummaryAsync(
        IEnumerable<MovieFileModel> movies,
        FileStatistics statistics)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Movie Files Export Summary");
        sb.AppendLine("=========================");
        sb.AppendLine();
        sb.AppendLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("Statistics:");
        sb.AppendLine($"  Total Files: {statistics.TotalFiles}");
        sb.AppendLine($"  Processed: {statistics.ProcessedFiles}");
        sb.AppendLine($"  Pending: {statistics.PendingFiles}");
        sb.AppendLine($"  Errors: {statistics.ErrorFiles}");
        sb.AppendLine($"Unparsed: {statistics.UnprocessedFiles}");
        sb.AppendLine($"  Average Confidence: {statistics.AverageConfidence:F1}%");
        sb.AppendLine($"  Total Size: {statistics.FileSizeText}");

        return await Task.FromResult(sb.ToString());
    }

    /// <inheritdoc/>
    public async Task<string> GenerateEpisodeSummaryAsync(
        IEnumerable<FileEpisodeModel> episodes,
        FileStatistics statistics)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TV Episode Files Export Summary");
        sb.AppendLine("===============================");
        sb.AppendLine();
        sb.AppendLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("Statistics:");
        sb.AppendLine($"  Total Files: {statistics.TotalFiles}");
        sb.AppendLine($"  Processed: {statistics.ProcessedFiles}");
        sb.AppendLine($"  Pending: {statistics.PendingFiles}");
        sb.AppendLine($"  Errors: {statistics.ErrorFiles}");
        sb.AppendLine($"  Unparsed: {statistics.UnprocessedFiles}");
        sb.AppendLine($"  Total Size: {statistics.FileSizeText}");

        return await Task.FromResult(sb.ToString());
    }

    /// <inheritdoc/>
    public string GetFileExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Csv => ".csv",
            ExportFormat.Json => ".json",
            ExportFormat.Xml => ".xml",
            ExportFormat.Excel => ".xlsx",
            _ => ".txt"
        };
    }

    /// <inheritdoc/>
    public string GetFileFilter(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Csv => "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            ExportFormat.Json => "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            ExportFormat.Xml => "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            ExportFormat.Excel => "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            _ => "All Files (*.*)|*.*"
        };
    }

    #region Private Methods

    private async Task<bool> ExportMoviesToCsvAsync(
        List<MovieFileModel> movies,
        ExportOptions options,
        IProgress<int>? progress)
    {
        var csv = new StringBuilder();

        // Header
        var headers = new List<string>();
        if (options.IncludeFileName) headers.Add("FileName");
        if (options.IncludeNewFileName) headers.Add("NewFileName");
        if (options.IncludeMediaName) headers.Add("MovieName");
        if (options.IncludeYear) headers.Add("Year");
        if (options.IncludeConfidence) headers.Add("Confidence");
        if (options.IncludeStatus) headers.Add("Status");
        if (options.IncludeFileSize) headers.Add("FileSize");
        if (options.IncludeExtension) headers.Add("Extension");
        if (options.IncludeFullPaths) headers.Add("SourcePath");
        if (options.IncludeFullPaths) headers.Add("DestinationPath");
        if (options.IncludeErrors) headers.Add("ErrorMessage");
        if (options.IncludeTimestamp) headers.Add("ExportedAt");

        csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

        // Data rows
        for (int i = 0; i < movies.Count; i++)
        {
            var movie = movies[i];
            var values = new List<string>();

            if (options.IncludeFileName) values.Add(CsvEscape(movie.FileName));
            if (options.IncludeNewFileName) values.Add(CsvEscape(movie.NewFileName));
            if (options.IncludeMediaName) values.Add(CsvEscape(movie.MovieName));
            if (options.IncludeYear) values.Add(CsvEscape(movie.MovieYear));
            if (options.IncludeConfidence) values.Add(movie.Confidence.ToString());
            if (options.IncludeStatus) values.Add(CsvEscape(movie.StatusText));
            if (options.IncludeFileSize) values.Add(movie.FileSize.ToString());
            if (options.IncludeExtension) values.Add(CsvEscape(movie.Extension));
            if (options.IncludeFullPaths) values.Add(CsvEscape(movie.SourcePath));
            if (options.IncludeFullPaths) values.Add(CsvEscape(movie.DestinationPath));
            if (options.IncludeErrors) values.Add(CsvEscape(movie.ErrorMessage));
            if (options.IncludeTimestamp) values.Add(CsvEscape(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            csv.AppendLine(string.Join(",", values));

            progress?.Report((i + 1) * 100 / movies.Count);
        }

        await File.WriteAllTextAsync(options.OutputPath, csv.ToString(), Encoding.UTF8);
        return true;
    }

    private async Task<bool> ExportMoviesToJsonAsync(
        List<MovieFileModel> movies,
        ExportOptions options,
        IProgress<int>? progress)
    {
        var data = movies.Select((movie, index) =>
        {
            var obj = new Dictionary<string, object?>();

            if (options.IncludeFileName) obj["fileName"] = movie.FileName;
            if (options.IncludeNewFileName) obj["newFileName"] = movie.NewFileName;
            if (options.IncludeMediaName) obj["movieName"] = movie.MovieName;
            if (options.IncludeYear) obj["year"] = movie.MovieYear;
            if (options.IncludeConfidence) obj["confidence"] = movie.Confidence;
            if (options.IncludeStatus) obj["status"] = movie.StatusText;
            if (options.IncludeFileSize) obj["fileSize"] = movie.FileSize;
            if (options.IncludeExtension) obj["extension"] = movie.Extension;
            if (options.IncludeFullPaths)
            {
                obj["sourcePath"] = movie.SourcePath;
                obj["destinationPath"] = movie.DestinationPath;
            }

            if (options.IncludeErrors) obj["errorMessage"] = movie.ErrorMessage;
            if (options.IncludeTimestamp) obj["exportedAt"] = DateTime.Now.ToString("o");

            progress?.Report((index + 1) * 100 / movies.Count);
            return obj;
        }).ToList();

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(options.OutputPath, json, Encoding.UTF8);
        return true;
    }

    private async Task<bool> ExportEpisodesToCsvAsync(
        List<FileEpisodeModel> episodes,
        ExportOptions options,
        IProgress<int>? progress)
    {
        var csv = new StringBuilder();

        // Header
        var headers = new List<string>();
        if (options.IncludeFileName) headers.Add("FileName");
        if (options.IncludeNewFileName) headers.Add("NewFileName");
        if (options.IncludeMediaName) headers.Add("ShowName");
        if (options.IncludeSeasonEpisode) headers.Add("Season");
        if (options.IncludeSeasonEpisode) headers.Add("Episode");
        if (options.IncludeStatus) headers.Add("Status");
        if (options.IncludeExtension) headers.Add("Extension");
        if (options.IncludeFullPaths) headers.Add("SourcePath");
        if (options.IncludeFullPaths) headers.Add("DestinationPath");
        if (options.IncludeErrors) headers.Add("ErrorMessage");
        if (options.IncludeTimestamp) headers.Add("ExportedAt");

        csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

        // Data rows
        for (int i = 0; i < episodes.Count; i++)
        {
            var episode = episodes[i];
            var values = new List<string>();

            if (options.IncludeFileName) values.Add(CsvEscape(episode.CoreEpisode.Filename));
            if (options.IncludeNewFileName) values.Add(CsvEscape(episode.NewFileName));
            if (options.IncludeMediaName) values.Add(CsvEscape(episode.ShowName));
            if (options.IncludeSeasonEpisode) values.Add(episode.SeasonNumber.ToString());
            if (options.IncludeSeasonEpisode) values.Add(episode.EpisodeNumbers);
            if (options.IncludeStatus) values.Add(CsvEscape(episode.StatusText));
            if (options.IncludeExtension) values.Add(CsvEscape(episode.Extension));
            if (options.IncludeFullPaths) values.Add(CsvEscape(episode.SourcePath));
            if (options.IncludeFullPaths) values.Add(CsvEscape(episode.DestinationPath));
            if (options.IncludeErrors) values.Add(CsvEscape(episode.ErrorMessage));
            if (options.IncludeTimestamp) values.Add(CsvEscape(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            csv.AppendLine(string.Join(",", values));

            progress?.Report((i + 1) * 100 / episodes.Count);
        }

        await File.WriteAllTextAsync(options.OutputPath, csv.ToString(), Encoding.UTF8);
        return true;
    }

    private async Task<bool> ExportEpisodesToJsonAsync(
        List<FileEpisodeModel> episodes,
        ExportOptions options,
        IProgress<int>? progress)
    {
        var data = episodes.Select((episode, index) =>
        {
            var obj = new Dictionary<string, object?>();

            if (options.IncludeFileName) obj["fileName"] = episode.CoreEpisode.Filename;
            if (options.IncludeNewFileName) obj["newFileName"] = episode.NewFileName;
            if (options.IncludeMediaName) obj["showName"] = episode.ShowName;
            if (options.IncludeSeasonEpisode)
            {
                obj["season"] = episode.SeasonNumber;
                obj["episode"] = episode.EpisodeNumbers;
            }

            if (options.IncludeStatus) obj["status"] = episode.StatusText;
            if (options.IncludeExtension) obj["extension"] = episode.Extension;
            if (options.IncludeFullPaths)
            {
                obj["sourcePath"] = episode.SourcePath;
                obj["destinationPath"] = episode.DestinationPath;
            }

            if (options.IncludeErrors) obj["errorMessage"] = episode.ErrorMessage;
            if (options.IncludeTimestamp) obj["exportedAt"] = DateTime.Now.ToString("o");

            progress?.Report((index + 1) * 100 / episodes.Count);
            return obj;
        }).ToList();

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(options.OutputPath, json, Encoding.UTF8);
        return true;
    }

    private async Task<bool> ExportMoviesToXmlAsync(
        List<MovieFileModel> movies,
        ExportOptions options,
        IProgress<int>? progress)
    {
        var root = new XElement("Movies",
            new XAttribute("ExportDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
            new XAttribute("TotalCount", movies.Count)
        );

        for (int i = 0; i < movies.Count; i++)
        {
            var movie = movies[i];
            var movieElement = new XElement("Movie");

            if (options.IncludeFileName) movieElement.Add(new XElement("FileName", movie.FileName));
            if (options.IncludeNewFileName) movieElement.Add(new XElement("NewFileName", movie.NewFileName));
            if (options.IncludeMediaName) movieElement.Add(new XElement("MovieName", movie.MovieName));
            if (options.IncludeYear) movieElement.Add(new XElement("Year", movie.MovieYear));
            if (options.IncludeConfidence) movieElement.Add(new XElement("Confidence", movie.Confidence));
            if (options.IncludeStatus) movieElement.Add(new XElement("Status", movie.StatusText));
            if (options.IncludeFileSize) movieElement.Add(new XElement("FileSize", movie.FileSize));
            if (options.IncludeExtension) movieElement.Add(new XElement("Extension", movie.Extension));
            if (options.IncludeFullPaths)
            {
                movieElement.Add(new XElement("SourcePath", movie.SourcePath));
                movieElement.Add(new XElement("DestinationPath", movie.DestinationPath));
            }

            if (options.IncludeErrors && !string.IsNullOrEmpty(movie.ErrorMessage))
            {
                movieElement.Add(new XElement("ErrorMessage", movie.ErrorMessage));
            }

            if (options.IncludeTimestamp) movieElement.Add(new XElement("ExportedAt", DateTime.Now.ToString("o")));

            root.Add(movieElement);
            progress?.Report((i + 1) * 100 / movies.Count);
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        await Task.Run(() => doc.Save(options.OutputPath));
        return true;
    }

    private async Task<bool> ExportEpisodesToXmlAsync(
        List<FileEpisodeModel> episodes,
        ExportOptions options,
        IProgress<int>? progress)
    {
        var root = new XElement("Episodes",
            new XAttribute("ExportDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
            new XAttribute("TotalCount", episodes.Count)
        );

        for (int i = 0; i < episodes.Count; i++)
        {
            var episode = episodes[i];
            var episodeElement = new XElement("Episode");

            if (options.IncludeFileName) episodeElement.Add(new XElement("FileName", episode.CoreEpisode.Filename));
            if (options.IncludeNewFileName) episodeElement.Add(new XElement("NewFileName", episode.NewFileName));
            if (options.IncludeMediaName) episodeElement.Add(new XElement("ShowName", episode.ShowName));
            if (options.IncludeSeasonEpisode)
            {
                episodeElement.Add(new XElement("Season", episode.SeasonNumber));
                episodeElement.Add(new XElement("Episode", episode.EpisodeNumbers));
            }

            if (options.IncludeStatus) episodeElement.Add(new XElement("Status", episode.StatusText));
            if (options.IncludeExtension) episodeElement.Add(new XElement("Extension", episode.Extension));
            if (options.IncludeFullPaths)
            {
                episodeElement.Add(new XElement("SourcePath", episode.SourcePath));
                episodeElement.Add(new XElement("DestinationPath", episode.DestinationPath));
            }

            if (options.IncludeErrors && !string.IsNullOrEmpty(episode.ErrorMessage))
            {
                episodeElement.Add(new XElement("ErrorMessage", episode.ErrorMessage));
            }

            if (options.IncludeTimestamp) episodeElement.Add(new XElement("ExportedAt", DateTime.Now.ToString("o")));

            root.Add(episodeElement);
            progress?.Report((i + 1) * 100 / episodes.Count);
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        await Task.Run(() => doc.Save(options.OutputPath));
        return true;
    }

    private string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        // Escape quotes and wrap in quotes if contains comma, quote, or newline
        value = value.Replace("\"", "\"\"");
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value}\"";

        return $"\"{value}\"";
    }

    #endregion
}