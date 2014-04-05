using System;
using System.Globalization;
using System.Windows.Data;

namespace VMM.Converters
{
    public class StringNullOrEmptyToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return String.IsNullOrEmpty(str) ? "n\\a" : str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}