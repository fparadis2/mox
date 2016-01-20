using System;
using System.IO;
using System.Windows.Media.Imaging;
using Mox.Database;

namespace Mox.UI
{
    internal class MagicCardsImageLoader : ImageLoader
    {
        #region Constants

        private const string BaseUrl = "http://magiccards.info/scans/en/";
        private const string RootDirectory = "magiccards.info";

        #endregion

        #region Methods

        public override bool TryLoadImage(ImageKey key, out BitmapSource image)
        {
            if (key is ImageKey.CardImage)
            {
                return TryLoadCardImage((ImageKey.CardImage)key, out image);
            }

            image = null;
            return false;
        }

        private static string GetCacheFilename(string subPath)
        {
            return Path.Combine(ImageService.CachePath, RootDirectory, subPath);
        }

        private static bool TryLoadCardImage(ImageKey.CardImage key, out BitmapSource image)
        {
            var card = key.Card;

            if (card.Index == 0)
            {
                image = null;
                return false;
            }

            var subName = string.Format("{0}/{1}.jpg", card.Set.MagicCardsInfoIdentifier, card.Index);

            var cacheFileName = GetCacheFilename(subName);
            var url = BaseUrl + subName;

            return TryLoadImageFromWeb(cacheFileName, url, out image);
        }

        #endregion
    }
}
