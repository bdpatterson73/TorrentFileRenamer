using System.Globalization;
using System.Windows.Data;

namespace TorrentFileRenamer.WPF.Converters;

/// <summary>
/// Converts file size in bytes to human-readable format (KB, MB, GB)
/// </summary>
public class FileSizeConverter : IValueConverter
{
  private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB" };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
   {
   return "0 bytes";
 }

        long bytes = 0;
        
        if (value is long longValue)
        {
     bytes = longValue;
        }
    else if (value is int intValue)
 {
    bytes = intValue;
        }
        else if (value is string stringValue && long.TryParse(stringValue, out long parsedValue))
        {
            bytes = parsedValue;
        }
  else
     {
            return value.ToString() ?? "0 bytes";
        }

        if (bytes < 0)
        {
            return "0 bytes";
        }

        if (bytes == 0)
        {
    return "0 bytes";
}

   int magnitude = 0;
        decimal adjustedSize = bytes;

        while (adjustedSize >= 1024 && magnitude < SizeSuffixes.Length - 1)
        {
            magnitude++;
          adjustedSize /= 1024;
      }

        return $"{adjustedSize:0.##} {SizeSuffixes[magnitude]}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
