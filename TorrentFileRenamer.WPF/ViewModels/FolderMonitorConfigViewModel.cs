using System.IO;
using System.Windows;
using Microsoft.Win32;
using TorrentFileRenamer.WPF.ViewModels.Base;

namespace TorrentFileRenamer.WPF.ViewModels;

/// <summary>
/// ViewModel for the Folder Monitor Configuration Dialog
/// </summary>
public class FolderMonitorConfigViewModel : ViewModelBase
{
    private string _watchFolder = string.Empty;
    private string _destinationFolder = string.Empty;
    private string _fileExtensions = "*.mp4;*.mkv;*.avi;*.m4v";
    private int _stabilityDelay = 30;
    private bool _autoStart = false;

    public FolderMonitorConfigViewModel()
    {
        BrowseWatchFolderCommand = new RelayCommand(BrowseWatchFolder);
        BrowseDestinationFolderCommand = new RelayCommand(BrowseDestinationFolder);
    }

    public string WatchFolder
    {
        get => _watchFolder;
        set
        {
            if (SetProperty(ref _watchFolder, value))
            {
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    public string DestinationFolder
    {
        get => _destinationFolder;
        set
        {
            if (SetProperty(ref _destinationFolder, value))
            {
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    public string FileExtensions
    {
        get => _fileExtensions;
        set => SetProperty(ref _fileExtensions, value);
    }

    public int StabilityDelay
    {
        get => _stabilityDelay;
        set => SetProperty(ref _stabilityDelay, value);
    }

    public bool AutoStart
    {
        get => _autoStart;
        set => SetProperty(ref _autoStart, value);
    }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(WatchFolder) &&
        Directory.Exists(WatchFolder) &&
        !string.IsNullOrWhiteSpace(DestinationFolder);

    public RelayCommand BrowseWatchFolderCommand { get; }
    public RelayCommand BrowseDestinationFolderCommand { get; }

    private void BrowseWatchFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Watch Folder",
            InitialDirectory = !string.IsNullOrWhiteSpace(WatchFolder) && Directory.Exists(WatchFolder)
                ? WatchFolder
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog() == true)
        {
            WatchFolder = dialog.FolderName;
        }
    }

    private void BrowseDestinationFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Destination Folder",
            InitialDirectory = !string.IsNullOrWhiteSpace(DestinationFolder) && Directory.Exists(DestinationFolder)
                ? DestinationFolder
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog() == true)
        {
            DestinationFolder = dialog.FolderName;
        }
    }
}