namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Service for displaying dialogs and user interactions
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a confirmation dialog
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);

    /// <summary>
    /// Shows an informational message
    /// </summary>
    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// Shows an error message
    /// </summary>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Shows a folder browser dialog
    /// </summary>
    Task<string?> ShowFolderBrowserAsync(string? initialPath = null);

    /// <summary>
    /// Shows a file open dialog
    /// </summary>
    Task<string?> ShowOpenFileDialogAsync(string? filter = null);

    // Synchronous versions for non-async contexts

    /// <summary>
    /// Shows a confirmation dialog (synchronous)
    /// </summary>
    bool ShowConfirmation(string title, string message);

    /// <summary>
    /// Shows an informational message (synchronous)
    /// </summary>
    void ShowMessage(string title, string message);

    /// <summary>
    /// Shows a folder browser dialog (synchronous)
    /// </summary>
    string? ShowFolderBrowserDialog(string? initialPath = null);

    /// <summary>
    /// Shows a save file dialog (synchronous)
    /// </summary>
    string? ShowSaveFileDialog(string? defaultFileName = null, string? filter = null);

    /// <summary>
    /// Shows the settings dialog
    /// </summary>
    bool? ShowSettingsDialog();

    /// <summary>
    /// Shows the log viewer dialog
    /// </summary>
    void ShowLogViewerDialog();

    /// <summary>
    /// Shows the about dialog
    /// </summary>
    void ShowAboutDialog();
}
