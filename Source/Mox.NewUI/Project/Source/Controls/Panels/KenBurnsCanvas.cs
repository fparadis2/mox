using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Mox.UI
{
    public abstract class KenBurnsCanvas : Canvas
    {
        #region Variables

        private readonly System.Random m_random = new System.Random();
        private readonly DispatcherTimer m_startCheckTimer = new DispatcherTimer();
        private DispatcherTimer m_contentChangeTimer;

        private FrameworkElement m_currentContent;
        private object m_nextContent;
        private object m_readyObject;

        #endregion

        #region Dependency Properties

        public static DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(KenBurnsCanvas), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(8)));
        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static DependencyProperty FadeDurationProperty = DependencyProperty.Register("FadeDuration", typeof(TimeSpan), typeof(KenBurnsCanvas), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(2)));
        public TimeSpan FadeDuration
        {
            get { return (TimeSpan)GetValue(FadeDurationProperty); }
            set { SetValue(FadeDurationProperty, value); }
        }
        
        public static DependencyProperty MinimumZoomProperty = DependencyProperty.Register("MinimumZoom", typeof(double), typeof(KenBurnsCanvas), new FrameworkPropertyMetadata(1.0));
        public double MinimumZoom
        {
            get { return (double)GetValue(MinimumZoomProperty); }
            set { SetValue(MinimumZoomProperty, value); }
        }

        public static DependencyProperty MaximumZoomProperty = DependencyProperty.Register("MaximumZoom", typeof(double), typeof(KenBurnsCanvas), new FrameworkPropertyMetadata(1.5));
        public double MaximumZoom
        {
            get { return (double)GetValue(MaximumZoomProperty); }
            set { SetValue(MaximumZoomProperty, value); }
        }

        public static DependencyProperty MaximumRotationProperty = DependencyProperty.Register("MaximumRotation", typeof(double), typeof(KenBurnsCanvas), new FrameworkPropertyMetadata(10.0));
        public double MaximumRotation
        {
            get { return (double)GetValue(MaximumRotationProperty); }
            set { SetValue(MaximumRotationProperty, value); }
        }

        #endregion

        #region Constructor

        protected KenBurnsCanvas()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Loaded += KenBurnsCanvas_Loaded;

                PrepareNextContent();
            }
        }

        #endregion

        #region Properties

        private TimeSpan TimeBetweenContentChange
        {
            get { return Duration - FadeDuration; }
        }

        #endregion

        #region Methods

        private bool StartFirstStoryboardIfReady()
        {
            if (IsReady(m_readyObject) && m_contentChangeTimer == null)
            {
                StartNextStoryboard();
                return true;
            }

            return false;
        }

        private void StartNextStoryboard()
        {
            if (!IsVisible)
            {
                return;
            }

            m_currentContent = CreateNewContent();
            Children.Add(m_currentContent);

            Storyboard storyboard = CreateStoryboard(m_currentContent);
            storyboard.Begin();

            Debug.Assert(m_contentChangeTimer == null);
            m_contentChangeTimer = new DispatcherTimer { Interval = TimeBetweenContentChange };
            m_contentChangeTimer.Tick += contentChangeTimerElapsed;
            m_contentChangeTimer.Start();
        }

        private Storyboard CreateStoryboard(FrameworkElement element)
        {
            Storyboard storyboard = new Storyboard();

            DoubleAnimation fadeInAnimation = CreateDoubleAnimation(element, OpacityProperty, 0, 1, FadeDuration);
            storyboard.Children.Add(fadeInAnimation);

            DoubleAnimation fadeOutAnimation = CreateDoubleAnimation(element, OpacityProperty, 1, 0, FadeDuration);
            fadeOutAnimation.BeginTime = TimeBetweenContentChange;
            fadeOutAnimation.Completed += (o, e) => Children.Remove(element);
            storyboard.Children.Add(fadeOutAnimation);

            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            AddMoveAnimations(storyboard, element);
            AddZoomRotateAnimation(storyboard, element);

            return storyboard;
        }

        private void AddMoveAnimations(Storyboard storyboard, FrameworkElement newContent)
        {
            Point halfSize = new Point(newContent.DesiredSize.Width / 2, newContent.DesiredSize.Height / 2);
            Rect allowedRect = new Rect(-halfSize.X, -halfSize.Y, ActualWidth, ActualHeight);

            Point initialPosition = PickRandomPoint(allowedRect);
            Point finalPosition = PickRandomPoint(allowedRect);

            storyboard.Children.Add(CreateDoubleAnimation(newContent, LeftProperty, initialPosition.X, finalPosition.X, Duration));
            storyboard.Children.Add(CreateDoubleAnimation(newContent, TopProperty, initialPosition.Y, finalPosition.Y, Duration));
        }

        private void AddZoomRotateAnimation(Storyboard storyboard, FrameworkElement newContent)
        {
            double zoomFactorX = newContent.DesiredSize.Width / ActualWidth;
            double zoomFactorY = newContent.DesiredSize.Height / ActualHeight;
            double zoomFactor = (zoomFactorX + zoomFactorY) / 2;

            double randomRotation = PickRandomRotation();
            double initialZoom = PickRandomZoom() / zoomFactor;
            double finalZoom = PickRandomZoom() / zoomFactor;

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform());
            transformGroup.Children.Add(new RotateTransform { Angle = randomRotation });
            newContent.RenderTransform = transformGroup;
            newContent.RenderTransformOrigin = new Point(0.5, 0.5);

            storyboard.Children.Add(CreateDoubleAnimation(newContent, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"), initialZoom, finalZoom, Duration));
            storyboard.Children.Add(CreateDoubleAnimation(newContent, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"), initialZoom, finalZoom, Duration));
        }

        private double PickRandomZoom()
        {
            return MinimumZoom + m_random.NextDouble() * (MaximumZoom - MinimumZoom);
        }

        private double PickRandomRotation()
        {
            return -MaximumRotation + m_random.NextDouble() * (MaximumRotation * 2);
        }

        private Point PickRandomPoint(Rect rect)
        {
            return new Point
            {
                X = rect.Left + m_random.NextDouble() * rect.Width,
                Y = rect.Top + m_random.NextDouble() * rect.Height,
            };
        }

        private static DoubleAnimation CreateDoubleAnimation(DependencyObject element, DependencyProperty property, double initialValue, double finalValue, Duration duration)
        {
            return CreateDoubleAnimation(element, new PropertyPath(property), initialValue, finalValue, duration);
        }

        private static DoubleAnimation CreateDoubleAnimation(DependencyObject element, PropertyPath property, double initialValue, double finalValue, Duration duration)
        {
            var da = new DoubleAnimation
            {
                Duration = duration,
                From = initialValue,
                To = finalValue
            };

            Storyboard.SetTarget(da, element);
            Storyboard.SetTargetProperty(da, property);

            return da;
        }

        private FrameworkElement CreateNewContent()
        {
            Debug.Assert(m_nextContent != null);
            object nextContent = m_nextContent;

            PrepareNextContent();

            return new ContentPresenter
            {
                Content = nextContent
            };
        }

        private void PrepareNextContent()
        {
            m_nextContent = GetNextContent(out m_readyObject);
        }

        protected abstract object GetNextContent(out object readyObject);

        protected virtual bool IsReady(object readyObject)
        {
            return true;
        }

        #endregion

        #region Event Handlers

        void m_startCheckTimer_Elapsed(object sender, EventArgs e)
        {
            if (StartFirstStoryboardIfReady())
            {
                m_startCheckTimer.Stop();
            }
        }

        void KenBurnsCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (!StartFirstStoryboardIfReady())
            {
                m_startCheckTimer.Interval = TimeSpan.FromMilliseconds(200);
                m_startCheckTimer.Tick += m_startCheckTimer_Elapsed;
                m_startCheckTimer.Start();
            }
        }

        void contentChangeTimerElapsed(object sender, EventArgs e)
        {
            var contentChangeTimer = m_contentChangeTimer;
            if (contentChangeTimer != null)
            {
                contentChangeTimer.Tick -= contentChangeTimerElapsed;
                contentChangeTimer.Stop();
                m_contentChangeTimer = null;

                StartNextStoryboard();
            }
        }

        #endregion
    }
}
