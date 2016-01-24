using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI
{
    public class DirectionalViewbox : Viewbox
    {
        #region Dependency Properties

        public static DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof (Orientation), typeof (DirectionalViewbox), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion

        #region Properties

        protected ContainerVisual InternalVisual
        {
            get { return ((ContainerVisual)GetVisualChild(0)); }
        }

        #endregion

        #region Methods

        protected override Size MeasureOverride(Size constraint)
        {
            UIElement child = Child;
            Size size = new Size();
            if (child != null)
            {
                Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

                switch (Orientation)
                {
                    case Orientation.Vertical:
                        availableSize.Width = constraint.Width;
                        break;

                    case Orientation.Horizontal:
                        availableSize.Height = constraint.Height;
                        break;
                }

                child.Measure(availableSize);
                Size desiredSize = child.DesiredSize;
                Size factor = ComputeScaleFactor(constraint, desiredSize, Stretch, StretchDirection, Orientation);
                size.Width = factor.Width * desiredSize.Width;
                size.Height = factor.Height * desiredSize.Height;
            }
            return size;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement child = Child;
            if (child != null)
            {
                Size desiredSize = child.DesiredSize;
                Size factor = ComputeScaleFactor(arrangeSize, desiredSize, Stretch, StretchDirection, Orientation);
                InternalVisual.Transform = new ScaleTransform(factor.Width, factor.Height);
                child.Arrange(new Rect(new Point(), child.DesiredSize));
                arrangeSize.Width = factor.Width * desiredSize.Width;
                arrangeSize.Height = factor.Height * desiredSize.Height;
            }
            return arrangeSize;
        }

        private static double GetFactor(double availableSize, double contentSize, bool isOrientation)
        {
            if (isOrientation)
            {
                return 1;
            }

            return IsZero(contentSize) ? 0.0 : (availableSize / contentSize);
        }

        private static Size ComputeScaleFactor(Size availableSize, Size contentSize, Stretch stretch, StretchDirection direction, Orientation orientation)
        {
            if (stretch == Stretch.None || (double.IsPositiveInfinity(availableSize.Width) && double.IsPositiveInfinity(availableSize.Height)))
            {
                return new Size(1, 1);
            }

            double width = GetFactor(availableSize.Width, contentSize.Width, orientation == Orientation.Vertical);
            double height = GetFactor(availableSize.Height, contentSize.Height, orientation == Orientation.Horizontal);

            if (double.IsPositiveInfinity(availableSize.Width))
            {
                width = height;
            }
            else if (double.IsPositiveInfinity(availableSize.Height))
            {
                height = width;
            }

            switch (stretch)
            {
                case Stretch.Fill:
                    break;

                case Stretch.Uniform:
                    width = height = (width < height) ? width : height;
                    break;

                case Stretch.UniformToFill:
                    width = height = (width > height) ? width : height;
                    break;

                default:
                    throw new NotImplementedException();
            }

            switch (direction)
            {
                case StretchDirection.UpOnly:
                    width = Math.Max(1, width);
                    height = Math.Max(1, height);
                    break;

                case StretchDirection.DownOnly:
                    width = Math.Min(1, width);
                    height = Math.Min(1, height);
                    break;

                case StretchDirection.Both:
                    break;

                default:
                    throw new NotImplementedException();
            }

            return new Size(width, height);
        }

        private static bool IsZero(double value)
        {
            return (Math.Abs(value) < 2.2204460492503131E-15);
        }

        #endregion
    }

    public class StretchViewbox : Panel
    {
        #region Dependency Properties

        public static DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(StretchViewbox), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion

        private double scale;

        protected override Size MeasureOverride(Size availableSize)
        {
            double size = 0;
            Size unlimitedSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (UIElement child in Children)
            {
                child.Measure(unlimitedSize);
                size += GetScaledValue(child.DesiredSize);
            }

            if (IsZero(size))
            {
                scale = 1;
            }
            else
            {
                scale = GetScaledValue(availableSize) / size;
            }

            return availableSize;
        }

        private static bool IsZero(double value)
        {
            return (Math.Abs(value) < 2.2204460492503131E-15);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Transform scaleTransform = new ScaleTransform(scale, scale);
            double size = 0;
            foreach (UIElement child in Children)
            {
                child.RenderTransform = scaleTransform;

                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        child.Arrange(new Rect(new Point(scale * size, 0), new Size(child.DesiredSize.Width, finalSize.Height / scale)));
                        size += child.DesiredSize.Width;
                        break;

                    case Orientation.Vertical:
                        child.Arrange(new Rect(new Point(0, scale * size), new Size(finalSize.Width / scale, child.DesiredSize.Height)));
                        size += child.DesiredSize.Height;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            return finalSize;
        }

        private double GetScaledValue(Size size)
        {
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    return size.Width;

                case Orientation.Vertical:
                    return size.Height;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
