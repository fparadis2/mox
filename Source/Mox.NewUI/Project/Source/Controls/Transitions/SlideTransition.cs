using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mox.UI
{
    public class SlideTransition : Transition
    {
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

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(Duration), typeof(SlideTransition), new UIPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200))));

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
            Point endPoint = ComputeEndPoint(transitionPresenter);
            
            TranslateTransform newContentTransform = new TranslateTransform(endPoint.X, endPoint.Y);
            TranslateTransform oldContentTransform = new TranslateTransform(0, 0);

            newContent.RenderTransform = newContentTransform;
            oldContent.RenderTransform = oldContentTransform;

            TranslateTo(oldContentTransform, endPoint, true, 
                () => TranslateTo(newContentTransform, new Point(0, 0), false,
                    () => EndTransition(transitionPresenter, oldContent, newContent)));
        }

        protected override void OnTransitionEnded(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent)
        {
            newContent.ClearValue(ContentPresenter.RenderTransformProperty);
            oldContent.ClearValue(ContentPresenter.RenderTransformProperty);
        }

        private Point ComputeEndPoint(TransitionPresenter presenter)
        {
            Point relative = ComputeRelativeEndPoint();

            return new Point(relative.X * presenter.ActualWidth, relative.Y * presenter.ActualHeight);
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
            DoubleAnimation da = new DoubleAnimation(endPoint.X, Duration)
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

        #endregion
    }
}