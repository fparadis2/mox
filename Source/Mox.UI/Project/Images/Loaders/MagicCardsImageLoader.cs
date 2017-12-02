using System;
using System.Diagnostics;
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
            if (card == null)
            {
                image = null;
                return false;
            }

            string index = card.MciIndex;

            if (string.IsNullOrEmpty(index))
            {
                index = card.Index.ToString();
            }

            if (string.IsNullOrEmpty(index))
            {
                Debug.WriteLine(string.Format("Cannot get image for card {0} because its index in the set {1} ({2}) is not available", card.Card.Name, card.Set.Name, card.Set.Identifier));
                image = null;
                return false;
            }

            var subName = string.Format("{0}/{1}.jpg", card.Set.MagicCardsInfoIdentifier, index);

            var cacheFileName = GetCacheFilename(subName);
            var url = BaseUrl + subName;

            return TryLoadImageFromWeb(cacheFileName, url, out image);
        }

        #endregion
    }
}
