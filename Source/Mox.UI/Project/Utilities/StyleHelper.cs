using System;
using System.Windows;
using System.Windows.Media;

namespace Mox.UI
{
    public static class StyleHelper
    {
        public static readonly DependencyProperty MouseOverBrushProperty = DependencyProperty.RegisterAttached(
            "MouseOverBrush", typeof (Brush), typeof (StyleHelper), new PropertyMetadata(default(Brush)));

        public static void SetMouseOverBrush(DependencyObject element, Brush value)
        {
            element.SetValue(MouseOverBrushProperty, value);
        }

        public static Brush GetMouseOverBrush(DependencyObject element)
        {
            return (Brush) element.GetValue(MouseOverBrushProperty);
        }

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
    }
}
