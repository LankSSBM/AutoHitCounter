//

using System;
using System.Globalization;
using System.Windows.Data;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Enum e ? e.GetDescription() : value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
