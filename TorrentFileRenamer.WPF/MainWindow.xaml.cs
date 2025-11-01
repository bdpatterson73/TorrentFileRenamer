using System.Windows;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels;
using TorrentFileRenamer.WPF.Views;

namespace TorrentFileRenamer.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string WindowKey = "MainWindow";
    private readonly IWindowStateService _windowStateService;
    private readonly MainViewModel _viewModel;
    private readonly TvEpisodesViewModel _tvEpisodesViewModel;
    private readonly MoviesViewModel _moviesViewModel;
    private readonly AutoMonitorViewModel _autoMonitorViewModel;

    public MainWindow(
        MainViewModel viewModel,
        IWindowStateService windowStateService,
        TvEpisodesViewModel tvEpisodesViewModel,
        MoviesViewModel moviesViewModel,
        AutoMonitorViewModel autoMonitorViewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _windowStateService = windowStateService ?? throw new ArgumentNullException(nameof(windowStateService));
        _tvEpisodesViewModel = tvEpisodesViewModel ?? throw new ArgumentNullException(nameof(tvEpisodesViewModel));
        _moviesViewModel = moviesViewModel ?? throw new ArgumentNullException(nameof(moviesViewModel));
        _autoMonitorViewModel = autoMonitorViewModel ?? throw new ArgumentNullException(nameof(autoMonitorViewModel));

        DataContext = _viewModel;

        // Register tab ViewModels with MainViewModel for keyboard shortcut routing
        _viewModel.RegisterTabViewModels(_tvEpisodesViewModel, _moviesViewModel, _autoMonitorViewModel);

        // Set DataContext for each tab view
        var tvView = FindName("TvEpisodesTabContent") as TvEpisodesView;
        if (tvView != null) tvView.DataContext = _tvEpisodesViewModel;

        var moviesView = FindName("MoviesTabContent") as MoviesView;
        if (moviesView != null) moviesView.DataContext = _moviesViewModel;

        var autoMonitorView = FindName("AutoMonitorTabContent") as AutoMonitorView;
        if (autoMonitorView != null) autoMonitorView.DataContext = _autoMonitorViewModel;

        // Restore window state
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _windowStateService.RestoreWindowState(this, WindowKey);

        // Restore selected tab
        var tabIndex = _windowStateService.RestoreSelectedTab("MainTabs");
        if (MainTabControl.Items.Count > tabIndex)
        {
            MainTabControl.SelectedIndex = tabIndex;
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Save window state
        _windowStateService.SaveWindowState(this, WindowKey);

        // Save selected tab
        _windowStateService.SaveSelectedTab("MainTabs", MainTabControl.SelectedIndex);
    }
}