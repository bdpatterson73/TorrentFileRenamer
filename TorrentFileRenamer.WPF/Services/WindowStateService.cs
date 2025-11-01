using System.Windows;
using TorrentFileRenamer.Core.Configuration;
using CoreWindowState = TorrentFileRenamer.Core.Configuration.WindowState;
using WpfWindowState = System.Windows.WindowState;

namespace TorrentFileRenamer.WPF.Services;

/// <summary>
/// Service for persisting and restoring window state
/// </summary>
public class WindowStateService : IWindowStateService
{
  private readonly AppSettings _appSettings;

    public WindowStateService(AppSettings appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    /// <summary>
    /// Saves the window state (position, size, maximized state)
    /// </summary>
    public void SaveWindowState(Window window, string windowKey)
  {
        if (window == null) throw new ArgumentNullException(nameof(window));
        if (string.IsNullOrWhiteSpace(windowKey)) throw new ArgumentException("Window key cannot be empty", nameof(windowKey));

        var state = new CoreWindowState
        {
            Left = window.Left,
Top = window.Top,
            Width = window.Width,
  Height = window.Height,
      WindowStateValue = (int)window.WindowState
        };

      _appSettings.WindowStates[windowKey] = state;
        _appSettings.Save();
    }

    /// <summary>
    /// Restores the window state
    /// </summary>
    public void RestoreWindowState(Window window, string windowKey)
    {
      if (window == null) throw new ArgumentNullException(nameof(window));
        if (string.IsNullOrWhiteSpace(windowKey)) throw new ArgumentException("Window key cannot be empty", nameof(windowKey));

        if (_appSettings.WindowStates.TryGetValue(windowKey, out var state))
        {
     // Ensure the window is visible on at least one screen
          if (IsWindowOnScreen(state.Left, state.Top, state.Width, state.Height))
            {
       window.Left = state.Left;
     window.Top = state.Top;
    window.Width = state.Width;
      window.Height = state.Height;
     window.WindowState = (WpfWindowState)state.WindowStateValue;
            }
        }
    }

    /// <summary>
    /// Saves column widths for a specific grid
    /// </summary>
    public void SaveColumnWidths(string gridKey, IEnumerable<double> widths)
    {
        if (string.IsNullOrWhiteSpace(gridKey)) throw new ArgumentException("Grid key cannot be empty", nameof(gridKey));
    if (widths == null) throw new ArgumentNullException(nameof(widths));

        _appSettings.ColumnWidths[gridKey] = widths.ToList();
        _appSettings.Save();
    }

    /// <summary>
    /// Restores column widths for a specific grid
    /// </summary>
    public IEnumerable<double>? RestoreColumnWidths(string gridKey)
  {
        if (string.IsNullOrWhiteSpace(gridKey)) throw new ArgumentException("Grid key cannot be empty", nameof(gridKey));

        if (_appSettings.ColumnWidths.TryGetValue(gridKey, out var widths))
        {
 return widths;
        }

        return null;
    }

    /// <summary>
    /// Saves the last selected tab index
    /// </summary>
  public void SaveSelectedTab(string tabKey, int index)
  {
     if (string.IsNullOrWhiteSpace(tabKey)) throw new ArgumentException("Tab key cannot be empty", nameof(tabKey));

        _appSettings.SelectedTabs[tabKey] = index;
        _appSettings.Save();
    }

    /// <summary>
    /// Restores the last selected tab index
    /// </summary>
    public int RestoreSelectedTab(string tabKey)
  {
        if (string.IsNullOrWhiteSpace(tabKey)) throw new ArgumentException("Tab key cannot be empty", nameof(tabKey));

        if (_appSettings.SelectedTabs.TryGetValue(tabKey, out var index))
    {
   return index;
        }

   return 0; // Default to first tab
    }

    /// <summary>
    /// Checks if a window position is visible on any screen
    /// </summary>
    private bool IsWindowOnScreen(double left, double top, double width, double height)
    {
        var rect = new Rect(left, top, width, height);

        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
          var screenRect = new Rect(
           screen.WorkingArea.Left,
     screen.WorkingArea.Top,
   screen.WorkingArea.Width,
        screen.WorkingArea.Height);

       if (screenRect.IntersectsWith(rect))
   {
       return true;
 }
        }

  return false;
    }
}
