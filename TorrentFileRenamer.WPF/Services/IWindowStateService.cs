using System.Windows;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Service for persisting and restoring window state
/// </summary>
public interface IWindowStateService
{
    /// <summary>
  /// Saves the window state (position, size, maximized state)
 /// </summary>
    void SaveWindowState(Window window, string windowKey);

    /// <summary>
    /// Restores the window state
 /// </summary>
    void RestoreWindowState(Window window, string windowKey);

    /// <summary>
    /// Saves column widths for a specific grid
    /// </summary>
    void SaveColumnWidths(string gridKey, IEnumerable<double> widths);

    /// <summary>
    /// Restores column widths for a specific grid
    /// </summary>
    IEnumerable<double>? RestoreColumnWidths(string gridKey);

    /// <summary>
    /// Saves the last selected tab index
    /// </summary>
    void SaveSelectedTab(string tabKey, int index);

    /// <summary>
    /// Restores the last selected tab index
    /// </summary>
    int RestoreSelectedTab(string tabKey);
}
