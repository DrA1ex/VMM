using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VMM.Converters
{
    internal class IsRemovedToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = value is bool && (bool)value;

            return !flag ? Brushes.Transparent : Application.Current.MainWindow.FindResource("ButtonTextDisabled");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}