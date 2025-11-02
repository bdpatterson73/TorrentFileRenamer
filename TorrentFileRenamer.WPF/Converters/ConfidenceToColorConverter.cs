using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;

namespace TorrentFileRenamer.WPF.Converters;

/// <summary>
/// Converts confidence percentage (0-100) to a color brush
/// </summary>
public class ConfidenceToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int confidence)
        {
            if (confidence >= 70)
                return new SolidColorBrush(MediaColor.FromRgb(76, 175, 80)); // Green
            if (confidence >= 40)
                return new SolidColorBrush(MediaColor.FromRgb(255, 152, 0)); // Orange
            return new SolidColorBrush(MediaColor.FromRgb(244, 67, 54)); // Red
        }

        return new SolidColorBrush(MediaColor.FromRgb(158, 158, 158)); // Gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}