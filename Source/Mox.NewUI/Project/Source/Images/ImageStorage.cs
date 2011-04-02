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
            ms_loaders.Add(new GathererSymbolLoader());
        }

        #endregion

        #region Methods

        public BitmapImage LoadImage(ImageKey key)
        {
            foreach (var loader in ms_loaders)
            {
                BitmapImage image;
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