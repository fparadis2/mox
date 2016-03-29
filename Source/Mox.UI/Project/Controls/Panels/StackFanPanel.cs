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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mox.UI.Game
{
    /// <summary>
    /// A panel used to show the spell stack
    /// </summary>
    public class StackFanPanel : Panel
    {
        #region Dependency Properties

        public static DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(StackFanPanel), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsArrange));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty FoldWidthProperty = DependencyProperty.Register("FoldWidth", typeof(double), typeof(StackFanPanel), new FrameworkPropertyMetadata(0.2, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double FoldWidth
        {
            get { return (double) GetValue(FoldWidthProperty); }
            set { SetValue(FoldWidthProperty, value); }
        }

        public static readonly DependencyProperty NormalOffsetRatioProperty = DependencyProperty.Register("NormalOffsetRatio", typeof(double), typeof(StackFanPanel), new FrameworkPropertyMetadata(0.9, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double NormalOffsetRatio
        {
            get { return (double)GetValue(NormalOffsetRatioProperty); }
            set { SetValue(NormalOffsetRatioProperty, value); }
        }

        #endregion

        #region Constructor

        public StackFanPanel()
        {
            Background = Brushes.Transparent;
        }

        #endregion

        #region Methods

        #region Arrangement

        protected override Size MeasureOverride(Size availableSize)
        {
            Size childSize = new Size();

            // Allow children as much room as they want - then scale them
            Size InfiniteSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
            foreach (UIElement child in Children)
            {
                child.Measure(InfiniteSize);

                childSize.Width = Math.Max(childSize.Width, child.DesiredSize.Width);
                childSize.Height = Math.Max(childSize.Height, child.DesiredSize.Height);
            }

            double height = Math.Min(availableSize.Height, childSize.Height);

            double ratio = height / childSize.Height;
            double childWidth = childSize.Width * ratio;
            double foldWidth = FoldWidth * childWidth;

            double width = 0;

            if (Children.Count > 0)
            {
                width -= foldWidth;
                width += childWidth;

                width += Children.Count * foldWidth;
            }

            return new Size(width, height);
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
            double foldWidth = FoldWidth * childWidth;

            double x = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                child.Arrange(new Rect(new Point(0, 0), child.DesiredSize));
                SetZIndex(child, i);
                AnimateTo(child, new Point(x, 0), ratio, TimeSpan.FromMilliseconds(200));

                x += foldWidth;
            }

            x -= foldWidth;

            return new Size(x + childWidth, finalSize.Height);
        }

        #endregion

        #region Animation

        private void AnimateTo(UIElement child, Point position, double ratio, TimeSpan duration)
        {
            // If this is the first time we've seen this child, add our transforms
            if (child.RenderTransform as TransformGroup == null)
            {
                child.RenderTransformOrigin = new Point(0, 0);
                TransformGroup newGroup = new TransformGroup();
                child.RenderTransform = newGroup;
                newGroup.Children.Add(new ScaleTransform { ScaleX = ratio, ScaleY = ratio });
                newGroup.Children.Add(new TranslateTransform { X = position.X, Y = position.Y });
            }

            TransformGroup group = (TransformGroup)child.RenderTransform;
            ScaleTransform scaleToBoundsTransform = (ScaleTransform)group.Children[0];
            TranslateTransform translateTransform = (TranslateTransform)group.Children[1];

            translateTransform.BeginAnimation(TranslateTransform.XProperty, MakeAnimation(position.X, duration));
            translateTransform.BeginAnimation(TranslateTransform.YProperty, MakeAnimation(position.Y, duration));

            scaleToBoundsTransform.ScaleX = ratio;
            scaleToBoundsTransform.ScaleY = ratio;
        }

        private static DoubleAnimation MakeAnimation(double to, TimeSpan duration)
        {
            DoubleAnimation anim = new DoubleAnimation(to, duration)
            {
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.7
            };

            return anim;
        }

        #endregion

        #endregion
    }
}
