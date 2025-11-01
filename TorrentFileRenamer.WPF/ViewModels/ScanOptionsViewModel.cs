using System.IO;
using TorrentFileRenamer.WPF.ViewModels.Base;
using System.Windows.Input;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for scan options dialog
/// </summary>
public class ScanOptionsViewModel : ViewModelBase
{
    private string _sourcePath = string.Empty;
  private string _destinationPath = string.Empty;
    private string _fileExtensions = ".mkv;.mp4;.avi";
    private string[] _validationErrors = Array.Empty<string>();

    public ScanOptionsViewModel()
    {
        BrowseSourceCommand = new RelayCommand(BrowseSource);
        BrowseDestinationCommand = new RelayCommand(BrowseDestination);
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
    Validate();
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
                Validate();
     }
        }
    }

    /// <summary>
    /// File extensions (semicolon separated)
    /// </summary>
  public string FileExtensions
    {
     get => _fileExtensions;
        set
        {
            if (SetProperty(ref _fileExtensions, value))
   {
    Validate();
            }
        }
    }

    /// <summary>
    /// Parsed file extensions as array
    /// </summary>
 public string[] FileExtensionsArray
    {
        get
    {
            return FileExtensions
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
             .Select(ext => ext.Trim())
      .Where(ext => !string.IsNullOrWhiteSpace(ext))
     .Select(ext => ext.StartsWith('.') ? ext : '.' + ext)
     .ToArray();
        }
    }

    /// <summary>
    /// Validation errors
    /// </summary>
    public string[] ValidationErrors
    {
        get => _validationErrors;
     private set => SetProperty(ref _validationErrors, value);
    }

    /// <summary>
    /// Whether the options are valid
    /// </summary>
    public bool IsValid => ValidationErrors.Length == 0;

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
        var dialog = new Microsoft.Win32.OpenFolderDialog
    {
       Title = "Select Source Directory"
        };

        if (!string.IsNullOrEmpty(SourcePath) && Directory.Exists(SourcePath))
        {
    dialog.InitialDirectory = SourcePath;
      }

  if (dialog.ShowDialog() == true)
   {
   SourcePath = dialog.FolderName;
      }
    }

    private void BrowseDestination()
    {
     var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Destination Directory"
        };

        if (!string.IsNullOrEmpty(DestinationPath) && Directory.Exists(DestinationPath))
        {
   dialog.InitialDirectory = DestinationPath;
        }

        if (dialog.ShowDialog() == true)
      {
   DestinationPath = dialog.FolderName;
        }
    }

    private void Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SourcePath))
     {
      errors.Add("Source path is required");
        }
  else if (!Directory.Exists(SourcePath))
        {
      errors.Add("Source directory does not exist");
     }

        if (string.IsNullOrWhiteSpace(DestinationPath))
   {
            errors.Add("Destination path is required");
        }

        if (string.IsNullOrWhiteSpace(FileExtensions))
        {
            errors.Add("At least one file extension is required");
        }
        else if (FileExtensionsArray.Length == 0)
    {
  errors.Add("No valid file extensions specified");
        }

      ValidationErrors = errors.ToArray();
    }
}
