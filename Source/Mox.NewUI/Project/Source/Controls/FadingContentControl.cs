using System;

using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Mox.UI
{
    [ContentProperty("NewContent")]
    public class FadingContentControl : ContentControl
    {
        #region FadeDuration

        public static readonly DependencyProperty FadeDurationProperty = DependencyProperty.Register("FadeDuration", typeof(Duration), typeof(FadingContentControl), new FrameworkPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(100))));

        /// <summary>
        /// FadeDuration will be used as the duration for Fade Out and Fade In animations
        /// </summary>
        public Duration FadeDuration
        {
            get { return (Duration)GetValue(FadeDurationProperty); }
            set { SetValue(FadeDurationProperty, value); }
        }

        public static readonly DependencyProperty NewContentProperty = DependencyProperty.Register("NewContent", typeof(object), typeof(FadingContentControl), new FrameworkPropertyMetadata(null, OnNewContentChanged));

        private static void OnNewContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FadingContentControl)d).OnNewContentChanged(e.NewValue);
        }

        public object NewContent
        {
            get { return GetValue(NewContentProperty); }
            set { SetValue(NewContentProperty, value); }
        }

        #endregion

        #region Methods

        private void OnNewContentChanged(object newContent)
        {
            if (Content == null)
            {
                Content = newContent;
                return;
            }

            IsHitTestVisible = false;
            DoubleAnimation da = new DoubleAnimation(0, FadeDuration) { DecelerationRatio = 1 };
            
            da.Completed += (o, e) =>
            {
                Content = newContent;
                IsHitTestVisible = true;

                FadeIn();
            };

            BeginAnimation(OpacityProperty, da);
        }

        private void FadeIn()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)(() =>
            {
                DoubleAnimation da = new DoubleAnimation(1, FadeDuration) { AccelerationRatio = 1 };
                BeginAnimation(OpacityProperty, da);
            }));
        }

        #endregion
    }
}
