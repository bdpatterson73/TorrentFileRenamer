using System.Globalization;
using System.Windows.Data;
using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Converters;

/// <summary>
/// Converts ProcessingStatus to Segoe MDL2 Assets icon
/// </summary>
public class StatusToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProcessingStatus status)
        {
            return status switch
            {
                ProcessingStatus.Pending => "\uE91F", // Clock icon
                ProcessingStatus.Processing => "\uE895", // Progress ring / Sync icon
                ProcessingStatus.Completed => "\uE73E", // Checkmark circle filled
                ProcessingStatus.Failed => "\uE711", // Error / X icon
                ProcessingStatus.Unparsed => "\uE9CE", // Unknown / Question mark
                _ => "\uE9CE"
            };
        }

        return "\uE9CE";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}