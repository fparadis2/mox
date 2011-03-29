using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Mox.UI
{
    public class FadeTransition : Transition
    {
        static FadeTransition()
        {
            IsNewContentTopmostProperty.OverrideMetadata(typeof(FadeTransition), new FrameworkPropertyMetadata(false));
        }

        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(Duration), typeof(FadeTransition), new UIPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200))));

        protected internal override void BeginTransition(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(0, Duration) { DecelerationRatio = 1 };
            fadeOut.Completed += delegate
            {
                DoubleAnimation fadeIn = new DoubleAnimation(1, Duration) { AccelerationRatio = 1 };

                fadeIn.Completed += delegate
                {
                    EndTransition(transitionElement, oldContent, newContent);
                };
                newContent.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            oldContent.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}