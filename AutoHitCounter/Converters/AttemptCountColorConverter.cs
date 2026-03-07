using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoHitCounter.Converters;

public class AttemptCountColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Active = new(Color.FromRgb(0x9D, 0x61, 0xA8));
    private static readonly SolidColorBrush Muted = new(Color.FromRgb(0xFF, 0xFF, 0xFF));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is int count && count > 0 ? Active : Muted;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}