using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoHitCounter.Converters
{
    public class LockIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? "🔓" : "🔒";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class LockTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? $"Unlocked Splits:{Environment.NewLine}Reordering with drag and Renaming with double click are enabled" 
                : $"Locked Splits:{Environment.NewLine}Reordering with drag and Renaming with double click are disabled.{Environment.NewLine}Double Click now moves through splits.";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}