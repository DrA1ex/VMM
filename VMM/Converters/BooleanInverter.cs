using System;
using System.Globalization;
using System.Windows.Data;

namespace VMM.Converters
{
    public class BooleanInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = (value is bool || value is int || value is long)
                        && System.Convert.ToBoolean(value);

            return !flag;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}