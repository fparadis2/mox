using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mox.UI
{
    public abstract class PolaroidStackCanvas : Canvas
    {
        #region Variables

        private readonly System.Random m_random = new System.Random();
        private readonly DispatcherTimer m_waitForReadyTimer = new DispatcherTimer();
        private readonly DispatcherTimer m_contentChangeTimer = new DispatcherTimer();

        private FrameworkElement m_currentContent;
        private object m_nextContent;

        #endregion

        #region Dependency Properties

        public static DependencyProperty MaximumChildrenCountProperty = DependencyProperty.Register("MaximumChildrenCount", typeof(int), typeof(PolaroidStackCanvas), new FrameworkPropertyMetadata(100));
        public int MaximumChildrenCount
        {
            get { return (int)GetValue(MaximumChildrenCountProperty); }
            set { SetValue(MaximumChildrenCountProperty, value); }
        }

        /// <summary>
        /// Time for an element to get to its final position
        /// </summary>
        public static DependencyProperty MovementDurationProperty = DependencyProperty.Register("MovementDuration", typeof(TimeSpan), typeof(PolaroidStackCanvas), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(1.5)));
        public TimeSpan MovementDuration
        {
            get { return (TimeSpan)GetValue(MovementDurationProperty); }
            set { SetValue(MovementDurationProperty, value); }
        }

        public static DependencyProperty WaitDurationProperty = DependencyProperty.Register("WaitDuration", typeof(TimeSpan), typeof(PolaroidStackCanvas), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(0.75)));
        public TimeSpan WaitDuration
        {
            get { return (TimeSpan)GetValue(WaitDurationProperty); }
            set { SetValue(WaitDurationProperty, value); }
        }

        public static DependencyProperty MaximumInitialRotationProperty = DependencyProperty.Register("MaximumInitialRotation", typeof(double), typeof(PolaroidStackCanvas), new FrameworkPropertyMetadata(180.0));
        public double MaximumInitialRotation
        {
            get { return (double)GetValue(MaximumInitialRotationProperty); }
            set { SetValue(MaximumInitialRotationProperty, value); }
        }

        public static DependencyProperty MaximumFinalRotationProperty = DependencyProperty.Register("MaximumFinalRotation", typeof(double), typeof(PolaroidStackCanvas), new FrameworkPropertyMetadata(20.0));
        public double MaximumFinalRotation
        {
            get { return (double)GetValue(MaximumFinalRotationProperty); }
            set { SetValue(MaximumFinalRotationProperty, value); }
        }

        #endregion

        #region Constructor

        protected PolaroidStackCanvas()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                m_waitForReadyTimer.Interval = TimeSpan.FromMilliseconds(200);
                m_waitForReadyTimer.Tick += m_waitForReadyTimer_Elapsed;

                m_contentChangeTimer = new DispatcherTimer { Interval = MovementDuration + WaitDuration };
                m_contentChangeTimer.Tick += m_contentChangeTimer_Elapsed;

                m_nextContent = PrepareNextContent();
                m_waitForReadyTimer.Start();
            }
        }

        #endregion

        #region Methods

        protected abstract object PrepareNextContent();

        protected virtual bool IsContentReady(object content)
        {
            return true;
        }

        protected abstract FrameworkElement CreateContainer(object content);

        private bool StartFirstStoryboardIfReady()
        {
            if (!IsVisible)
            {
                return false;
            }

            if (IsContentReady(m_nextContent))
            {
                StartNextStoryboard();
                return true;
            }

            return false;
        }

        private void StartNextStoryboard()
        {
            m_currentContent = CreateContainer(m_nextContent);
            m_nextContent = PrepareNextContent();

            if (Children.Count >= MaximumChildrenCount && Children.Count > 0)
            {
                Children.RemoveAt(0);
            }

            Children.Add(m_currentContent);
            m_currentContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Storyboard storyboard = CreateStoryboard(m_currentContent);
            storyboard.Begin();

            m_contentChangeTimer.Start();
        }

        private Storyboard CreateStoryboard(FrameworkElement element)
        {
            Storyboard storyboard = new Storyboard();

            AddMoveAnimations(storyboard, element);
            AddRotateAnimation(storyboard, element);

            return storyboard;
        }

        private void AddMoveAnimations(Storyboard storyboard, FrameworkElement newContent)
        {
            Point halfSize = new Point(newContent.DesiredSize.Width / 2, newContent.DesiredSize.Height / 2);
            Rect allowedRect = new Rect(-halfSize.X, -halfSize.Y, ActualWidth, ActualHeight);

            Point finalPosition;
            Point initialPosition;
            FindInitialAndFinalPositions(allowedRect, newContent.DesiredSize, out initialPosition, out finalPosition);

            storyboard.Children.Add(CreateDoubleAnimation(newContent, LeftProperty, initialPosition.X, finalPosition.X, MovementDuration));
            storyboard.Children.Add(CreateDoubleAnimation(newContent, TopProperty, initialPosition.Y, finalPosition.Y, MovementDuration));
        }

        private void FindInitialAndFinalPositions(Rect bounds, Size elementSize, out Point initialPosition, out Point finalPosition)
        {
            Rect finalBounds = bounds;
            finalBounds.Inflate(finalBounds.Width * -0.1, finalBounds.Height * -0.1);
            finalPosition = PickRandomPoint(bounds);

            bounds.Inflate(elementSize.Width * 2, elementSize.Height * 2);

            Vector boundsCenter = new Vector(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);

            // Fit an ellipse on the bounds
            double ellipseWidth = bounds.Width / Math.Sqrt(2);
            double ellipseHeight = bounds.Height / Math.Sqrt(2);

            // Pick a random point on the ellipse
            double randomAngle = m_random.NextDouble() * 2 * Math.PI;
            Point initialPosition1 = new Point(ellipseWidth / 2 * Math.Cos(randomAngle), ellipseHeight / 2 * Math.Sin(randomAngle)) + boundsCenter;
            Vector delta1 = initialPosition1 - finalPosition;

            randomAngle += Math.PI;
            Point initialPosition2 = new Point(ellipseWidth / 2 * Math.Cos(randomAngle), ellipseHeight / 2 * Math.Sin(randomAngle)) + boundsCenter;
            Vector delta2 = initialPosition2 - finalPosition;

            initialPosition = delta1.LengthSquared < delta2.LengthSquared ? initialPosition2 : initialPosition1;
        }

        private void AddRotateAnimation(Storyboard storyboard, FrameworkElement newContent)
        {
            double initialRotation = PickRandomRotation(MaximumInitialRotation);
            double finalRotation = PickRandomRotation(MaximumFinalRotation);

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new RotateTransform());
            newContent.RenderTransform = transformGroup;
            newContent.RenderTransformOrigin = new Point(0.5, 0.5);

            storyboard.Children.Add(CreateDoubleAnimation(newContent, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)"), initialRotation, finalRotation, MovementDuration));
        }

        private double PickRandomRotation(double maxRotation)
        {
            return -maxRotation + m_random.NextDouble() * (maxRotation * 2);
        }

        private Point PickRandomPoint(Rect rect)
        {
            return new Point
            {
                X = rect.Left + m_random.NextDouble() * rect.Width,
                Y = rect.Top + m_random.NextDouble() * rect.Height,
            };
        }

        private static readonly IEasingFunction ms_easingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 3 };

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
                To = finalValue,
                EasingFunction = ms_easingFunction 
            };

            Storyboard.SetTarget(da, element);
            Storyboard.SetTargetProperty(da, property);

            return da;
        }

        #endregion

        #region Event Handlers

        void m_waitForReadyTimer_Elapsed(object sender, EventArgs e)
        {
            if (StartFirstStoryboardIfReady())
            {
                m_waitForReadyTimer.Stop();
            }
        }

        void m_contentChangeTimer_Elapsed(object sender, EventArgs e)
        {
            m_contentChangeTimer.Stop();
            m_waitForReadyTimer.Start();
        }

        #endregion
    }
}
