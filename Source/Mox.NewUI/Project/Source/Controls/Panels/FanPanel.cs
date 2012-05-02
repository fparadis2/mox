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
using System.Windows.Media.Animation;

namespace Mox.UI.Game
{
    /// <summary>
    /// A panel used to mimic a hand of cards.
    /// </summary>
    public class FanPanel : Panel
    {
        #region Dependency Properties

        public static DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(FanPanel), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsArrange, SelectedIndex_ValueChanged));
        public static DependencyProperty CompactionFactorProperty = DependencyProperty.Register("CompactionFactor", typeof(double), typeof(FanPanel), new FrameworkPropertyMetadata(0.3, FrameworkPropertyMetadataOptions.AffectsArrange));
        public static DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(FanPanel), new FrameworkPropertyMetadata(1.1, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static DependencyProperty ScaleDirectionProperty = DependencyProperty.Register("ScaleDirection", typeof(StretchDirection), typeof(FanPanel), new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsArrange));
        
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

        public double CompactionFactor
        {
            get { return (double) GetValue(CompactionFactorProperty); }
            set { SetValue(CompactionFactorProperty, value); }
        }

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public StretchDirection ScaleDirection
        {
            get { return (StretchDirection)GetValue(ScaleDirectionProperty); }
            set { SetValue(ScaleDirectionProperty, value); }
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
            const double Padding = 2;
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

            if (SelectedIndex >= 0 && SelectedIndex < Children.Count)
            {
                Increment *= (1 - CompactionFactor);
            }

            double currentPosition = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                if (i == SelectedIndex || (i == SelectedIndex + 1 && SelectedIndex >= 0))
                {
                    currentPosition += CompactionFactor / 2;
                }

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

                AnimateTo(child, position, ratio, i == SelectedIndex ? Scale : 1, duration);
                child.Arrange(new Rect(new Point(0, 0), child.DesiredSize));
            }
        }

        private void AnimateTo(UIElement child, Point position, double scaleToBounds, double additionalScale, TimeSpan duration)
        {
            // If this is the first time we've seen this child, add our transforms
            if (child.RenderTransform as TransformGroup == null)
            {
                child.RenderTransformOrigin = new Point (0, 0);
                TransformGroup newGroup = new TransformGroup();
                child.RenderTransform = newGroup;
                newGroup.Children.Add(new ScaleTransform());
                newGroup.Children.Add(new ScaleTransform { ScaleX = scaleToBounds, ScaleY = scaleToBounds });
                newGroup.Children.Add(new TranslateTransform { X = position.X, Y = position.Y });
            }

            TransformGroup group = (TransformGroup)child.RenderTransform;
            ScaleTransform scaleTransform = (ScaleTransform)group.Children[0];
            ScaleTransform scaleToBoundsTransform = (ScaleTransform)group.Children[1];
            TranslateTransform translateTransform = (TranslateTransform)group.Children[2];

            Point additionalScaleCenter = GetScaleOrigin(child.DesiredSize);
            scaleTransform.CenterX = additionalScaleCenter.X;
            scaleTransform.CenterY = additionalScaleCenter.Y;
            

            if (duration == TimeSpan.Zero)
            {
                translateTransform.BeginAnimation(TranslateTransform.XProperty, null);
                translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                scaleToBoundsTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                scaleToBoundsTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);

                translateTransform.X = position.X;
                translateTransform.Y = position.Y;

                scaleToBoundsTransform.ScaleX = scaleToBounds;
                scaleToBoundsTransform.ScaleY = scaleToBounds;

                scaleTransform.ScaleX = additionalScale;
                scaleTransform.ScaleY = additionalScale;
            }
            else
            {
                translateTransform.BeginAnimation(TranslateTransform.XProperty, MakeAnimation(position.X, duration));
                translateTransform.BeginAnimation(TranslateTransform.YProperty, MakeAnimation(position.Y, duration));

                scaleToBoundsTransform.BeginAnimation(ScaleTransform.ScaleXProperty, MakeAnimation(scaleToBounds, duration));
                scaleToBoundsTransform.BeginAnimation(ScaleTransform.ScaleYProperty, MakeAnimation(scaleToBounds, duration));

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, MakeAnimation(additionalScale, duration));
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, MakeAnimation(additionalScale, duration));
            }
        }

        private Point GetScaleOrigin(Size childSize)
        {
            double centerX = childSize.Width / 2;

            switch (ScaleDirection)
            {
                case StretchDirection.Both:
                    return new Point(centerX, childSize.Height / 2);
                case StretchDirection.DownOnly:
                    return new Point(centerX, 0);
                case StretchDirection.UpOnly:
                    return new Point(centerX, childSize.Height);
                default:
                    throw new NotImplementedException();
            }
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

        #region Overrides

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
