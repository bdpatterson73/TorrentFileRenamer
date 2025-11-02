using System.Windows.Input;
using TorrentFileRenamer.WPF.Models;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for statistics widget
/// </summary>
public class StatsViewModel : ViewModelBase
{
    private readonly ISearchService _searchService;
    private FileStatistics _statistics = new();
    private bool _isVisible = true;
    private bool _isExpanded = true;
    private DateTime _lastRefresh = DateTime.Now;

    public StatsViewModel(ISearchService searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));

        // Commands
        RefreshCommand = new RelayCommand(ExecuteRefresh);
        ToggleVisibilityCommand = new RelayCommand(_ => IsVisible = !IsVisible);
        ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
    }

    #region Properties

    /// <summary>
    /// Current statistics
    /// </summary>
    public FileStatistics Statistics
    {
        get => _statistics;
        set => SetProperty(ref _statistics, value);
    }

    /// <summary>
    /// Whether the widget is visible
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    /// <summary>
    /// Whether the widget is expanded
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    /// <summary>
    /// When statistics were last refreshed
    /// </summary>
    public DateTime LastRefresh
    {
        get => _lastRefresh;
        set
        {
            if (SetProperty(ref _lastRefresh, value))
            {
                OnPropertyChanged(nameof(LastRefreshText));
            }
        }
    }

    /// <summary>
    /// Last refresh as readable text
    /// </summary>
    public string LastRefreshText
    {
        get
        {
            var elapsed = DateTime.Now - LastRefresh;
            if (elapsed.TotalSeconds < 60)
                return "just now";
            if (elapsed.TotalMinutes < 60)
                return $"{(int)elapsed.TotalMinutes} minute{((int)elapsed.TotalMinutes != 1 ? "s" : "")} ago";
            if (elapsed.TotalHours < 24)
                return $"{(int)elapsed.TotalHours} hour{((int)elapsed.TotalHours != 1 ? "s" : "")} ago";
            return LastRefresh.ToString("yyyy-MM-dd HH:mm");
        }
    }

    /// <summary>
    /// Success rate percentage
    /// </summary>
    public double SuccessRate
    {
        get
        {
            if (Statistics.TotalFiles == 0) return 0;
            return (double)Statistics.ProcessedFiles / Statistics.TotalFiles * 100;
        }
    }

    /// <summary>
    /// Error rate percentage
    /// </summary>
    public double ErrorRate
    {
        get
        {
            if (Statistics.TotalFiles == 0) return 0;
            return (double)Statistics.ErrorFiles / Statistics.TotalFiles * 100;
        }
    }

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand ToggleVisibilityCommand { get; }
    public ICommand ToggleExpandCommand { get; }

    #endregion

    #region Events

    /// <summary>
    /// Raised when refresh is requested
    /// </summary>
    public event EventHandler? RefreshRequested;

    #endregion

    #region Command Implementations

    private void ExecuteRefresh(object? parameter)
    {
        RefreshRequested?.Invoke(this, EventArgs.Empty);
        LastRefresh = DateTime.Now;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates statistics for movie files
    /// </summary>
    public void UpdateMovieStatistics(IEnumerable<MovieFileModel> movies)
    {
        Statistics = _searchService.CalculateMovieStatistics(movies);
        LastRefresh = DateTime.Now;
        OnPropertyChanged(nameof(SuccessRate));
        OnPropertyChanged(nameof(ErrorRate));
    }

    /// <summary>
    /// Updates statistics for TV episode files
    /// </summary>
    public void UpdateEpisodeStatistics(IEnumerable<FileEpisodeModel> episodes)
    {
        Statistics = _searchService.CalculateEpisodeStatistics(episodes);
        LastRefresh = DateTime.Now;
        OnPropertyChanged(nameof(SuccessRate));
        OnPropertyChanged(nameof(ErrorRate));
    }

    /// <summary>
    /// Resets statistics
    /// </summary>
    public void Reset()
    {
        Statistics.Reset();
        LastRefresh = DateTime.Now;
        OnPropertyChanged(nameof(SuccessRate));
        OnPropertyChanged(nameof(ErrorRate));
    }

    /// <summary>
    /// Gets a summary text for display
    /// </summary>
    public string GetSummaryText()
    {
        return $"{Statistics.TotalFiles} files | " +
               $"{Statistics.ProcessedFiles} processed | " +
               $"{Statistics.ErrorFiles} errors | " +
               $"{Statistics.FileSizeText}";
    }

    #endregion
}