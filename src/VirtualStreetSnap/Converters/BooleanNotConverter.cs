using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace VirtualStreetSnap.Converters;

public class BooleanNotConverter:IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return !boolean;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return !boolean;
        }
        return value;
    }

}