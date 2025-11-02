using System.Windows;
using TorrentFileRenamer.Core.Configuration;
using TorrentFileRenamer.WPF.ViewModels;
using TorrentFileRenamer.WPF.Views;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;
using WpfMessageBoxResult = System.Windows.MessageBoxResult;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfOpenFolderDialog = Microsoft.Win32.OpenFolderDialog;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;
using WpfApplication = System.Windows.Application;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Implementation of dialog service for WPF
/// </summary>
public class DialogService : IDialogService
{
    private readonly AppSettings _appSettings;

    public DialogService(AppSettings appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    public Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var result = WpfMessageBox.Show(
            message,
            title,
            WpfMessageBoxButton.YesNo,
            WpfMessageBoxImage.Question);

        return Task.FromResult(result == WpfMessageBoxResult.Yes);
    }

    public Task ShowMessageAsync(string title, string message)
    {
        WpfMessageBox.Show(
            message,
            title,
            WpfMessageBoxButton.OK,
            WpfMessageBoxImage.Information);

        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string title, string message)
    {
        WpfMessageBox.Show(
            message,
            title,
            WpfMessageBoxButton.OK,
            WpfMessageBoxImage.Error);

        return Task.CompletedTask;
    }

    public Task<string?> ShowFolderBrowserAsync(string? initialPath = null)
    {
        var dialog = new WpfOpenFolderDialog
        {
            Title = "Select Folder",
            InitialDirectory = initialPath ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FolderName : null);
    }

    public Task<string?> ShowOpenFileDialogAsync(string? filter = null)
    {
        var dialog = new WpfOpenFileDialog
        {
            Filter = filter ?? "All Files (*.*)|*.*",
            CheckFileExists = true
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FileName : null);
    }

    // Synchronous versions for non-async contexts

    public bool ShowConfirmation(string title, string message)
    {
        var result = WpfMessageBox.Show(
            message,
            title,
            WpfMessageBoxButton.YesNo,
            WpfMessageBoxImage.Question);

        return result == WpfMessageBoxResult.Yes;
    }

    public void ShowMessage(string title, string message)
    {
        WpfMessageBox.Show(
            message,
            title,
            WpfMessageBoxButton.OK,
            WpfMessageBoxImage.Information);
    }

    public string? ShowFolderBrowserDialog(string? initialPath = null)
    {
        var dialog = new WpfOpenFolderDialog
        {
            Title = "Select Folder",
            InitialDirectory = initialPath ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        var result = dialog.ShowDialog();
        return result == true ? dialog.FolderName : null;
    }

    public string? ShowSaveFileDialog(string? defaultFileName = null, string? filter = null)
    {
        var dialog = new WpfSaveFileDialog
        {
            FileName = defaultFileName ?? "export.txt",
            Filter = filter ?? "All Files (*.*)|*.*",
            AddExtension = true
        };

        var result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }

    public bool? ShowSettingsDialog()
    {
        var viewModel = new SettingsViewModel(_appSettings, this);
        var dialog = new SettingsDialog
        {
            DataContext = viewModel,
            Owner = WpfApplication.Current.MainWindow
        };

        return dialog.ShowDialog();
    }

    public void ShowLogViewerDialog()
    {
        var viewModel = new LogViewerViewModel(this);
        var dialog = new LogViewerDialog
        {
            DataContext = viewModel,
            Owner = WpfApplication.Current.MainWindow
        };

        dialog.ShowDialog();
    }

    public void ShowAboutDialog()
    {
        var dialog = new AboutDialog
        {
            Owner = WpfApplication.Current.MainWindow
        };

        dialog.ShowDialog();
    }
}