using System.IO;
using TorrentFileRenamer.Core.Models;
using TorrentFileRenamer.Core.Services;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// WPF wrapper for Core's MovieFile with observable properties
/// </summary>
public class MovieFileModel : ObservableObject
{
    private readonly MovieFile _coreMovie;
    private ProcessingStatus _status;
    private string _errorMessage = string.Empty;
    private int _confidence;

    public MovieFileModel(MovieFile coreMovie, string sourceFile)
    {
        _coreMovie = coreMovie;

        // Calculate confidence using MediaTypeDetector
        _confidence = MediaTypeDetector.GetMovieConfidence(sourceFile);

        // Set initial status based on whether parsing succeeded
        _status = string.IsNullOrEmpty(coreMovie.MovieName) || coreMovie.MovieName == "Unknown Movie"
            ? ProcessingStatus.Unparsed
            : ProcessingStatus.Pending;
    }

    /// <summary>
    /// The underlying Core MovieFile
    /// </summary>
    public MovieFile CoreMovie => _coreMovie;

    /// <summary>
    /// Source file path
    /// </summary>
    public string SourcePath => _coreMovie.FileNamePath ?? string.Empty;

    /// <summary>
    /// Destination directory path
    /// </summary>
    public string DestinationPath => _coreMovie.NewDestDirectory;

    /// <summary>
    /// Movie name (parsed)
    /// </summary>
    public string MovieName => _coreMovie.MovieName ?? "Unknown Movie";

    /// <summary>
    /// Movie year (if detected)
    /// </summary>
    public string MovieYear => _coreMovie.MovieYear ?? string.Empty;

    /// <summary>
    /// Display name with year (if available)
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(MovieYear))
                return $"{MovieName} ({MovieYear})";
            return MovieName;
        }
    }

    /// <summary>
    /// File name only (without path)
    /// </summary>
    public string FileName => _coreMovie.FileName ?? string.Empty;

    /// <summary>
    /// New filename (extracted from NewDestDirectory)
    /// </summary>
    public string NewFileName
    {
        get
        {
            if (string.IsNullOrEmpty(_coreMovie.NewDestDirectory))
                return string.Empty;

            return Path.GetFileName(_coreMovie.NewDestDirectory) ?? string.Empty;
        }
    }

    /// <summary>
    /// File extension
    /// </summary>
    public string Extension => Path.GetExtension(FileName);

    /// <summary>
    /// File size in bytes (if available)
    /// </summary>
    public long FileSize
    {
        get
        {
            if (string.IsNullOrEmpty(SourcePath) || !File.Exists(SourcePath))
                return 0;

            try
            {
                return new FileInfo(SourcePath).Length;
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Detection confidence (0-100)
    /// </summary>
    public int Confidence
    {
        get => _confidence;
        set => SetProperty(ref _confidence, value);
    }

    /// <summary>
    /// Confidence level as text (High/Medium/Low)
    /// </summary>
    public string ConfidenceLevel
    {
        get
        {
            if (_confidence >= 70) return "High";
            if (_confidence >= 40) return "Medium";
            return "Low";
        }
    }

    /// <summary>
    /// Current processing status
    /// </summary>
    public ProcessingStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    /// <summary>
    /// Status as display text
    /// </summary>
    public string StatusText => Status.ToString();

    /// <summary>
    /// Error message (if any)
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Whether the file parsed successfully
    /// </summary>
    public bool IsParsed => Status != ProcessingStatus.Unparsed;
}