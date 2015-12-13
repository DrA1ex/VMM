using System;
using System.Windows;
using System.Windows.Media;

namespace VMM.Helper
{
    public static class UiTreeHelper
    {
        public static DependencyObject FindOfType(DependencyObject src, Type type)
        {
            if(src.GetType() == type)
            {
                return src;
            }

            for(var i = 0; i < VisualTreeHelper.GetChildrenCount(src); i++)
            {
                var child = VisualTreeHelper.GetChild(src, i);

                var result = FindOfType(child, type);
                if(result == null)
                {
                    continue;
                }

                return result;
            }
            return null;
        }
    }
}