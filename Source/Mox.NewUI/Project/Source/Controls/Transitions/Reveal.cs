using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Mox.UI
{
    public class Reveal : Decorator
    {
        #region Constructors

        static Reveal()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(Reveal), new FrameworkPropertyMetadata(true));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Whether the child is expanded or not.
        /// An animation may be in progress when the value changes.
        /// This is not meant to be used with AnimationProgress and can overwrite any
        /// animation or values in that property.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Reveal), new UIPropertyMetadata(false, OnIsExpandedChanged));

        private static void OnIsExpandedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((Reveal)sender).SetupAnimation((bool)e.NewValue);
        }

        /// <summary>
        /// The duration in milliseconds of the reveal animation.
        /// Will apply to the next animation that occurs (not to currently running animations).
        /// </summary>
        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(double), typeof(Reveal), new UIPropertyMetadata(250.0));

        public HorizontalRevealMode HorizontalReveal
        {
            get { return (HorizontalRevealMode)GetValue(HorizontalRevealProperty); }
            set { SetValue(HorizontalRevealProperty, value); }
        }

        public static readonly DependencyProperty HorizontalRevealProperty = DependencyProperty.Register("HorizontalReveal", typeof(HorizontalRevealMode), typeof(Reveal), new UIPropertyMetadata(HorizontalRevealMode.LeftToRight));

        public VerticalRevealMode VerticalReveal
        {
            get { return (VerticalRevealMode)GetValue(VerticalRevealProperty); }
            set { SetValue(VerticalRevealProperty, value); }
        }

        public static readonly DependencyProperty VerticalRevealProperty = DependencyProperty.Register("VerticalReveal", typeof(VerticalRevealMode), typeof(Reveal), new UIPropertyMetadata(VerticalRevealMode.None));

        private double AnimationProgress
        {
            get { return (double)GetValue(AnimationProgressProperty); }
        }

        private static readonly DependencyProperty AnimationProgressProperty = DependencyProperty.Register("AnimationProgress", typeof(double), typeof(Reveal), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, null, OnCoerceAnimationProgress));

        private static object OnCoerceAnimationProgress(DependencyObject d, object baseValue)
        {
            double num = (double)baseValue;
            
            if (num < 0.0)
            {
                return 0.0;
            }

            if (num > 1.0)
            {
                return 1.0;
            }

            return baseValue;
        }

        #endregion

        #region Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            UIElement child = Child;
            if (child != null)
            {
                child.Measure(availableSize);

                double percent = AnimationProgress;
                double width = CalculateWidth(child.DesiredSize.Width, percent, HorizontalReveal);
                double height = CalculateHeight(child.DesiredSize.Height, percent, VerticalReveal);
                return new Size(width, height);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UIElement child = Child;
            if (child != null)
            {
                double percent = AnimationProgress;
                HorizontalRevealMode horizontalReveal = HorizontalReveal;
                VerticalRevealMode verticalReveal = VerticalReveal;

                double childWidth = child.DesiredSize.Width;
                double childHeight = child.DesiredSize.Height;
                double x = CalculateLeft(childWidth, percent, horizontalReveal);
                double y = CalculateTop(childHeight, percent, verticalReveal);

                child.Arrange(new Rect(x, y, childWidth, childHeight));

                childWidth = child.RenderSize.Width;
                childHeight = child.RenderSize.Height;
                double width = CalculateWidth(childWidth, percent, horizontalReveal);
                double height = CalculateHeight(childHeight, percent, verticalReveal);
                return new Size(width, height);
            }

            return new Size();
        }

        private static double CalculateLeft(double width, double percent, HorizontalRevealMode reveal)
        {
            if (reveal == HorizontalRevealMode.RightToLeft)
            {
                return (percent - 1.0) * width;
            }
            
            if (reveal == HorizontalRevealMode.CenterToEdge)
            {
                return (percent - 1.0) * width * 0.5;
            }

            return 0.0;
        }

        private static double CalculateTop(double height, double percent, VerticalRevealMode reveal)
        {
            if (reveal == VerticalRevealMode.BottomToTop)
            {
                return (percent - 1.0) * height;
            }
            
            if (reveal == VerticalRevealMode.CenterToEdge)
            {
                return (percent - 1.0) * height * 0.5;
            }

            return 0.0;
        }

        private static double CalculateWidth(double originalWidth, double percent, HorizontalRevealMode reveal)
        {
            if (reveal == HorizontalRevealMode.None)
            {
                return originalWidth;
            }

            return originalWidth * percent;
        }

        private static double CalculateHeight(double originalHeight, double percent, VerticalRevealMode reveal)
        {
            if (reveal == VerticalRevealMode.None)
            {
                return originalHeight;
            }

            return originalHeight * percent;
        }

        private void SetupAnimation(bool isExpanded)
        {
            // Adjust the time if the animation is already in progress
            double currentProgress = AnimationProgress;
            if (isExpanded)
            {
                currentProgress = 1.0 - currentProgress;
            }

            DoubleAnimation animation = new DoubleAnimation
            {
                To = isExpanded ? 1.0 : 0.0,
                Duration = TimeSpan.FromMilliseconds(Duration * currentProgress),
                FillBehavior = FillBehavior.HoldEnd
            };

            BeginAnimation(AnimationProgressProperty, animation);
        }

        #endregion
    }

    public enum HorizontalRevealMode
    {
        /// <summary>
        /// No reveal animation.
        /// </summary>
        None,

        /// <summary>
        /// Reveal from the left to the right.
        /// </summary>
        LeftToRight,

        /// <summary>
        /// Reveal from the right to the left.
        /// </summary>
        RightToLeft,

        /// <summary>
        /// Reveal from the center to the bounding edge.
        /// </summary>
        CenterToEdge,
    }

    public enum VerticalRevealMode
    {
        /// <summary>
        /// No reveal animation.
        /// </summary>
        None,

        /// <summary>
        /// Reveal from the top to the bottom.
        /// </summary>
        TopToBottom,

        /// <summary>
        /// Reveal from the bottom to the top.
        /// </summary>
        BottomToTop,

        /// <summary>
        /// Reveal from the center to the bounding edge.
        /// </summary>
        CenterToEdge,
    }
}
