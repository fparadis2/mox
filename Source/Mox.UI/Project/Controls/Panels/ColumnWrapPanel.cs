using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class ColumnWrapPanel : Panel
    {
        public static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register(
            "MaxColumns", typeof (int), typeof (ColumnWrapPanel), new PropertyMetadata(2));

        public int MaxColumns
        {
            get { return (int) GetValue(MaxColumnsProperty); }
            set { SetValue(MaxColumnsProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            foreach (UIElement child in InternalChildren)
                child.Measure(new Size(double.MaxValue, double.MaxValue));

            Size totalSize;
            Size idealSize = ComputeIdealSize(constraint, out totalSize);

            Size panelSize = new Size(idealSize.Width, 0);

            double columnHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                columnHeight += child.DesiredSize.Height;

                if (columnHeight >= idealSize.Height)
                {
                    panelSize.Height = Math.Max(panelSize.Height, columnHeight);
                    columnHeight = 0;
                }
            }

            panelSize.Height = Math.Max(panelSize.Height, columnHeight);
            return panelSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size totalSize;
            Size idealSize = ComputeIdealSize(finalSize, out totalSize);

            double x = 0;
            double y = 0;

            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(new Rect(x, y, totalSize.Width, child.DesiredSize.Height));

                y += child.DesiredSize.Height;

                if (y >= idealSize.Height)
                {
                    x += totalSize.Width;
                    y = 0;
                }
            }

            return finalSize;
        }

        private Size ComputeIdealSize(Size availableSize, out Size totalSize)
        {
            totalSize = new Size();

            foreach (UIElement child in InternalChildren)
            {
                Size childSize = child.DesiredSize;

                totalSize.Width = Math.Max(totalSize.Width, childSize.Width);
                totalSize.Height += childSize.Height;
            }

            int numColumns;

            if (totalSize.Width * MaxColumns <= availableSize.Width)
            {
                numColumns = MaxColumns;
            }
            else
            {
                numColumns = (int)Math.Floor(availableSize.Width / totalSize.Width);
            }

            numColumns = Math.Max(1, numColumns);
            return new Size(numColumns * totalSize.Width, totalSize.Height / numColumns);
        }
    }
}
