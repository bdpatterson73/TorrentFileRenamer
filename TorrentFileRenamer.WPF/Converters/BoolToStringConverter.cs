using System.Globalization;
using System.Windows.Data;

namespace TorrentFileRenamer.WPF.Converters;

/// <summary>
/// Converts boolean values to string representations
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
       return string.Empty;

        if (parameter is not string paramString)
            return boolValue.ToString();

        // Parameter format: "TrueValue|FalseValue"
        var parts = paramString.Split('|');
        if (parts.Length != 2)
  return boolValue.ToString();

        return boolValue ? parts[0] : parts[1];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
