using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    internal class ImageStorage
    {
        #region Variables

        private static readonly List<IImageLoader> ms_loaders = new List<IImageLoader>();

        #endregion

        #region Constructor

        static ImageStorage()
        {
            ms_loaders.Add(new HQCGSymbolLoader());
            //ms_loaders.Add(new GathererSymbolLoader());
            ms_loaders.Add(new MagicCardsImageLoader());
            ms_loaders.Add(new CardFrameImageLoader());
            //ms_loaders.Add(new DefaultCardImageLoader());
        }

        #endregion

        #region Methods

        public BitmapSource LoadImage(ImageKey key)
        {
            foreach (var loader in ms_loaders)
            {
                BitmapSource image;
                if (loader.TryLoadImage(key, out image))
                {
                    return image;
                }
            }

            return null;
        }

        #endregion
    }
}