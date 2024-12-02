namespace VirtualStreetSnap.Converters;

using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;

public class WidthToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double width && parameter is string thresholdStr &&
            double.TryParse(thresholdStr, out double threshold))
        {   
            // Console.WriteLine($"Width: {width}, Threshold: {threshold}");
            return !(width < threshold);
        }

        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}