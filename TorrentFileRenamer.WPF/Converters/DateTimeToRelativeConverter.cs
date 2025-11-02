using System.Globalization;
using System.Windows.Data;

namespace TorrentFileRenamer.WPF.Converters;

/// <summary>
/// Converts DateTime to relative time format (e.g., "2 minutes ago", "yesterday")
/// </summary>
public class DateTimeToRelativeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime dateTime)
        {
            return string.Empty;
        }

        var timeSpan = DateTime.Now - dateTime;

        // Future dates
        if (timeSpan.TotalSeconds < 0)
        {
            return "just now";
        }

        // Less than a minute
        if (timeSpan.TotalSeconds < 60)
        {
            return "just now";
        }

        // Less than an hour
        if (timeSpan.TotalMinutes < 60)
        {
            int minutes = (int)timeSpan.TotalMinutes;
            return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
        }

        // Less than a day
        if (timeSpan.TotalHours < 24)
        {
            int hours = (int)timeSpan.TotalHours;
            return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
        }

        // Less than a week
        if (timeSpan.TotalDays < 7)
        {
            int days = (int)timeSpan.TotalDays;
            if (days == 1)
            {
                return "yesterday";
            }

            return $"{days} days ago";
        }

        // Less than a month
        if (timeSpan.TotalDays < 30)
        {
            int weeks = (int)(timeSpan.TotalDays / 7);
            return weeks == 1 ? "1 week ago" : $"{weeks} weeks ago";
        }

        // Less than a year
        if (timeSpan.TotalDays < 365)
        {
            int months = (int)(timeSpan.TotalDays / 30);
            return months == 1 ? "1 month ago" : $"{months} months ago";
        }

        // More than a year
        int years = (int)(timeSpan.TotalDays / 365);
        return years == 1 ? "1 year ago" : $"{years} years ago";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}