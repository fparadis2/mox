using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Mox.UI
{
    public static class CardZoomAdornerBehavior
    {
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached("Enabled", typeof (bool), typeof (CardZoomAdornerBehavior), new FrameworkPropertyMetadata(default(bool), WhenEnabledChanged));

        public static void SetEnabled(DependencyObject element, bool value)
        {
            element.SetValue(EnabledProperty, value);
        }

        public static bool GetEnabled(DependencyObject element)
        {
            return (bool) element.GetValue(EnabledProperty);
        }

        public static readonly DependencyProperty TargetControlProperty = DependencyProperty.RegisterAttached("TargetControl", typeof (UIElement), typeof (CardZoomAdornerBehavior), new PropertyMetadata(default(UIElement)));

        public static void SetTargetControl(DependencyObject element, UIElement value)
        {
            element.SetValue(TargetControlProperty, value);
        }

        public static UIElement GetTargetControl(DependencyObject element)
        {
            return (UIElement) element.GetValue(TargetControlProperty);
        }

        private static readonly DependencyProperty AdornerProperty = DependencyProperty.RegisterAttached("Adorner", typeof (CardZoomAdorner), typeof (CardZoomAdornerBehavior), new PropertyMetadata(default(CardZoomAdorner)));

        public static void SetAdorner(DependencyObject element, CardZoomAdorner value)
        {
            element.SetValue(AdornerProperty, value);
        }

        public static CardZoomAdorner GetAdorner(DependencyObject element)
        {
            return (CardZoomAdorner) element.GetValue(AdornerProperty);
        }

        private static void WhenEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = d as UIElement;
            if (element == null)
                return;

            if ((bool) e.NewValue)
            {
                element.PreviewMouseDown += WhenMouseDown;
                element.PreviewMouseUp += WhenMouseUp;
                element.LostMouseCapture += WhenLostMouseCapture;
            }
            else
            {
                element.PreviewMouseDown -= WhenMouseDown;
                element.PreviewMouseUp -= WhenMouseUp;
                element.LostMouseCapture -= WhenLostMouseCapture;
            }
        }

        private static void WhenMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;

            if (e.ChangedButton != MouseButton.Right)
                return;

            if (!element.CaptureMouse())
                return;

            e.Handled = true;

            var adorner = GetAdorner(element);

            if (adorner == null)
            {
                var decorator = GetTopmostDecorator(element);
                adorner = new CardZoomAdorner(decorator, GetTargetControl(element) ?? element);
                SetAdorner(element, adorner);
            }

            Debug.Assert(adorner != null);
            adorner.FadeIn();
        }

        private static void WhenMouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;

            if (e.ChangedButton != MouseButton.Right)
                return;

            if (element.IsMouseCaptured)
                element.ReleaseMouseCapture();
        }

        private static void WhenLostMouseCapture(object sender, MouseEventArgs e)
        {
            var element = (UIElement)sender;
            var adorner = GetAdorner(element);

            if (adorner == null)
                return;

            adorner.FadeOut(() => DestroyAdorner(element));
        }

        private static void DestroyAdorner(UIElement element)
        {
            var adorner = GetAdorner(element);

            if (adorner != null)
            {
                adorner.Dispose();
                SetAdorner(element, null);
            }
        }

        private static AdornerDecorator GetTopmostDecorator(DependencyObject element)
        {
            AdornerDecorator decorator = null;

            for (DependencyObject parent = element; parent != null; parent = VisualTreeHelper.GetParent(parent))
            {
                if (parent is AdornerDecorator)
                    decorator = (AdornerDecorator)parent;
            }

            return decorator;
        }
    }
}
