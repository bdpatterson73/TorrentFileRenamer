using System.Collections.ObjectModel;
using System.Windows.Input;
using TorrentFileRenamer.WPF.Models;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for search functionality
/// </summary>
public class SearchViewModel : ViewModelBase
{
    private readonly ISearchService _searchService;
    private string _searchText = string.Empty;
    private bool _isSearchVisible;
    private List<string> _searchHistory = new();
    private int _resultCount;

    public SearchViewModel(ISearchService searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));

// Commands
        SearchCommand = new RelayCommand(ExecuteSearch, CanExecuteSearch);
        ClearSearchCommand = new RelayCommand(ExecuteClearSearch, _ => !string.IsNullOrWhiteSpace(SearchText));
        ToggleSearchVisibilityCommand = new RelayCommand(_ => IsSearchVisible = !IsSearchVisible);
        SelectHistoryCommand = new RelayCommand(ExecuteSelectHistory);
        ClearHistoryCommand = new RelayCommand(ExecuteClearHistory, _ => SearchHistory.Count > 0);
    }

    #region Properties

    /// <summary>
    /// Current search text
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ((RelayCommand)SearchCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ClearSearchCommand).RaiseCanExecuteChanged();

                // Auto-search on text change (with debounce in real implementation)
                if (!string.IsNullOrWhiteSpace(value))
                {
                    SearchCommand.Execute(null);
                }
            }
        }
    }

    /// <summary>
    /// Whether the search panel is visible
    /// </summary>
    public bool IsSearchVisible
    {
        get => _isSearchVisible;
        set => SetProperty(ref _isSearchVisible, value);
    }

    /// <summary>
    /// Search history
    /// </summary>
    public List<string> SearchHistory
    {
        get => _searchHistory;
        set => SetProperty(ref _searchHistory, value);
    }

    /// <summary>
    /// Number of search results
    /// </summary>
    public int ResultCount
    {
        get => _resultCount;
        set => SetProperty(ref _resultCount, value);
    }

    /// <summary>
    /// Whether there are search results
    /// </summary>
    public bool HasResults => ResultCount > 0;

    /// <summary>
    /// Result summary text
    /// </summary>
    public string ResultSummary => ResultCount > 0
        ? $"Found {ResultCount} result{(ResultCount != 1 ? "s" : "")}"
        : "No results found";

    #endregion

    #region Commands

    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand ToggleSearchVisibilityCommand { get; }
    public ICommand SelectHistoryCommand { get; }
    public ICommand ClearHistoryCommand { get; }

    #endregion

    #region Events

    /// <summary>
    /// Raised when search criteria changes
    /// </summary>
    public event EventHandler? SearchChanged;

    #endregion

    #region Command Implementations

    private bool CanExecuteSearch(object? parameter)
    {
        return !string.IsNullOrWhiteSpace(SearchText);
    }

    private void ExecuteSearch(object? parameter)
    {
        // Add to history if not already present
        if (!string.IsNullOrWhiteSpace(SearchText) && !SearchHistory.Contains(SearchText))
        {
            var history = new List<string>(SearchHistory) { SearchText };
            if (history.Count > 10) // Keep last 10 searches
            {
                history.RemoveAt(0);
            }

            SearchHistory = history;
        }

        // Notify listeners
        SearchChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ExecuteClearSearch(object? parameter)
    {
        SearchText = string.Empty;
        ResultCount = 0;
        SearchChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ExecuteSelectHistory(object? parameter)
    {
        if (parameter is string searchText && !string.IsNullOrWhiteSpace(searchText))
        {
            SearchText = searchText;
        }
    }

    private void ExecuteClearHistory(object? parameter)
    {
        SearchHistory = new List<string>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the result count
    /// </summary>
    public void UpdateResultCount(int count)
    {
        ResultCount = count;
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(ResultSummary));
    }

    /// <summary>
    /// Loads search history from settings
    /// </summary>
    public void LoadSearchHistory(List<string> history)
    {
        SearchHistory = history ?? new List<string>();
    }

    /// <summary>
    /// Gets current search history for saving
    /// </summary>
    public List<string> GetSearchHistory()
    {
        return new List<string>(SearchHistory);
    }

    #endregion
}