using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI
{
    public static class PanAndZoomBehavior
    {
        #region Attached Properties

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled", typeof(bool), typeof(PanAndZoomBehavior), new PropertyMetadata(default(bool), OnEnabledChanged));

        public static void SetEnabled(ScrollViewer element, UIElement value)
        {
            element.SetValue(EnabledProperty, value);
        }

        public static bool GetEnabled(ScrollViewer element)
        {
            return (bool)element.GetValue(EnabledProperty);
        }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer element = d as ScrollViewer;
            if (element == null)
                return;

            if ((bool)e.NewValue)
            {
                element.Loaded += WhenLoaded;
                element.ScrollChanged += WhenScrollChanged;
            }
            else
            {
                element.Loaded -= WhenLoaded;
                element.ScrollChanged -= WhenScrollChanged;
            }
        }

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.RegisterAttached(
            "Zoom", typeof (double), typeof (PanAndZoomBehavior), new PropertyMetadata(1.0));

        public static void SetZoom(DependencyObject element, double value)
        {
            element.SetValue(ZoomProperty, value);
        }

        public static double GetZoom(DependencyObject element)
        {
            return (double) element.GetValue(ZoomProperty);
        }

        #endregion

        #region Methods

        private static void Initialize(FrameworkElement element)
        {
            TransformGroup group = new TransformGroup();

            group.Children.Add(new ScaleTransform());
            group.Children.Add(new TranslateTransform());

            element.LayoutTransform = group;
        }

        private static void WhenLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as ScrollViewer;
            if (element == null)
                return;

            var content = GetContent(element);
            Initialize(content);
            ZoomExtents(element, content);
        }

        private static void WhenScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var element = (ScrollViewer)sender;
            var content = GetContent(element);
            ZoomExtents(element, content);
        }

        private static void ZoomExtents(ScrollViewer element, FrameworkElement content)
        {
            var scaleTransform = GetScaleTransform(content);
            if (scaleTransform == null)
                return;

            Size size = GetContentSize(content);

            if (size.Width <= 0 ||
                size.Height <= 0)
            {
                SetZoom(element, 1);
                return;
            }

            double widthRatio = (element.ViewportWidth - content.Margin.Left - content.Margin.Right) / size.Width;
            double heightRatio = (element.ViewportHeight - content.Margin.Top - content.Margin.Bottom) / size.Height;

            double ratio = Math.Min(widthRatio, heightRatio);

            SetZoom(element, ratio);
            scaleTransform.ScaleX = ratio;
            scaleTransform.ScaleY = ratio;
        }

        private static ScaleTransform GetScaleTransform(FrameworkElement element)
        {
            TransformGroup group = element.LayoutTransform as TransformGroup;
            if (group == null)
                return null;

            return group.Children.OfType<ScaleTransform>().First();
        }

        private static readonly PropertyInfo ms_itemsHostProperty = typeof (ItemsControl).GetProperty("ItemsHost",
            BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);

        private static FrameworkElement GetContent(ScrollViewer element)
        {
            return element.Content as FrameworkElement;
        }

        private static Size GetContentSize(FrameworkElement content)
        {
            ItemsControl itemsControl = content as ItemsControl;
            if (itemsControl != null && ms_itemsHostProperty != null)
            {
                return GetContentSize((FrameworkElement)ms_itemsHostProperty.GetValue(content));
            }

            return new Size(content.ActualWidth, content.ActualHeight);
        }

        #endregion
    }
}
