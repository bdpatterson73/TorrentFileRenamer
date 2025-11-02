using System.Windows.Input;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// Main ViewModel for the application window
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private string _statusMessage = "Ready";
    private double _progressValue;
    private double _progressMaximum = 100;
    private bool _isProgressVisible;
    private int _selectedTabIndex;
    private bool _isSearchPanelVisible;

    // Tab ViewModels for routing commands
    private TvEpisodesViewModel? _tvEpisodesViewModel;
    private MoviesViewModel? _moviesViewModel;
    private AutoMonitorViewModel? _autoMonitorViewModel;

    // Phase 6: Search ViewModel
    private SearchViewModel? _searchViewModel;

    public MainViewModel(IDialogService dialogService, SearchViewModel searchViewModel)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _searchViewModel = searchViewModel ?? throw new ArgumentNullException(nameof(searchViewModel));

        // Initialize commands
        ShowSettingsCommand = new RelayCommand(ExecuteShowSettings);
        ExitCommand = new RelayCommand(ExecuteExit);
        ShowAboutCommand = new RelayCommand(ExecuteShowAbout);
        ShowLogsCommand = new RelayCommand(ExecuteShowLogs);
        ScanCommand = new RelayCommand(ExecuteScan, _ => SelectedTabIndex <= 1); // TV or Movies tab
        ProcessCommand = new RelayCommand(ExecuteProcess, _ => SelectedTabIndex <= 1);
        RefreshCommand = new RelayCommand(ExecuteRefresh);
        ShowDocumentationCommand = new RelayCommand(ExecuteShowDocumentation);

        // Phase 5 commands
        ShowKeyboardShortcutsCommand = new RelayCommand(ExecuteShowKeyboardShortcuts);
        ScanTvCommand = new RelayCommand(ExecuteScanTv);
        ScanMoviesCommand = new RelayCommand(ExecuteScanMovies);
        SwitchToTvTabCommand = new RelayCommand(_ => SelectedTabIndex = 0);
        SwitchToMoviesTabCommand = new RelayCommand(_ => SelectedTabIndex = 1);
        SwitchToAutoMonitorTabCommand = new RelayCommand(_ => SelectedTabIndex = 2);

        // Phase 6: Search & Filter commands
        ToggleSearchCommand = new RelayCommand(ExecuteToggleSearch);
        ToggleAdvancedFilterCommand = new RelayCommand(ExecuteToggleAdvancedFilter);
        FocusSearchCommand = new RelayCommand(ExecuteFocusSearch);

        // Phase 7: Export command
        ShowExportCommand = new RelayCommand(ExecuteShowExport);
    }

    #region Properties

    /// <summary>
    /// Status message displayed in the status bar
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Current progress value
    /// </summary>
    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    /// <summary>
    /// Maximum progress value
    /// </summary>
    public double ProgressMaximum
    {
        get => _progressMaximum;
        set => SetProperty(ref _progressMaximum, value);
    }

    /// <summary>
    /// Whether the progress bar is visible
    /// </summary>
    public bool IsProgressVisible
    {
        get => _isProgressVisible;
        set => SetProperty(ref _isProgressVisible, value);
    }

    /// <summary>
    /// Currently selected tab index
    /// </summary>
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (SetProperty(ref _selectedTabIndex, value))
            {
                // Refresh command states when tab changes
                ((RelayCommand)ScanCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ProcessCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Whether the search panel is visible (Phase 6)
    /// </summary>
    public bool IsSearchPanelVisible
    {
        get => _isSearchPanelVisible;
        set => SetProperty(ref _isSearchPanelVisible, value);
    }

    /// <summary>
    /// Search ViewModel for MainWindow search panel (Phase 6)
    /// </summary>
    public SearchViewModel SearchViewModel => _searchViewModel!;

    #endregion

    #region Commands

    public ICommand ShowSettingsCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand ShowAboutCommand { get; }
    public ICommand ShowLogsCommand { get; }
    public ICommand ScanCommand { get; }
    public ICommand ProcessCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ShowDocumentationCommand { get; }

    // Phase 5 commands
    public ICommand ShowKeyboardShortcutsCommand { get; }
    public ICommand ScanTvCommand { get; }
    public ICommand ScanMoviesCommand { get; }
    public ICommand SwitchToTvTabCommand { get; }
    public ICommand SwitchToMoviesTabCommand { get; }
    public ICommand SwitchToAutoMonitorTabCommand { get; }

    // Phase 6: Search & Filter commands
    public ICommand ToggleSearchCommand { get; }
    public ICommand ToggleAdvancedFilterCommand { get; }
    public ICommand FocusSearchCommand { get; }

    // Phase 7: Export command
    public ICommand ShowExportCommand { get; }

    #endregion

    #region Command Implementations

    private void ExecuteShowSettings(object? parameter)
    {
        StatusMessage = "Opening settings...";
        var result = _dialogService.ShowSettingsDialog();
        if (result == true)
        {
            StatusMessage = "Settings saved successfully";
        }
        else
        {
            StatusMessage = "Settings cancelled";
        }
    }

    private void ExecuteExit(object? parameter)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void ExecuteShowAbout(object? parameter)
    {
        _dialogService.ShowAboutDialog();
        StatusMessage = "TorrentFileRenamer - WPF Version 2.0";
    }

    private void ExecuteShowLogs(object? parameter)
    {
        StatusMessage = "Opening log viewer...";
        _dialogService.ShowLogViewerDialog();
        StatusMessage = "Ready";
    }

    private void ExecuteScan(object? parameter)
    {
        // Route command to the active tab's ViewModel
        if (SelectedTabIndex == 0 && _tvEpisodesViewModel?.ScanCommand.CanExecute(null) == true)
        {
            _tvEpisodesViewModel.ScanCommand.Execute(null);
        }
        else if (SelectedTabIndex == 1 && _moviesViewModel?.ScanCommand.CanExecute(null) == true)
        {
            _moviesViewModel.ScanCommand.Execute(null);
        }
    }

    private void ExecuteProcess(object? parameter)
    {
        // Route command to the active tab's ViewModel
        if (SelectedTabIndex == 0 && _tvEpisodesViewModel?.ProcessCommand.CanExecute(null) == true)
        {
            _tvEpisodesViewModel.ProcessCommand.Execute(null);
        }
        else if (SelectedTabIndex == 1 && _moviesViewModel?.ProcessCommand.CanExecute(null) == true)
        {
            _moviesViewModel.ProcessCommand.Execute(null);
        }
    }

    private void ExecuteRefresh(object? parameter)
    {
        // Refresh the current tab (could rescan or refresh data)
        StatusMessage = "Refreshing...";
        // Implementation depends on what refresh means in each context
        StatusMessage = "Ready";
    }

    private void ExecuteShowDocumentation(object? parameter)
    {
        StatusMessage = "Documentation feature coming soon...";
        _dialogService.ShowMessage("Documentation", "Documentation feature will be available in a future update.");
    }

    /// <summary>
    /// Shows the keyboard shortcuts dialog
    /// </summary>
    private void ExecuteShowKeyboardShortcuts(object? parameter)
    {
        try
        {
            var dialog = new Views.KeyboardShortcutsDialog
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            dialog.ShowDialog();
            StatusMessage = "Keyboard shortcuts reference displayed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error showing shortcuts: {ex.Message}";
        }
    }

    /// <summary>
    /// Scan for TV episodes
    /// </summary>
    private void ExecuteScanTv(object? parameter)
    {
        SelectedTabIndex = 0; // Switch to TV tab
        if (_tvEpisodesViewModel?.ScanCommand.CanExecute(null) == true)
        {
            _tvEpisodesViewModel.ScanCommand.Execute(null);
        }
    }

    /// <summary>
    /// Scan for movies
    /// </summary>
    private void ExecuteScanMovies(object? parameter)
    {
        SelectedTabIndex = 1; // Switch to Movies tab
        if (_moviesViewModel?.ScanCommand.CanExecute(null) == true)
        {
            _moviesViewModel.ScanCommand.Execute(null);
        }
    }

    /// <summary>
    /// Phase 6: Toggle search panel visibility (Ctrl+F)
    /// </summary>
    private void ExecuteToggleSearch(object? parameter)
    {
        IsSearchPanelVisible = !IsSearchPanelVisible;
        StatusMessage = IsSearchPanelVisible ? "Search panel opened" : "Search panel closed";

        if (IsSearchPanelVisible)
        {
            // Focus will be handled by the view
            _searchViewModel?.ToggleSearchVisibilityCommand.Execute(null);
        }
    }

    /// <summary>
    /// Phase 6: Toggle advanced filter panel (Ctrl+Shift+F)
    /// </summary>
    private void ExecuteToggleAdvancedFilter(object? parameter)
    {
        // Route to active tab's FilterViewModel
        if (SelectedTabIndex == 0 && _tvEpisodesViewModel != null)
        {
            _tvEpisodesViewModel.FilterViewModel.TogglePanelVisibilityCommand.Execute(null);
            StatusMessage = _tvEpisodesViewModel.FilterViewModel.IsPanelVisible
                ? "Advanced filters opened"
                : "Advanced filters closed";
        }
        else if (SelectedTabIndex == 1 && _moviesViewModel != null)
        {
            _moviesViewModel.FilterViewModel.TogglePanelVisibilityCommand.Execute(null);
            StatusMessage = _moviesViewModel.FilterViewModel.IsPanelVisible
                ? "Advanced filters opened"
                : "Advanced filters closed";
        }
    }

    /// <summary>
    /// Phase 6: Focus search box (Ctrl+E)
    /// </summary>
    private void ExecuteFocusSearch(object? parameter)
    {
        if (!IsSearchPanelVisible)
        {
            IsSearchPanelVisible = true;
        }

        StatusMessage = "Search focused";
        // Focus will be handled by the view
    }

    /// <summary>
    /// Phase 7: Show export options
    /// </summary>
    private void ExecuteShowExport(object? parameter)
    {
        // Route command to the active tab's ViewModel
        if (SelectedTabIndex == 0 && _tvEpisodesViewModel?.ExportCommand.CanExecute(null) == true)
        {
            _tvEpisodesViewModel.ExportCommand.Execute(null);
        }
        else if (SelectedTabIndex == 1 && _moviesViewModel?.ExportCommand.CanExecute(null) == true)
        {
            _moviesViewModel.ExportCommand.Execute(null);
        }
        else
        {
            StatusMessage = "No data available to export";
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Register tab ViewModels for command routing
    /// </summary>
    public void RegisterTabViewModels(
        TvEpisodesViewModel tvEpisodesViewModel,
        MoviesViewModel moviesViewModel,
        AutoMonitorViewModel autoMonitorViewModel)
    {
        _tvEpisodesViewModel = tvEpisodesViewModel;
        _moviesViewModel = moviesViewModel;
        _autoMonitorViewModel = autoMonitorViewModel;
    }

    /// <summary>
    /// Updates the status message
    /// </summary>
    public void UpdateStatus(string message)
    {
        StatusMessage = message;
    }

    /// <summary>
    /// Updates the progress
    /// </summary>
    public void UpdateProgress(double value, double maximum)
    {
        ProgressValue = value;
        ProgressMaximum = maximum;
        IsProgressVisible = value < maximum;
    }

    /// <summary>
    /// Hides the progress bar
    /// </summary>
    public void HideProgress()
    {
        IsProgressVisible = false;
        ProgressValue = 0;
    }

    #endregion
}