using System.Windows.Media;

namespace VMM.Helper
{
    public static class ColorSerializationHelper
    {
        public static ColorConverter ColorConverter { get; } = new ColorConverter();

        public static Color FromString(string str)
        {
            var result = ColorConverter.ConvertFromString(str);
            if(result != null)
                return ((Color)result);

            return Colors.White;
        }

        public static string ToString(Color c)
        {
            return ColorConverter.ConvertToString(c);
        }
    }
}
