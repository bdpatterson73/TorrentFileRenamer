using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for Scan Movies Dialog
/// </summary>
public class ScanMoviesViewModel : ViewModelBase
{
    private string _sourcePath = string.Empty;
    private string _destinationPath = string.Empty;
    private string _fileExtensions = ".mkv, .mp4, .avi, .m4v";
    private int _minimumConfidence = 40;
    private bool _isValid;

    public ScanMoviesViewModel()
    {
        BrowseSourceCommand = new RelayCommand(_ => BrowseSource());
    BrowseDestinationCommand = new RelayCommand(_ => BrowseDestination());
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
