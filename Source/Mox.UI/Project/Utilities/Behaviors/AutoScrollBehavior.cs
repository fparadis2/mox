using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public static class AutoScrollBehavior
    {
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, OnEnabledChanged));

        public static bool GetEnabled(ScrollViewer scroll)
        {
            return (bool)scroll.GetValue(EnabledProperty);
        }

        public static void SetEnabled(ScrollViewer scroll, bool value)
        {
            scroll.SetValue(EnabledProperty, value);
        }

        private static readonly DependencyProperty AutoScrollingProperty = DependencyProperty.RegisterAttached("_AutoScrolling", typeof(bool), typeof(AutoScrollBehavior));

        private static void OnEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;

            if ((bool)e.NewValue)
            {
                scrollViewer.ScrollToEnd();
                scrollViewer.ScrollChanged += OnScrollChanged;
            }
            else
            {
                scrollViewer.ScrollChanged -= OnScrollChanged;
            }
        }

        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;

            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {
                scrollViewer.SetValue(AutoScrollingProperty, scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight);
            }
            else if ((bool)scrollViewer.GetValue(AutoScrollingProperty))
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
            }
        }
    }
}
