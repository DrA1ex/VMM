using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VMM.Converters
{
    public class StringNullOrEmptyToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            var invert = parameter as string == "invert";

            return invert
                ? (string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible)
                : (string.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}