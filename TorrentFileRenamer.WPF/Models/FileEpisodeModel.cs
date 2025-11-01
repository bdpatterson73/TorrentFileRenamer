using TorrentFileRenamer.Core.Models;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// WPF wrapper for Core's FileEpisode with observable properties
/// </summary>
public class FileEpisodeModel : ObservableObject
{
    private readonly FileEpisode _coreEpisode;
    private ProcessingStatus _status;
    private string _errorMessage = string.Empty;

    public FileEpisodeModel(FileEpisode coreEpisode)
    {
      _coreEpisode = coreEpisode;
      
        // Set initial status based on whether parsing succeeded
        _status = string.IsNullOrEmpty(coreEpisode.ShowName) || coreEpisode.NewFileName == "NO NAME"
    ? ProcessingStatus.Unparsed
            : ProcessingStatus.Pending;
 }

    /// <summary>
    /// The underlying Core FileEpisode
    /// </summary>
    public FileEpisode CoreEpisode => _coreEpisode;

    /// <summary>
    /// Source file path
    /// </summary>
    public string SourcePath => _coreEpisode.FullFilePath;

    /// <summary>
    /// Destination file path
    /// </summary>
    public string DestinationPath => _coreEpisode.NewFileNamePath;

    /// <summary>
    /// Show name
    /// </summary>
    public string ShowName => _coreEpisode.ShowName;

    /// <summary>
    /// Season number
    /// </summary>
  public int SeasonNumber => _coreEpisode.SeasonNumber;

    /// <summary>
    /// Primary episode number
    /// </summary>
    public int EpisodeNumber => _coreEpisode.EpisodeNumber;

    /// <summary>
    /// All episode numbers (for multi-episode files)
    /// </summary>
    public string EpisodeNumbers
    {
  get
        {
         if (_coreEpisode.IsMultiEpisode && _coreEpisode.EpisodeNumbers.Count > 1)
     {
    return string.Join(", ", _coreEpisode.EpisodeNumbers);
        }
            return _coreEpisode.EpisodeNumber.ToString();
        }
    }

    /// <summary>
    /// Whether this is a multi-episode file
    /// </summary>
    public bool IsMultiEpisode => _coreEpisode.IsMultiEpisode;

    /// <summary>
    /// New filename
    /// </summary>
    public string NewFileName => _coreEpisode.NewFileName;

    /// <summary>
    /// File extension
    /// </summary>
    public string Extension => _coreEpisode.Extension;

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

    /// <summary>
    /// Plex validation warnings/issues (if any)
    /// </summary>
    public string PlexValidation
    {
        get
     {
     if (_coreEpisode.PlexValidation == null)
   return string.Empty;

            var validation = _coreEpisode.PlexValidation;
     if (validation.IsValid)
     return "? Plex Compatible";

     var messages = new List<string>();
        if (validation.Issues.Count > 0)
     messages.AddRange(validation.Issues);
            if (validation.Warnings.Count > 0)
     messages.AddRange(validation.Warnings);

       return string.Join("; ", messages);
 }
    }
}
