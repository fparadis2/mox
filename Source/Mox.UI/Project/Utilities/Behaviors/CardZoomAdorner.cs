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
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Mox.UI.Shaders;

namespace Mox.UI
{
    public class CardZoomAdorner : Adorner, IDisposable
    {
        #region Constants

        private static readonly Duration FadeInDuration = TimeSpan.FromMilliseconds(150);
        private static readonly Duration FadeOutDuration = TimeSpan.FromMilliseconds(100);
        private const double BufferZone = 0.05;

        private static readonly GloomSettings BaseGloomSettings = new GloomSettings
        {
            GloomIntensity = 0,
            BaseIntensity = 1,
            GloomSaturation = 0,
            BaseSaturation = 1
        };

        private static readonly GloomSettings TargetGloomSettings = new GloomSettings
        {
            GloomIntensity = 0.5,
            BaseIntensity = 1,
            GloomSaturation = 0.1,
            BaseSaturation = 0.5
        };

        #endregion

        #region Variables

        private readonly AdornerLayer m_layer;
        private readonly ContentPresenter m_decoratorPresenter;
        private readonly VisualCollection m_children;
        private readonly CardZoomAdornerElementVisual m_elementVisual;
        private readonly VisualBrush m_brush;
        private readonly GloomEffect m_gloom;

        private System.Action m_completedFadeOutAction;
        private DoubleAnimation m_xAnimation;

        #endregion

        #region Constructor

        public CardZoomAdorner(AdornerDecorator decorator, UIElement adornedElement)
            : base(adornedElement)
        {
            m_layer = decorator.AdornerLayer;
            m_decoratorPresenter = FindContentPresenter(decorator);
            m_children = new VisualCollection(this);
            m_elementVisual = new CardZoomAdornerElementVisual(this);
            m_brush = new VisualBrush(adornedElement);

            m_gloom = new GloomEffect
            {
                GloomIntensity = BaseGloomSettings.GloomIntensity,
                BaseIntensity = BaseGloomSettings.BaseIntensity,
                GloomSaturation = BaseGloomSettings.GloomSaturation,
                BaseSaturation = BaseGloomSettings.BaseSaturation
            };

            m_layer.Add(this);

            IsHitTestVisible = false;

            var parent = (UIElement)VisualTreeHelper.GetParent(AdornedElement);
            parent.Opacity = 0;

            m_decoratorPresenter.Effect = m_gloom;

            m_children.Add(m_elementVisual);
        }

        public void Dispose()
        {
            m_decoratorPresenter.Effect = null;
            m_layer.Remove(this);

            var parent = (UIElement)VisualTreeHelper.GetParent(AdornedElement);
            parent.Opacity = 1;
        }

        #endregion

        #region Properties

        public Brush Brush
        {
            get { return m_brush; }
        }

        #endregion

        #region Methods

        protected override int VisualChildrenCount { get { return m_children.Count; } }
        protected override Visual GetVisualChild(int index) { return m_children[index]; }

        public void FadeIn()
        {
            Point position;
            double rotation, scale;
            GetInitialState(out position, out rotation, out scale);

            m_elementVisual.XOffset = position.X;
            m_elementVisual.YOffset = position.Y;
            m_elementVisual.Rotation = rotation;
            m_elementVisual.Scale = scale;

            Rect bounds = GetTargetRect();
            AnimateTo(bounds.Location, 0, GetTargetScale(bounds), FadeInDuration, null);
            AnimateGloomTo(TargetGloomSettings, FadeInDuration);
        }

        public void FadeOut(System.Action completedAction)
        {
            Point position;
            double rotation, scale;
            GetInitialState(out position, out rotation, out scale);
            AnimateTo(position, rotation, scale, FadeOutDuration, completedAction);
            AnimateGloomTo(BaseGloomSettings, FadeOutDuration);
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

            m_elementVisual.BeginAnimation(CardZoomAdornerElementVisual.XOffsetProperty, m_xAnimation);
            m_elementVisual.BeginAnimation(CardZoomAdornerElementVisual.YOffsetProperty, new DoubleAnimation { To = position.Y, Duration = duration });
            m_elementVisual.BeginAnimation(CardZoomAdornerElementVisual.RotationProperty, new DoubleAnimation { To = rotation, Duration = duration });
            m_elementVisual.BeginAnimation(CardZoomAdornerElementVisual.ScaleProperty, new DoubleAnimation { To = scale, Duration = duration });
        }

        private void AnimateGloomTo(GloomSettings settings, Duration duration)
        {
            m_gloom.BeginAnimation(GloomEffect.GloomIntensityProperty, new DoubleAnimation { To = settings.GloomIntensity, Duration = duration });
            m_gloom.BeginAnimation(GloomEffect.BaseIntensityProperty, new DoubleAnimation { To = settings.BaseIntensity, Duration = duration });
            m_gloom.BeginAnimation(GloomEffect.GloomSaturationProperty, new DoubleAnimation { To = settings.GloomSaturation, Duration = duration });
            m_gloom.BeginAnimation(GloomEffect.BaseSaturationProperty, new DoubleAnimation { To = settings.BaseSaturation, Duration = duration });
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

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var child in m_children.OfType<UIElement>())
                child.Arrange(new Rect(0, 0, m_layer.ActualWidth, m_layer.ActualHeight));

            return base.ArrangeOverride(finalSize);
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

        private void GetInitialState(out Point position, out double rotation, out double scale)
        {
            GeneralTransform transform = AdornedElement.TransformToVisual(this);

            Decompose((MatrixTransform)transform, out position, out rotation, out scale);
        }

        private static void Decompose(MatrixTransform transform, out Point position, out double rotation, out double scale)
        {
            position = new Point { X = transform.Matrix.OffsetX, Y = transform.Matrix.OffsetY };
            scale = Math.Sqrt(transform.Matrix.Determinant);
            rotation = Math.Acos(transform.Matrix.M11 / scale) * 180 / Math.PI;
        }

        private static ContentPresenter FindContentPresenter(AdornerDecorator decorator)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(decorator); i++)
            {
                var presenter = VisualTreeHelper.GetChild(decorator, i) as ContentPresenter;
                if (presenter != null)
                    return presenter;
            }

            return null;
        }

        #endregion

        #region Nested Types

        private class GloomSettings
        {
            public double GloomIntensity;
            public double BaseIntensity;
            public double GloomSaturation;
            public double BaseSaturation;
        }

        private class CardZoomAdornerElementVisual : FrameworkElement
        {
            public static readonly DependencyProperty XOffsetProperty = DependencyProperty.Register("XOffset", typeof(double), typeof(CardZoomAdornerElementVisual), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
            public static readonly DependencyProperty YOffsetProperty = DependencyProperty.Register("YOffset", typeof(double), typeof(CardZoomAdornerElementVisual), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

            public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(double), typeof(CardZoomAdornerElementVisual), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

            public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(CardZoomAdornerElementVisual), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

            private readonly CardZoomAdorner m_owner;

            public CardZoomAdornerElementVisual(CardZoomAdorner owner)
            {
                m_owner = owner;
            }

            public double XOffset
            {
                get { return (double)GetValue(XOffsetProperty); }
                set { SetValue(XOffsetProperty, value); }
            }

            public double YOffset
            {
                get { return (double)GetValue(YOffsetProperty); }
                set { SetValue(YOffsetProperty, value); }
            }

            public double Rotation
            {
                get { return (double)GetValue(RotationProperty); }
                set { SetValue(RotationProperty, value); }
            }

            public double Scale
            {
                get { return (double)GetValue(ScaleProperty); }
                set { SetValue(ScaleProperty, value); }
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
                    drawingContext.DrawRectangle(m_owner.Brush, null, new Rect(m_owner.AdornedElement.DesiredSize));
                }
                drawingContext.Pop();
            }
        }

        #endregion
    }
}