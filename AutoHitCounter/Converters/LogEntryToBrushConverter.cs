//

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AutoHitCounter.Models;

namespace AutoHitCounter.Converters;

public class LogEntryToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EventLogEntry entry)
        {
            return entry.Value ? Brushes.Chartreuse : Brushes.Red;
        }

        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
