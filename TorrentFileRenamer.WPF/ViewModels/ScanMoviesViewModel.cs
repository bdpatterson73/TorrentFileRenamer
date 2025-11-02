using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for Scan Movies Dialog
/// </summary>
public class ScanMoviesViewModel : ViewModelBase
{
    private readonly AppSettings _appSettings;
    private string _sourcePath = string.Empty;
    private string _destinationPath = string.Empty;
    private string _fileExtensions = ".mkv, .mp4, .avi, .m4v";
    private int _minimumConfidence = 40;
    private bool _isValid;

    public ScanMoviesViewModel(AppSettings appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));

        BrowseSourceCommand = new RelayCommand(_ => BrowseSource());
        BrowseDestinationCommand = new RelayCommand(_ => BrowseDestination());

        // Load last used paths if RememberLastPaths is enabled
        if (_appSettings.RememberLastPaths)
        {
            _sourcePath = _appSettings.LastMovieSourcePath;
            _destinationPath = _appSettings.LastMovieDestinationPath;
            _fileExtensions = !string.IsNullOrWhiteSpace(_appSettings.LastMovieFileExtensions)
                ? _appSettings.LastMovieFileExtensions
                : ".mkv, .mp4, .avi, .m4v";
            _minimumConfidence = _appSettings.LastMovieMinimumConfidence;

            // Trigger property change notifications
            OnPropertyChanged(nameof(SourcePath));
            OnPropertyChanged(nameof(DestinationPath));
            OnPropertyChanged(nameof(FileExtensions));
            OnPropertyChanged(nameof(MinimumConfidence));

            ValidatePaths();
        }
    }

    /// <summary>
    /// Source directory path
    /// </summary>
    public string SourcePath
    {
        get => _sourcePath;
        set
        {
            if (SetProperty(ref _sourcePath, value))
            {
                ValidatePaths();
            }
        }
    }

    /// <summary>
    /// Destination directory path
    /// </summary>
    public string DestinationPath
    {
        get => _destinationPath;
        set
        {
            if (SetProperty(ref _destinationPath, value))
            {
                ValidatePaths();
            }
        }
    }

    /// <summary>
    /// File extensions (comma-separated)
    /// </summary>
    public string FileExtensions
    {
        get => _fileExtensions;
        set => SetProperty(ref _fileExtensions, value);
    }

    /// <summary>
    /// File extensions as array
    /// </summary>
    public string[] FileExtensionsArray
    {
        get
        {
            return FileExtensions
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim())
                .Where(ext => !string.IsNullOrWhiteSpace(ext))
                .Select(ext => ext.StartsWith('.') ? ext : $".{ext}")
                .ToArray();
        }
    }

    /// <summary>
    /// Minimum confidence threshold (0-100)
    /// </summary>
    public int MinimumConfidence
    {
        get => _minimumConfidence;
        set => SetProperty(ref _minimumConfidence, value);
    }

    /// <summary>
    /// Whether the input is valid
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        private set => SetProperty(ref _isValid, value);
    }

    /// <summary>
    /// Command to browse for source directory
    /// </summary>
    public ICommand BrowseSourceCommand { get; }

    /// <summary>
    /// Command to browse for destination directory
    /// </summary>
    public ICommand BrowseDestinationCommand { get; }

    /// <summary>
    /// Save the current paths to settings
    /// </summary>
    public void SavePaths()
    {
        if (_appSettings.RememberLastPaths)
        {
            _appSettings.LastMovieSourcePath = SourcePath;
            _appSettings.LastMovieDestinationPath = DestinationPath;
            _appSettings.LastMovieFileExtensions = FileExtensions;
            _appSettings.LastMovieMinimumConfidence = MinimumConfidence;
            _appSettings.Save();
        }
    }

    private void BrowseSource()
    {
        var dialog = new FolderBrowserDialog
        {
            Description = "Select source folder containing movies",
            ShowNewFolderButton = false,
            SelectedPath = SourcePath
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            SourcePath = dialog.SelectedPath;
        }
    }

    private void BrowseDestination()
    {
        var dialog = new FolderBrowserDialog
        {
            Description = "Select destination folder for organized movies",
            ShowNewFolderButton = true,
            SelectedPath = DestinationPath
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            DestinationPath = dialog.SelectedPath;
        }
    }

    private void ValidatePaths()
    {
        IsValid = !string.IsNullOrWhiteSpace(SourcePath) &&
                  !string.IsNullOrWhiteSpace(DestinationPath) &&
                  Directory.Exists(SourcePath);
    }
}