using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI.Library
{
    public class DeckCardListingPanel : Panel
    {
        private UIElement DeckListing
        {
            get { return InternalChildren[0]; }
        }

        private FrameworkElement RightPanel
        {
            get { return (FrameworkElement)InternalChildren[1]; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (InternalChildren.Count < 2)
                return availableSize;

            DeckListing.Measure(new Size(double.MaxValue, availableSize.Height));
            Size deckListingSize = DeckListing.DesiredSize;

            double remainingWidth = Math.Max(0, availableSize.Width - deckListingSize.Width);
            RightPanel.Visibility = remainingWidth < RightPanel.MinWidth ? Visibility.Collapsed : Visibility.Visible;

            RightPanel.Measure(new Size(remainingWidth, availableSize.Height));
            Size rightPanelSize = RightPanel.DesiredSize;

            Size availableSizeForDeckListing = new Size(availableSize.Width - rightPanelSize.Width, availableSize.Height);
            DeckListing.Measure(availableSizeForDeckListing);

            double height = Math.Max(deckListingSize.Height, rightPanelSize.Height);

            return new Size(availableSize.Width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (InternalChildren.Count >= 2)
            {
                double deckListingWidth = Math.Min(finalSize.Width, DeckListing.DesiredSize.Width);
                DeckListing.Arrange(new Rect(0, 0, deckListingWidth, finalSize.Height));

                RightPanel.Arrange(new Rect(finalSize.Width - RightPanel.DesiredSize.Width, 0, RightPanel.DesiredSize.Width, finalSize.Height));
            }

            return finalSize;
        }
    }
}
