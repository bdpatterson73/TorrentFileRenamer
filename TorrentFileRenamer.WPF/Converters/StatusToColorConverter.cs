using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TorrentFileRenamer.WPF.Models;
using MediaColor = System.Windows.Media.Color;
using MediaBrushes = System.Windows.Media.Brushes;

namespace TorrentFileRenamer.WPF.Converters;

/// <summary>
/// Converts ProcessingStatus to a color brush
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
    if (value is ProcessingStatus status)
      {
      return status switch
            {
    ProcessingStatus.Pending => new SolidColorBrush(MediaColor.FromRgb(232, 244, 248)), // Light Blue
       ProcessingStatus.Processing => new SolidColorBrush(MediaColor.FromRgb(254, 249, 231)), // Light Yellow
      ProcessingStatus.Completed => new SolidColorBrush(MediaColor.FromRgb(232, 245, 233)), // Light Green
 ProcessingStatus.Failed => new SolidColorBrush(MediaColor.FromRgb(253, 236, 234)), // Light Red
          ProcessingStatus.Unparsed => new SolidColorBrush(MediaColor.FromRgb(245, 245, 245)), // Light Gray
  _ => MediaBrushes.White
        };
  }

        return MediaBrushes.White;
  }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
 throw new NotImplementedException();
 }
}
