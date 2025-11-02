using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TorrentFileRenamer.WPF.Models;

namespace TorrentFileRenamer.WPF.Converters;

/// <summary>
/// Converts ProcessingStatus to Visibility for Retry button (visible only for Failed status)
/// </summary>
public class StatusToRetryVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProcessingStatus status)
        {
            return status == ProcessingStatus.Failed ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}