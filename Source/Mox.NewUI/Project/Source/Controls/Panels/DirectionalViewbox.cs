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
}
