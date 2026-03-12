using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoHitCounter.Converters;

public class StringToBrushConverter : IValueConverter
{
    private static readonly StringToColorConverter ColorConverter = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var result = ColorConverter.Convert(value, typeof(Color), parameter, culture);
        if (result is Color color)
            return new SolidColorBrush(color);
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}