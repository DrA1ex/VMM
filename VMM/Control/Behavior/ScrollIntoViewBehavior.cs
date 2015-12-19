using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using VMM.Helper;

namespace VMM.Control.Behavior
{
    public class ScrollIntoViewBehavior : Behavior<ListView>
    {
        protected override void OnAttached()
        {
            ListView listView = AssociatedObject;
            listView.SelectionChanged += OnListViewSelectionChanged;
        }

        protected override void OnDetaching()
        {
            ListView listView = AssociatedObject;
            listView.SelectionChanged -= OnListViewSelectionChanged;
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = AssociatedObject;
            if(listView.SelectedItems != null && listView.SelectedItems.Count == 1)
            {
                var scrollView = listView.FindVisualChild<ScrollViewer>();
                if(scrollView != null && listView.SelectedIndex >= 0)
                {
                    var selectedIndex = listView.SelectedIndex;
                    if(scrollView.VerticalOffset > selectedIndex)
                    {
                        scrollView.ScrollToVerticalOffset(selectedIndex);
                    }
                    else if(scrollView.VerticalOffset + scrollView.ViewportHeight < selectedIndex)
                    {
                        scrollView.ScrollToVerticalOffset(Math.Max(0, selectedIndex - scrollView.ViewportHeight));
                    }
                }
            }
        }
    }
}
