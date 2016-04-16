using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mox.UI
{
    public static class CardTapBehavior
    {
        public static readonly DependencyProperty IsTappedProperty = DependencyProperty.RegisterAttached(
            "IsTapped", typeof (bool), typeof (CardTapBehavior), new PropertyMetadata(default(bool), WhenIsTappedChanged));

        public static void SetIsTapped(UIElement element, bool value)
        {
            element.SetValue(IsTappedProperty, value);
        }

        public static bool GetIsTapped(UIElement element)
        {
            return (bool) element.GetValue(IsTappedProperty);
        }

        private static void WhenIsTappedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element == null)
                return;

            var rotateTransform = GetOrCreateRotateTransform(element);
            rotateTransform.CenterX = element.ActualWidth / 2;
            rotateTransform.CenterY = element.ActualHeight - (element.ActualWidth / 2);

            double targetAngle = (bool)e.NewValue ? 90 : 0;
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, MakeAnimation(targetAngle, TimeSpan.FromSeconds(0.1)));
        }

        private static RotateTransform GetOrCreateRotateTransform(UIElement element)
        {
            var rotateTransform = element.RenderTransform as RotateTransform;
            if (rotateTransform == null)
            {
                rotateTransform = new RotateTransform();
                element.RenderTransform = rotateTransform;
            }
            return rotateTransform;
        }

        private static DoubleAnimation MakeAnimation(double to, TimeSpan duration)
        {
            DoubleAnimation anim = new DoubleAnimation(to, duration)
            {
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.7
            };

            return anim;
        }
    }
}
