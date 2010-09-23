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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mox.UI
{
    public class CardZoomAdorner : Adorner
    {
        #region Inner Types

        private class NoTransitionsCustomVisualStateManager : VisualStateManager
        {
            #region Methods

            protected override bool GoToStateCore(FrameworkElement control, FrameworkElement stateGroupsRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
            {
                return base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, false);
            }

            #endregion
        }

        #endregion

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

        private readonly CardTemplate m_cardTemplate = new CardTemplate();

        private System.Action m_completedFadeOutAction;
        private DoubleAnimation m_xAnimation;

        #endregion

        #region Constructor

        public CardZoomAdorner(CardTemplate adornedElement)
            : base(adornedElement)
        {
            m_cardTemplate.CardControl.Animate = false;
            m_cardTemplate.DataContext = adornedElement.DataContext;
            m_cardTemplate.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //m_cardTemplate.CardControl.Frame.SetValue(VisualStateManager.CustomVisualStateManagerProperty, new NoTransitionsCustomVisualStateManager());

            Point position;
            double rotation, scale;
            GetInitialState(out position, out rotation, out scale);

            XOffset = position.X;
            YOffset = position.Y;
            Rotation = rotation;
            Scale = scale;

            IsHitTestVisible = false;
        }

        #endregion

        #region Properties

        private new CardTemplate AdornedElement
        {
            get { return (CardTemplate)base.AdornedElement; }
        }

        private Visual AdornedFrame
        {
            get { return AdornedElement.CardControl.Frame; }
        }

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
            AnimateTo(bounds.Location, 0, GetScale(bounds), FadeInDuration, null);
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

            VisualBrush visualBrush = new VisualBrush(m_cardTemplate);

            TransformGroup group = new TransformGroup();
            group.Children.Add(new RotateTransform { Angle = Rotation });
            group.Children.Add(new ScaleTransform { ScaleX = Scale, ScaleY = Scale });
            group.Children.Add(new TranslateTransform { X = XOffset, Y = YOffset });

            drawingContext.PushTransform(group);
            {
                //Brush visualBrush = Brushes.Blue;
                drawingContext.DrawRectangle(visualBrush, null, new Rect(m_cardTemplate.DesiredSize));
            }
            drawingContext.Pop();
        }

        private Rect GetTargetRect()
        {
            AdornerLayer parentLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
            Rect targetRect = GetTargetRect(new Rect(new Size(parentLayer.ActualWidth, parentLayer.ActualHeight)));

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

        private double GetScale(Rect rect)
        {
            Rect bounds = new Rect(m_cardTemplate.DesiredSize);
            return Math.Min(rect.Width / bounds.Width, rect.Height / bounds.Height);
        }

        private void GetInitialState(out Point position, out double rotation, out double scale)
        {
            AdornerLayer parentLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
            GeneralTransform transform = AdornedFrame.TransformToVisual(parentLayer);

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
