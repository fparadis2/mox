using System;
using System.Windows;
using System.Windows.Media;

namespace Mox.UI
{
    public static class StyleHelper
    {
        #region MouseOverBrush

        public static readonly DependencyProperty MouseOverBrushProperty = DependencyProperty.RegisterAttached(
            "MouseOverBrush", typeof(Brush), typeof(StyleHelper), new PropertyMetadata(default(Brush)));

        public static void SetMouseOverBrush(DependencyObject element, Brush value)
        {
            element.SetValue(MouseOverBrushProperty, value);
        }

        public static Brush GetMouseOverBrush(DependencyObject element)
        {
            return (Brush)element.GetValue(MouseOverBrushProperty);
        }
        
        #endregion

        #region MouseOverBorderBrush

        public static readonly DependencyProperty MouseOverBorderBrushProperty = DependencyProperty.RegisterAttached(
            "MouseOverBorderBrush", typeof(Brush), typeof(StyleHelper), new PropertyMetadata(default(Brush)));

        public static void SetMouseOverBorderBrush(DependencyObject element, Brush value)
        {
            element.SetValue(MouseOverBorderBrushProperty, value);
        }

        public static Brush GetMouseOverBorderBrush(DependencyObject element)
        {
            return (Brush)element.GetValue(MouseOverBorderBrushProperty);
        }

        #endregion

        #region FocusBorderBrush

        public static readonly DependencyProperty FocusBorderBrushProperty = DependencyProperty.RegisterAttached(
            "FocusBorderBrush", typeof(Brush), typeof(StyleHelper), new PropertyMetadata(default(Brush)));

        public static void SetFocusBorderBrush(DependencyObject element, Brush value)
        {
            element.SetValue(FocusBorderBrushProperty, value);
        }

        public static Brush GetFocusBorderBrush(DependencyObject element)
        {
            return (Brush)element.GetValue(FocusBorderBrushProperty);
        }

        #endregion

        #region PressedBrush

        public static readonly DependencyProperty PressedBrushProperty = DependencyProperty.RegisterAttached(
            "PressedBrush", typeof (Brush), typeof (StyleHelper), new PropertyMetadata(default(Brush)));

        public static void SetPressedBrush(DependencyObject element, Brush value)
        {
            element.SetValue(PressedBrushProperty, value);
        }

        public static Brush GetPressedBrush(DependencyObject element)
        {
            return (Brush) element.GetValue(PressedBrushProperty);
        }

        #endregion

        #region DisabledBrush

        public static readonly DependencyProperty DisabledBrushProperty = DependencyProperty.RegisterAttached(
            "DisabledBrush", typeof (Brush), typeof (StyleHelper), new PropertyMetadata(default(Brush)));

        public static void SetDisabledBrush(DependencyObject element, Brush value)
        {
            element.SetValue(DisabledBrushProperty, value);
        }

        public static Brush GetDisabledBrush(DependencyObject element)
        {
            return (Brush) element.GetValue(DisabledBrushProperty);
        }

        #endregion
    }
}
