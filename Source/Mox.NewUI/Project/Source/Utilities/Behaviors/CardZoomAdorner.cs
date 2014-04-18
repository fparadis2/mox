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
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mox.UI
{
    public class CardZoomAdorner : Adorner, IDisposable
    {
        #region Constants

        private static readonly Duration FadeInDuration = TimeSpan.FromMilliseconds(150);
        private static readonly Duration FadeOutDuration = TimeSpan.FromMilliseconds(100);
        private const double BufferZone = 0.05;

        #endregion

        #region Variables

        private static readonly DependencyProperty XOffsetProperty = DependencyProperty.Register("XOffset", typeof(double), typeof(CardZoomAdorner), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty YOffsetProperty = DependencyProperty.Register("YOffset", typeof(double), typeof(CardZoomAdorner), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        private static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(double), typeof(CardZoomAdorner), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        private static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(CardZoomAdorner), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        private readonly AdornerLayer m_layer;
        private readonly VisualBrush m_brush;

        private System.Action m_completedFadeOutAction;
        private DoubleAnimation m_xAnimation;

        #endregion

        #region Constructor

        public CardZoomAdorner(AdornerLayer layer, UIElement adornedElement)
            : base(adornedElement)
        {
            m_layer = layer;
            m_brush = new VisualBrush(adornedElement);

            Point position;
            double rotation, scale;
            GetInitialState(out position, out rotation, out scale);

            XOffset = position.X;
            YOffset = position.Y;
            Rotation = rotation;
            Scale = scale;

            IsHitTestVisible = false;

            var parent = (UIElement)VisualTreeHelper.GetParent(AdornedElement);
            parent.Opacity = 0;

            m_layer.Add(this);
        }

        public void Dispose()
        {
            m_layer.Remove(this);

            var parent = (UIElement)VisualTreeHelper.GetParent(AdornedElement);
            parent.Opacity = 1;
        }

        #endregion

        #region Properties

        private double XOffset
        {
            get { return (double)GetValue(XOffsetProperty); }
            set { SetValue(XOffsetProperty, value); }
        }

        private double YOffset
        {
            get { return (double)GetValue(YOffsetProperty); }
            set { SetValue(YOffsetProperty, value); }
        }

        private double Rotation
        {
            get { return (double)GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); }
        }

        private double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        #endregion

        #region Methods

        public void FadeIn()
        {
            Rect bounds = GetTargetRect();
            AnimateTo(bounds.Location, 0, GetTargetScale(bounds), FadeInDuration, null);
        }

        public void FadeOut(System.Action completedAction)
        {
            Point position;
            double rotation, scale;
            GetInitialState(out position, out rotation, out scale);
            AnimateTo(position, rotation, scale, FadeOutDuration, completedAction);
        }

        private void AnimateTo(Point position, double rotation, double scale, Duration duration, System.Action completedAction)
        {
            Debug.Assert(!double.IsNaN(rotation));

            m_completedFadeOutAction = completedAction;

            if (m_xAnimation != null)
            {
                m_xAnimation.Completed -= CompleteFadeOut;
            }

            m_xAnimation = new DoubleAnimation { To = position.X, Duration = duration };
            m_xAnimation.Completed += CompleteFadeOut;

            BeginAnimation(XOffsetProperty, m_xAnimation);
            BeginAnimation(YOffsetProperty, new DoubleAnimation { To = position.Y, Duration = duration });
            BeginAnimation(RotationProperty, new DoubleAnimation { To = rotation, Duration = duration });
            BeginAnimation(ScaleProperty, new DoubleAnimation { To = scale, Duration = duration });
        }

        private void CompleteFadeOut(object sender, EventArgs e)
        {
            if (m_completedFadeOutAction != null)
            {
                m_completedFadeOutAction();
            }
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            return Transform.Identity;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            TransformGroup group = new TransformGroup();
            group.Children.Add(new RotateTransform { Angle = Rotation });
            group.Children.Add(new ScaleTransform { ScaleX = Scale, ScaleY = Scale });
            group.Children.Add(new TranslateTransform { X = XOffset, Y = YOffset });

            drawingContext.PushTransform(group);
            {
                drawingContext.DrawRectangle(m_brush, null, new Rect(AdornedElement.DesiredSize));
            }
            drawingContext.Pop();
        }

        private Rect GetTargetRect()
        {
            Rect targetRect = GetTargetRect(new Rect(new Size(m_layer.ActualWidth, m_layer.ActualHeight)));

            return targetRect;
        }

        private static Rect GetTargetRect(Rect totalRect)
        {
            totalRect.Inflate(-BufferZone * totalRect.Width, -BufferZone * totalRect.Height);

            const double desiredRatio = 48.0 / 68.0;
            double currentRatio = totalRect.Width / totalRect.Height;

            if (desiredRatio > currentRatio)
            {
                // center vertically
                totalRect.Inflate(0, (totalRect.Width / desiredRatio - totalRect.Height) / 2);
            }
            else
            {
                // center horizontally
                totalRect.Inflate((totalRect.Height * desiredRatio - totalRect.Width) / 2, 0);
            }

            return totalRect;
        }

        private double GetTargetScale(Rect rect)
        {
            Rect bounds = new Rect(AdornedElement.DesiredSize);
            double targetScale = Math.Min(rect.Width / bounds.Width, rect.Height / bounds.Height);
            
            return targetScale;
        }

        private static double Floor(double value, double interval)
        {
            value /= interval;
            value = Math.Floor(value);
            value *= interval;
            return value;
        }

        private void GetInitialState(out Point position, out double rotation, out double scale)
        {
            GeneralTransform transform = AdornedElement.TransformToVisual(m_layer);

            Decompose((MatrixTransform)transform, out position, out rotation, out scale);
        }

        private static void Decompose(MatrixTransform transform, out Point position, out double rotation, out double scale)
        {
            position = new Point { X = transform.Matrix.OffsetX, Y = transform.Matrix.OffsetY };
            scale = Math.Sqrt(transform.Matrix.Determinant);
            rotation = Math.Acos(transform.Matrix.M11 / scale) * 180 / Math.PI;
        }

        #endregion
    }
}
