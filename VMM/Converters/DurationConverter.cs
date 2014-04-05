using System;
using System.Globalization;
using System.Windows.Data;

namespace VMM.Converters
{
    public class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                var duration = (int) value;
                return String.Format("{0}:{1:00}", duration/60, duration%60);
            }

            return "#err";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}