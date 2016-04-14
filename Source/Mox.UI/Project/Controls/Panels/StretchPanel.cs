using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    /// <summary>
    /// Like a StackPanel but stretches all elements equally to fill the available space
    /// </summary>
    public class StretchPanel : Panel
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(StretchPanel), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Orientation Orientation
        {
            get { return (Orientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private Size m_measuredSize;

        protected override Size MeasureOverride(Size availableSize)
        {
            var orientation = Orientation;

            m_measuredSize = new Size();

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);

                switch (orientation)
                {
                    case Orientation.Horizontal:
                    {
                        m_measuredSize.Width += child.DesiredSize.Width;
                        m_measuredSize.Height = Math.Max(m_measuredSize.Height, child.DesiredSize.Height);
                        break;
                    }
                    case Orientation.Vertical:
                    {
                        m_measuredSize.Width = Math.Max(m_measuredSize.Width, child.DesiredSize.Width);
                        m_measuredSize.Height += child.DesiredSize.Height;
                        break;
                    }
                    default:
                        throw new NotImplementedException();
                }
            }

            return m_measuredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var orientation = Orientation;

            double position = 0;
            double stretchFactor = GetStretchFactor(finalSize, orientation, m_measuredSize);
            
            Size arrangedSize = new Size();

            foreach (UIElement child in Children)
            {
                switch (orientation)
                {
                    case Orientation.Horizontal:
                    {
                        double width = child.DesiredSize.Width * stretchFactor;
                        double height = child.DesiredSize.Height;

                        child.Arrange(new Rect(position, 0, width, height));
                        position += width;
                        
                        arrangedSize.Width += width;
                        arrangedSize.Height = Math.Max(arrangedSize.Height, height);
                        
                        break;
                    }
                    case Orientation.Vertical:
                    {
                        double width = child.DesiredSize.Width;
                        double height = child.DesiredSize.Height * stretchFactor;

                        child.Arrange(new Rect(0, position, width, height));
                        position += height;

                        arrangedSize.Width = Math.Max(arrangedSize.Width, width);
                        arrangedSize.Height += height;

                        break;
                    }
                    default:
                        throw new NotImplementedException();
                }
            }

            return arrangedSize;
        }

        private static double GetStretchFactor(Size finalSize, Orientation orientation, Size measuredSize)
        {
            double stretchFactor;
            switch (orientation)
            {
                case Orientation.Horizontal:
                {
                    if (measuredSize.Width <= 0)
                        return 1;

                    stretchFactor = finalSize.Width / measuredSize.Width;
                    break;
                }
                case Orientation.Vertical:
                {
                    if (measuredSize.Height <= 0)
                        return 1;

                    stretchFactor = finalSize.Height / measuredSize.Height;
                    break;
                }
                default:
                    throw new NotImplementedException();
            }

            if (stretchFactor < 1)
                stretchFactor = 1;

            return stretchFactor;
        }
    }
}
