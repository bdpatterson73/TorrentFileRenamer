using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TorrentFileRenamer.WPF.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels providing INotifyPropertyChanged implementation
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
{
    private bool _disposed = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
  /// Raises the PropertyChanged event
    /// </summary>
    /// <param name="propertyName">Name of the property that changed</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets the property value and raises PropertyChanged if the value has changed
    /// </summary>
    /// <typeparam name="T">Type of the property</typeparam>
    /// <param name="field">Reference to the backing field</param>
    /// <param name="value">New value</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>True if the value changed, false otherwise</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
  if (EqualityComparer<T>.Default.Equals(field, value))
   return false;

   field = value;
     OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Disposes the ViewModel and releases resources
    /// </summary>
    public void Dispose()
    {
  Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the ViewModel. Override this method to clean up resources in derived classes.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
        {
          if (disposing)
    {
         // Dispose managed resources in derived classes
            }
    _disposed = true;
        }
  }
}
