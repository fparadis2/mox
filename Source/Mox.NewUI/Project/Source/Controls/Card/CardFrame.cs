using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Mox.Database;

namespace Mox.UI
{
    public class CardFrame : FrameworkElement
    {
        internal const double DefaultWidth = 736;
        internal const double DefaultHeight = 1050;

        public CardFrame()
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            
            //TextOptions.SetTextRenderingMode(this, TextRenderingMode.Auto);
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
            get { return Card; }
            set { Card = MasterCardDatabase.Instance.GetCardInstance(value); }
        }

        internal SymbolTextRenderer AbilityTextRenderer;

        #endregion

        #region Layout

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            TextOptions.SetTextHintingMode(this, sizeInfo.NewSize.Width < 200 ? TextHintingMode.Animated : TextHintingMode.Auto);
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

        internal Point ToRenderCoordinates(Point cardCoords)
        {
            var renderSize = RenderSize;

            return new Point(
                cardCoords.X / DefaultWidth * renderSize.Width,
                cardCoords.Y / DefaultHeight * renderSize.Height
                );
        }

        #endregion

        #region Rendering

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            CardFrameRenderer.Render(this, drawingContext, Card);
        }

        internal void RenderImage(DrawingContext context, ImageKey key, Rect bounds)
        {
            var image = LoadImage(key);

            if (image != null)
            {
                context.DrawImage(image, bounds);
            }
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
                List<TextTokenizer.Token> tokens = new List<TextTokenizer.Token>();
                TextTokenizer.Tokenize(card.Card.Text, tokens);

                var typeface = new Typeface(Fonts.AbilityTextFont, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                var size = 15;
                var maxSize = new Size(500, 500);
                AbilityTextRenderer = new SymbolTextRenderer(card.Card.Text, tokens, maxSize, typeface, size);
            }
        }

        #endregion
    }
}
