using System;
using System.Globalization;
using System.Windows.Data;

namespace VMM.Converters
{
    internal class SortingMethodDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var descending = value is bool && (bool)value;
            return descending ? "По убыванию" : "По возрастанию";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}