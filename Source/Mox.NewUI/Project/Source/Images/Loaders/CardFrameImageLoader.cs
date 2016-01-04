using System;
using System.Windows.Media.Imaging;
using Mox.UI.ImageGenerator;

namespace Mox.UI
{
    internal class CardFrameImageLoader : IImageLoader
    {
        public bool TryLoadImage(ImageKey key, out BitmapSource image)
        {
            var cardKey = key as ImageKey.CardFrameImage;
            if (cardKey != null)
            {
                image = CardFrameGenerator.RenderFrame(cardKey.Card);
                return true;
            }

            image = null;
            return false;
        }
    }
}
