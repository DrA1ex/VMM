using System.Reflection;

namespace VMM.Helper
{
    public static class ReflectionHelper
    {
        public static object GetPropertyValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(src, null);
        }
    }
}