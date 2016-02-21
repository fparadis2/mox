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
        #region Colors

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

        #endregion

        #region Fonts

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

        #endregion

        #region Content

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark", typeof (string), typeof (StyleExtensions), new PropertyMetadata(default(string)));

        public static void SetWatermark(DependencyObject element, string value)
        {
            element.SetValue(WatermarkProperty, value);
        }

        public static string GetWatermark(DependencyObject element)
        {
            return (string) element.GetValue(WatermarkProperty);
        }

        #endregion
    }
}
