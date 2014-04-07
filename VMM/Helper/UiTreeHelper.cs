using System;
using System.Windows;
using System.Windows.Media;

namespace VMM.Helper
{
    public static class UiTreeHelper
    {
        public static DependencyObject FindOfType(DependencyObject src, Type type)
        {
            if (src.GetType() == type)
            {
                return src;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(src); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(src, i);

                DependencyObject result = FindOfType(child, type);
                if (result == null)
                {
                    continue;
                }

                return result;
            }
            return null;
        }
    }
}