using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class WrapPanel : Panel
    {
        public static readonly DependencyProperty HorizontalContentAlignmentProperty = DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(WrapPanel), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

        public HorizontalAlignment HorizontalContentAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size currentLineSize = new Size();
            Size panelSize = new Size();

            foreach (UIElement child in InternalChildren)
            {
                // Flow passes its own constraint to children
                child.Measure(constraint);
                Size childSize = child.DesiredSize;

                if (currentLineSize.Width + childSize.Width > constraint.Width) // need to switch to another line
                {
                    panelSize.Width = Math.Max(currentLineSize.Width, panelSize.Width);
                    panelSize.Height += currentLineSize.Height;
                    currentLineSize = childSize;

                    if (childSize.Width > constraint.Width) // if the element is wider then the constraint - give it a separate line                    
                    {
                        panelSize.Width = Math.Max(childSize.Width, panelSize.Width);
                        panelSize.Height += childSize.Height;
                        currentLineSize = new Size();
                    }
                }
                else // continue to accumulate a line
                {
                    currentLineSize.Width += childSize.Width;
                    currentLineSize.Height = Math.Max(childSize.Height, currentLineSize.Height);
                }
            }

            // the last line size, if any need to be added
            panelSize.Width = Math.Max(currentLineSize.Width, panelSize.Width);
            panelSize.Height += currentLineSize.Height;

            return panelSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            int firstInLine = 0;
            Size currentLineSize = new Size();
            double accumulatedHeight = 0;

            UIElementCollection children = InternalChildren;

            for (int i = 0; i < children.Count; i++)
            {
                Size childSize = children[i].DesiredSize;

                if (currentLineSize.Width + childSize.Width > arrangeBounds.Width) // need to switch to another line
                {
                    ArrangeLine(accumulatedHeight, currentLineSize, arrangeBounds.Width, firstInLine, i);

                    accumulatedHeight += currentLineSize.Height;
                    currentLineSize = childSize;

                    if (childSize.Width > arrangeBounds.Width) // the element is wider then the constraint - give it a separate line                    
                    {
                        ArrangeLine(accumulatedHeight, childSize, arrangeBounds.Width, i, ++i);
                        accumulatedHeight += childSize.Height;
                        currentLineSize = new Size();
                    }
                    firstInLine = i;
                }
                else // continue to accumulate a line
                {
                    currentLineSize.Width += childSize.Width;
                    currentLineSize.Height = Math.Max(childSize.Height, currentLineSize.Height);
                }
            }

            if (firstInLine < children.Count)
                ArrangeLine(accumulatedHeight, currentLineSize, arrangeBounds.Width, firstInLine, children.Count);

            return arrangeBounds;
        }

        private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
        {
            double x = GetLineStart(lineSize.Width, boundsWidth);

            UIElementCollection children = InternalChildren;
            for (int i = start; i < end; i++)
            {
                UIElement child = children[i];
                child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));
                x += child.DesiredSize.Width;
            }
        }

        private double GetLineStart(double lineWidth, double boundsWidth)
        {
            switch (HorizontalContentAlignment)
            {
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Stretch:
                    return 0;

                case HorizontalAlignment.Center:
                    return (boundsWidth - lineWidth) / 2;

                case HorizontalAlignment.Right:
                    return boundsWidth - lineWidth;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
