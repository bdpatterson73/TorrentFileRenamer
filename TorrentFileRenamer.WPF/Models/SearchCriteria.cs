using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.Models;

/// <summary>
/// Represents search criteria for filtering media files
/// </summary>
public class SearchCriteria : ObservableObject
{
    private string _searchText = string.Empty;
    private bool _searchInFileName = true;
    private bool _searchInMovieName = true;
    private bool _searchInShowName = true;
    private bool _searchInYear = false;
    private int _minConfidence;
    private int _maxConfidence = 100;
    private long _minFileSize;
    private long _maxFileSize = long.MaxValue;
    private DateTime? _dateAddedFrom;
    private DateTime? _dateAddedTo;
    private List<string> _selectedExtensions = new();
    private List<ProcessingStatus> _selectedStatuses = new();

    /// <summary>
    /// The search text to match against
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    /// <summary>
    /// Whether to search in file names
    /// </summary>
    public bool SearchInFileName
    {
        get => _searchInFileName;
        set => SetProperty(ref _searchInFileName, value);
    }

    /// <summary>
    /// Whether to search in movie names
    /// </summary>
    public bool SearchInMovieName
    {
        get => _searchInMovieName;
        set => SetProperty(ref _searchInMovieName, value);
    }

    /// <summary>
    /// Whether to search in TV show names
    /// </summary>
    public bool SearchInShowName
    {
        get => _searchInShowName;
        set => SetProperty(ref _searchInShowName, value);
    }

    /// <summary>
    /// Whether to search in year fields
    /// </summary>
    public bool SearchInYear
    {
        get => _searchInYear;
        set => SetProperty(ref _searchInYear, value);
    }

    /// <summary>
    /// Minimum confidence percentage (0-100)
    /// </summary>
    public int MinConfidence
    {
        get => _minConfidence;
        set => SetProperty(ref _minConfidence, value);
    }

    /// <summary>
    /// Maximum confidence percentage (0-100)
    /// </summary>
    public int MaxConfidence
    {
        get => _maxConfidence;
        set => SetProperty(ref _maxConfidence, value);
    }

    /// <summary>
    /// Minimum file size in bytes
    /// </summary>
    public long MinFileSize
    {
        get => _minFileSize;
        set => SetProperty(ref _minFileSize, value);
    }

    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    public long MaxFileSize
    {
        get => _maxFileSize;
        set => SetProperty(ref _maxFileSize, value);
    }

    /// <summary>
    /// Filter by date added (from)
    /// </summary>
    public DateTime? DateAddedFrom
    {
        get => _dateAddedFrom;
        set => SetProperty(ref _dateAddedFrom, value);
    }

    /// <summary>
    /// Filter by date added (to)
    /// </summary>
    public DateTime? DateAddedTo
    {
        get => _dateAddedTo;
        set => SetProperty(ref _dateAddedTo, value);
    }

    /// <summary>
    /// Selected file extensions to filter by
    /// </summary>
    public List<string> SelectedExtensions
    {
        get => _selectedExtensions;
        set => SetProperty(ref _selectedExtensions, value);
    }

    /// <summary>
    /// Selected statuses to filter by
    /// </summary>
    public List<ProcessingStatus> SelectedStatuses
    {
        get => _selectedStatuses;
        set => SetProperty(ref _selectedStatuses, value);
    }

    /// <summary>
    /// Whether any filters are active
    /// </summary>
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchText) ||
        MinConfidence > 0 ||
        MaxConfidence < 100 ||
        MinFileSize > 0 ||
        MaxFileSize < long.MaxValue ||
        DateAddedFrom.HasValue ||
        DateAddedTo.HasValue ||
        SelectedExtensions.Count > 0 ||
        SelectedStatuses.Count > 0;

    /// <summary>
    /// Resets all filters to defaults
    /// </summary>
    public void Reset()
    {
        SearchText = string.Empty;
        SearchInFileName = true;
        SearchInMovieName = true;
        SearchInShowName = true;
        SearchInYear = false;
        MinConfidence = 0;
        MaxConfidence = 100;
        MinFileSize = 0;
        MaxFileSize = long.MaxValue;
        DateAddedFrom = null;
        DateAddedTo = null;
        SelectedExtensions.Clear();
        SelectedStatuses.Clear();
    }

    /// <summary>
    /// Creates a copy of the search criteria
    /// </summary>
    public SearchCriteria Clone()
    {
        return new SearchCriteria
        {
            SearchText = SearchText,
            SearchInFileName = SearchInFileName,
            SearchInMovieName = SearchInMovieName,
            SearchInShowName = SearchInShowName,
            SearchInYear = SearchInYear,
            MinConfidence = MinConfidence,
            MaxConfidence = MaxConfidence,
            MinFileSize = MinFileSize,
            MaxFileSize = MaxFileSize,
            DateAddedFrom = DateAddedFrom,
            DateAddedTo = DateAddedTo,
            SelectedExtensions = new List<string>(SelectedExtensions),
            SelectedStatuses = new List<ProcessingStatus>(SelectedStatuses)
        };
    }
}