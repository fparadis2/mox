using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mox.UI.Game
{
    [TemplatePart(Name = "PART_SelectedItemIndicator", Type = typeof(FrameworkElement))]
    public class StepsItemsControl : Selector
    {
        #region Dependency Properties

        public static readonly DependencyProperty FadeDurationProperty = DependencyProperty.Register(
            "FadeDuration", typeof (TimeSpan), typeof (StepsItemsControl), new PropertyMetadata(TimeSpan.FromSeconds(0.25)));

        public TimeSpan FadeDuration
        {
            get { return (TimeSpan) GetValue(FadeDurationProperty); }
            set { SetValue(FadeDurationProperty, value); }
        }

        public static readonly DependencyProperty MoveDurationProperty = DependencyProperty.Register(
            "MoveDuration", typeof(TimeSpan), typeof(StepsItemsControl), new PropertyMetadata(TimeSpan.FromSeconds(0.5)));

        public TimeSpan MoveDuration
        {
            get { return (TimeSpan)GetValue(MoveDurationProperty); }
            set { SetValue(MoveDurationProperty, value); }
        }

        #endregion

        private FrameworkElement m_selectedItemIndicator;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_selectedItemIndicator = GetTemplateChild("PART_SelectedItemIndicator") as FrameworkElement;

            if (m_selectedItemIndicator != null)
            {
                m_selectedItemIndicator.HorizontalAlignment = HorizontalAlignment.Left;
                m_selectedItemIndicator.Opacity = 0;
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (m_selectedItemIndicator == null)
                return;

            if (e.RemovedItems.Count > 0 && e.AddedItems.Count > 0)
            {
                MoveTo(SelectedIndex);
            }
            else if (e.AddedItems.Count > 0)
            {
                FadeIn(SelectedIndex);
            }
            else
            {
                FadeOut();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (SelectedIndex >= 0 && m_selectedItemIndicator != null)
            {
                Rect bounds;
                if (!GetBounds(SelectedIndex, out bounds))
                    return;

                m_selectedItemIndicator.ApplyAnimationClock(MarginProperty, null);
                m_selectedItemIndicator.ApplyAnimationClock(WidthProperty, null);

                m_selectedItemIndicator.Margin = new Thickness(bounds.Left, 0, 0, 0);
                m_selectedItemIndicator.Width = bounds.Width;
            }
        }

        private void MoveTo(int selectedIndex)
        {
            Rect bounds;
            if (!GetBounds(selectedIndex, out bounds))
                return;

            m_selectedItemIndicator.BeginAnimation(MarginProperty, MakeAnimation(new Thickness(bounds.Left, 0, 0, 0), MoveDuration));
            m_selectedItemIndicator.BeginAnimation(WidthProperty, MakeAnimation(bounds.Width, MoveDuration));
        }

        private void FadeIn(int selectedIndex)
        {
            Rect bounds;
            if (!GetBounds(selectedIndex, out bounds))
                return;

            m_selectedItemIndicator.ApplyAnimationClock(MarginProperty, null);
            m_selectedItemIndicator.ApplyAnimationClock(WidthProperty, null);

            m_selectedItemIndicator.Margin = new Thickness(bounds.Left, 0, 0, 0);
            m_selectedItemIndicator.Width = bounds.Width;

            m_selectedItemIndicator.BeginAnimation(OpacityProperty, MakeAnimation(1, FadeDuration));
        }

        private void FadeOut()
        {
            m_selectedItemIndicator.BeginAnimation(OpacityProperty, MakeAnimation(0, FadeDuration));
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

        private static ThicknessAnimation MakeAnimation(Thickness to, TimeSpan duration)
        {
            ThicknessAnimation anim = new ThicknessAnimation(to, duration)
            {
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.7
            };

            return anim;
        }

        private bool GetBounds(int index, out Rect bounds)
        {
            Debug.Assert(index >= 0);
            FrameworkElement container = ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;

            if (container == null)
            {
                bounds = new Rect();
                return false;
            }

            var transform = container.TransformToAncestor(this);
            bounds = new Rect(0, 0, container.ActualWidth, container.ActualHeight);

            bounds = transform.TransformBounds(bounds);
            return true;
        }
    }
}
