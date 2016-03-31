// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI
{
    /// <summary>
    /// A panel used to mimic a hand of cards.
    /// </summary>
    public class FanPanel : Panel
    {
        #region Dependency Properties

        public static DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(FanPanel), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsArrange, SelectedIndex_ValueChanged));
        
        #endregion

        #region Variables

        private int m_lastSelectedIndex;

        #endregion

        #region Constructor

        public FanPanel()
        {
            Background = Brushes.Transparent;
        }

        #endregion

        #region Properties

        public int SelectedIndex
        {
            get { return (int) GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        #endregion

        #region Methods

        #region Arrangement

        protected override Size MeasureOverride(Size availableSize)
        {
            Size idealSize = new Size(0, 0);

            // Allow children as much room as they want - then scale them
            Size InfiniteSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
            foreach (UIElement child in Children)
            {
                child.Measure(InfiniteSize);

                Size childSize = child.DesiredSize;
                idealSize.Width += childSize.Width;
                idealSize.Height = Math.Max(idealSize.Height, childSize.Height);
            }

            // EID calls us with infinity, but framework doesn't like us to return infinity
            if (double.IsInfinity(availableSize.Height) || double.IsInfinity(availableSize.Width))
            {
                return idealSize;
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children == null || Children.Count == 0)
            {
                return finalSize;
            }

            // Assume all children have the same size
            Size childSize = Children[0].DesiredSize;
            double ratio = finalSize.Height / childSize.Height;
            double childWidth = childSize.Width * ratio;

            // Check if enough space is available
            const double Padding = 5;
            double totalChildWidth = Math.Min(finalSize.Width, (childWidth + Padding) * Children.Count - Padding);

            double margin = (finalSize.Width - totalChildWidth) / 2;

            // ------------------------
            // |  |  |  |             |
            // |  |  |  | child width |
            // ------------------------
            double sideLength = Math.Max(0, totalChildWidth - childWidth);

            // I don't understand where the 2 factor comes from :(
            var realPositions = GetNormalizedPositions().Select(x => (margin + x * sideLength)).ToArray();

            AnimateAll(realPositions, ratio, TimeSpan.FromMilliseconds(200));

            return finalSize;
        }

        private double[] GetNormalizedPositions()
        {
            double[] positions = new double[Children.Count];

            double Increment = 1.0 / (positions.Length - 1);

            double currentPosition = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = currentPosition;
                currentPosition += Increment;
            }
            return positions;
        }

        #endregion

        #region Animation

        private void AnimateAll(double[] positions, double ratio, TimeSpan duration)
        {
            Debug.Assert(Children != null && Children.Count != 0);
            Debug.Assert(positions.Length == Children.Count);

            int zIndex = 0;
            int zIndexIncrement = m_lastSelectedIndex == 0 ? -1 : 1;

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];

                // Set ZIndex
                SetZIndex(child, zIndex);
                zIndex += zIndexIncrement;

                if (i == m_lastSelectedIndex - 1)
                {
                    zIndexIncrement = -zIndexIncrement;
                }

                Point position = new Point(positions[i], 0);
                child.Arrange(new Rect(position, child.DesiredSize));

                child.RenderTransformOrigin = new Point(0, 0);
                child.RenderTransform = new ScaleTransform { ScaleX = ratio, ScaleY = ratio };
            }
        }

        #endregion

        #endregion

        #region Event Handlers

        private static void SelectedIndex_ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FanPanel panel = (FanPanel)obj;

            if (panel.SelectedIndex >= 0)
            {
                panel.m_lastSelectedIndex = panel.SelectedIndex;
            }
        }

        #endregion
    }
}
