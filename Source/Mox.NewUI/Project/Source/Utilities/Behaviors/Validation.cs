using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public static class Validation
    {
        #region Properties

        public static readonly DependencyProperty AdornedElementToolTipProperty = DependencyProperty.RegisterAttached("AdornedElementToolTip", typeof(object), typeof(Validation), new PropertyMetadata(null, OnAdornedElementToolTipChanged));

        public static void SetAdornedElementToolTip(DependencyObject d, object tooltip)
        {
            d.SetValue(AdornedElementToolTipProperty, tooltip);
        }

        public static object GetAdornedElementToolTip(DependencyObject d)
        {
            return d.GetValue(AdornedElementToolTipProperty);
        }

        private static void OnAdornedElementToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AdornedElementPlaceholder placeholder = d as AdornedElementPlaceholder;
            if (placeholder != null)
            {
                Control control = placeholder.AdornedElement as Control;
                if (control != null)
                {
                    control.ToolTip = e.NewValue;
                }
            }

        }

        #endregion
    }
}
