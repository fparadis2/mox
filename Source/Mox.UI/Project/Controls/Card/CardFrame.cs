using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using Mox.Database;

namespace Mox.UI
{
    public class CardFrame : FrameworkElement
    {
        internal const double DefaultWidth = 736;
        internal const double DefaultHeight = 1050;

        private CardFrameRenderer m_renderer;

        public CardFrame()
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        }

        #region Properties

        public static readonly DependencyProperty CardProperty = DependencyProperty.Register("Card", typeof(CardInstanceInfo), typeof(CardFrame), new FrameworkPropertyMetadata(default(CardInstanceInfo), FrameworkPropertyMetadataOptions.AffectsRender, OnCardChanged));

        private static void OnCardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frame = ((CardFrame)d);
            frame.OnCardChanged();
        }

        public CardInstanceInfo Card
        {
            get { return (CardInstanceInfo)GetValue(CardProperty); }
            set { SetValue(CardProperty, value); }
        }

        public CardIdentifier CardIdentifier
        {
            get
            {
                var card = Card;
                if (card == null)
                    return new CardIdentifier();
                return card;
            }
            set { Card = MasterCardDatabase.Instance.GetCardInstance(value); }
        }

        #endregion

        #region Layout

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.NewSize.Width < 200)
            {
                TextOptions.SetTextRenderingMode(this, TextRenderingMode.Aliased);
                TextOptions.SetTextHintingMode(this, TextHintingMode.Animated);
            }
            else
            {
                TextOptions.SetTextRenderingMode(this, TextRenderingMode.Auto);
                TextOptions.SetTextHintingMode(this, TextHintingMode.Auto);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return MeasureArrangeHelper(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return MeasureArrangeHelper(finalSize);
        }

        private static Size MeasureArrangeHelper(Size inputSize)
        {
            bool infiniteWidth = double.IsPositiveInfinity(inputSize.Width);
            bool infiniteHeight = double.IsPositiveInfinity(inputSize.Height);

            if (infiniteWidth && infiniteHeight)
            {
                return new Size(DefaultWidth, DefaultHeight);
            }

            const double AspectRatio = DefaultWidth / DefaultHeight;

            if (infiniteWidth)
            {
                return new Size(inputSize.Height * AspectRatio, inputSize.Height);
            }

            if (infiniteHeight)
            {
                return new Size(inputSize.Width, inputSize.Width / AspectRatio);
            }

            double ratio = inputSize.Width / inputSize.Height;

            if (ratio > AspectRatio) // Wider than high
            {
                return new Size(inputSize.Height * AspectRatio, inputSize.Height);
            }

            return new Size(inputSize.Width, inputSize.Width / AspectRatio);
        }

        #endregion

        #region Rendering

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (m_renderer != null)
                m_renderer.Render(drawingContext, RenderSize);
        }

        internal ImageSource LoadImage(ImageKey key)
        {
            return ImageService.LoadImage(key, OnImageLoaded);
        }

        private void OnImageLoaded(ImageKey key, BitmapSource image)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new System.Action(InvalidateVisual));
        }

        #endregion

        #region Misc

        private void OnCardChanged()
        {
            var card = Card;
            if (card != null)
            {
                m_renderer = CardFrameRenderer.Create(this, card);
            }
        }

        #endregion
    }
}
