using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace VMM.Converters
{
    public class BoolToSaveIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = values.Aggregate(false, (current, value) => current | value is bool && (bool)value);
            return Application.Current.MainWindow.FindResource(flag ? "WaitIcon" : "DownloadIcon");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}