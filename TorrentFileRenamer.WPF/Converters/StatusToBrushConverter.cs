using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TorrentFileRenamer.WPF.Models;
using Color = System.Windows.Media.Color;

namespace TorrentFileRenamer.WPF.Converters;

/// <summary>
/// Converts ProcessingStatus to Material Design color brush
/// </summary>
public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProcessingStatus status)
        {
            return status switch
            {
                ProcessingStatus.Pending => new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)), // Blue
                ProcessingStatus.Processing => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07)), // Amber
                ProcessingStatus.Completed => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)), // Green
                ProcessingStatus.Failed => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)), // Red
                ProcessingStatus.Unparsed => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)), // Gray
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}