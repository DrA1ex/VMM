using System;
using System.Linq;
using System.Reflection;

namespace VMM.Helper
{
    public static class ReflectionHelper
    {
        public static object GetPropertyValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(src, null);
        }

        public static object[] GetStaticProperties(Type type)
        {
            return type.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static)
                .Select(c => c.GetValue(null))
                .ToArray();
        }
    }
}