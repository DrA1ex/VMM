using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VMM.Converters
{
    internal class BoolToPlayIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = value is bool && (bool)value;
            return Application.Current.MainWindow.FindResource(flag ? "PauseIcon" : "PlayIcon");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}