using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    internal class ImageStorage
    {
        #region Variables

        private static readonly Dictionary<System.Type, IImageLoader> ms_loaders = new Dictionary<System.Type, IImageLoader>();

        #endregion

        #region Constructor

        static ImageStorage()
        {
            
        }

        #endregion

        #region Methods

        public BitmapImage LoadImage(ImageKey key)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}