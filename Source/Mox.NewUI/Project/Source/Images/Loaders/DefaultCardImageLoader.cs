using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Mox.Database;

namespace Mox.UI
{
    internal class DefaultCardImageLoader : ImageLoader
    {
        #region Constants

        private const string BaseUrl = "http://mtgimage.com/multiverseid/";
        private const string RootDirectory = "mtgimage.com";

        #endregion

        #region Properties

        private static string CacheDirectory
        {
            get { return Path.Combine(ImageService.CachePath, RootDirectory); }
        }

        #endregion

        #region Methods

        public override bool TryLoadImage(ImageKey key, out BitmapSource image)
        {
            if (key is ImageKey.CardImage)
            {
                return TryLoadImage((ImageKey.CardImage)key, out image);
            }

            image = null;
            return false;
        }

        private static bool TryLoadImage(ImageKey.CardImage key, out BitmapSource image)
        {
            string relativeFileName = GetRelativeFileName(key);
            string cacheFileName = Path.Combine(CacheDirectory, relativeFileName);
            string url = GetImageUrl(relativeFileName);

            return TryLoadImageFromWeb(cacheFileName, url, out image);
        }

        private static string GetRelativeFileName(ImageKey.CardImage key)
        {
            return string.Format("{0}{1}.jpg", key.Card.MultiverseId, key.Cropped ? ".crop" : string.Empty);
        }

        private static string GetImageUrl(string relativeFileName)
        {
            return BaseUrl + relativeFileName;
        }

        #endregion
    }
}
