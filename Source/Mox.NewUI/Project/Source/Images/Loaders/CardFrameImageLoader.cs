using System;
using System.Windows.Media.Imaging;
using Mox.UI.ImageGenerator;

namespace Mox.UI
{
#warning remove
    internal class CardFrameImageLoader : IImageLoader
    {
        public bool TryLoadImage(ImageKey key, out BitmapSource image)
        {
            image = null;
            return false;
        }
    }
}
