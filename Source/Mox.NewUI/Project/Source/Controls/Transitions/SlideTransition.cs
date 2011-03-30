using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mox.UI
{
    public class SlideTransition : Transition
    {
        #region Variables

        private static readonly System.Random ms_random = new System.Random();

        #endregion

        #region Dependency Properties

        static SlideTransition()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(SlideTransition), new FrameworkPropertyMetadata(true));
        }

        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(Duration), typeof(SlideTransition), new UIPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(400))));

        public float DurationRandomDeviation
        {
            get { return (float)GetValue(RandomDeviationDurationProperty); }
            set { SetValue(RandomDeviationDurationProperty, value); }
        }

        public static readonly DependencyProperty RandomDeviationDurationProperty = DependencyProperty.Register("DurationRandomDeviation", typeof(float), typeof(SlideTransition), new UIPropertyMetadata(0.2f));

        public Duration OffDuration
        {
            get { return (Duration)GetValue(OffDurationProperty); }
            set { SetValue(OffDurationProperty, value); }
        }

        public static readonly DependencyProperty OffDurationProperty = DependencyProperty.Register("OffDuration", typeof(Duration), typeof(SlideTransition), new UIPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(100))));

        public Dock SlideDirection
        {
            get { return (Dock)GetValue(SlideDirectionProperty); }
            set { SetValue(SlideDirectionProperty, value); }
        }

        public static readonly DependencyProperty SlideDirectionProperty = DependencyProperty.Register("SlideDirection", typeof(Dock), typeof(SlideTransition), new UIPropertyMetadata(Dock.Left));

        #endregion

        #region Methods

        protected internal override void BeginTransition(TransitionPresenter transitionPresenter, ContentPresenter oldContent, ContentPresenter newContent)
        {
            Point endPoint = ComputeEndPoint(oldContent);

            TranslateTransform newContentTransform = new TranslateTransform(endPoint.X, endPoint.Y);
            TranslateTransform oldContentTransform = new TranslateTransform(0, 0);

            newContent.RenderTransform = newContentTransform;
            oldContent.RenderTransform = oldContentTransform;

            TranslateTo(oldContentTransform, endPoint, true, () =>
            {
                MidTransition(transitionPresenter, oldContent, newContent);

                endPoint = ComputeEndPoint(newContent);
                newContentTransform = new TranslateTransform(endPoint.X, endPoint.Y);
                newContent.RenderTransform = newContentTransform;
                oldContent.RenderTransform = new TranslateTransform(endPoint.X, endPoint.Y);

                TranslateTo(newContentTransform, new Point(0, 0), false, 
                    () => EndTransition(transitionPresenter, oldContent, newContent));
            });
        }

        protected override void OnTransitionEnded(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent)
        {
            newContent.ClearValue(ContentPresenter.RenderTransformProperty);
            oldContent.ClearValue(ContentPresenter.RenderTransformProperty);
        }

        private Point ComputeEndPoint(FrameworkElement referenceControl)
        {
            Point relative = ComputeRelativeEndPoint();

            return new Point(relative.X * referenceControl.ActualWidth, relative.Y * referenceControl.ActualHeight);
        }

        private Point ComputeRelativeEndPoint()
        {
            const double Extent = 1.5;

            switch (SlideDirection)
            {
                case Dock.Left:
                    return new Point(-Extent, 0);

                case Dock.Right:
                    return new Point(+Extent, 0);

                case Dock.Top:
                    return new Point(0, -Extent);

                case Dock.Bottom:
                    return new Point(0, +Extent);

                default:
                    throw new NotImplementedException();
            }
        }

        private void TranslateTo(TranslateTransform transform, Point endPoint, bool goingOut, System.Action completedAction)
        {
            var duration = ComputeRandomDuration();
            DoubleAnimation da = new DoubleAnimation(endPoint.X, duration)
            {
                EasingFunction = new CubicEase
                {
                    EasingMode = goingOut ? EasingMode.EaseIn : EasingMode.EaseOut
                }
            };

            if (!goingOut)
            {
                da.BeginTime = OffDuration.TimeSpan;
            }

            transform.BeginAnimation(TranslateTransform.XProperty, da);

            da.To = endPoint.Y;

            if (completedAction != null)
            {
                da.Completed += delegate
                {
                    completedAction();
                };
            }

            transform.BeginAnimation(TranslateTransform.YProperty, da);
        }

        private Duration ComputeRandomDuration()
        {
            TimeSpan duration = Duration.TimeSpan;
            TimeSpan randomDeviation = TimeSpan.FromMilliseconds(duration.TotalMilliseconds * DurationRandomDeviation * (ms_random.NextDouble() * 2.0 - 1.0));
            return duration + randomDeviation;
        }

        #endregion
    }
}