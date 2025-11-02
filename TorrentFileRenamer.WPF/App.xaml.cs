using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.Core.Services;
using TorrentFileRenamer.WPF.Services;
using TorrentFileRenamer.WPF.ViewModels;
using WpfApplication = System.Windows.Application;

namespace TorrentFileRenamer.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : WpfApplication
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core Services from TorrentFileRenamer.Core
        services.AddSingleton<AppSettings>(sp => AppSettings.Load());
        services.AddSingleton<FolderMonitorService>();

        // WPF Services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IScanningService, ScanningService>();
        services.AddSingleton<IFileProcessingService, FileProcessingService>();
        services.AddSingleton<IWindowStateService, WindowStateService>();
        services.AddSingleton<IMruService, MruService>();
        services.AddSingleton<ISearchService, SearchService>();
        services.AddSingleton<IExportService, ExportService>();  // Phase 7

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<TvEpisodesViewModel>();
        services.AddTransient<MoviesViewModel>();
        services.AddSingleton<AutoMonitorViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<FilterViewModel>();
        services.AddTransient<StatsViewModel>();
        services.AddTransient<ExportViewModel>();  // Phase 7

        // Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Dispose AutoMonitorViewModel to stop monitoring and cleanup
        var autoMonitorViewModel = _serviceProvider?.GetService<AutoMonitorViewModel>();
        autoMonitorViewModel?.Dispose();
        
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

