using System;
using System.Windows;

namespace Mox.UI
{
    public enum ContextualColor
    {
        Normal,
        Default,
        Danger
    }

    public static class StyleExtensions
    {
        public static readonly DependencyProperty ContextualColorProperty = DependencyProperty.RegisterAttached(
            "ContextualColor", typeof(ContextualColor), typeof(StyleExtensions), new PropertyMetadata(ContextualColor.Normal));

        public static void SetContextualColor(DependencyObject element, ContextualColor value)
        {
            element.SetValue(ContextualColorProperty, value);
        }

        public static ContextualColor GetContextualColor(DependencyObject element)
        {
            return (ContextualColor)element.GetValue(ContextualColorProperty);
        }

        public static readonly DependencyProperty HeaderFontSizeProperty = DependencyProperty.RegisterAttached(
            "HeaderFontSize", typeof(double), typeof(StyleExtensions), new PropertyMetadata(24.0));

        public static void SetHeaderFontSize(DependencyObject element, double value)
        {
            element.SetValue(HeaderFontSizeProperty, value);
        }

        public static double GetHeaderFontSize(DependencyObject element)
        {
            return (double)element.GetValue(HeaderFontSizeProperty);
        }
    }
}
