using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VMM.Converters
{
    internal class BoolToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var invert = parameter as string == "invert";
            var flag = (value is bool || value is int || value is long)
                       && System.Convert.ToBoolean(value);
            if(invert)
            {
                flag = !flag;
            }

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}