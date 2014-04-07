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
            string relativeFileName = GetRelativeFileName(key.Card);
            string cacheFileName = Path.Combine(CacheDirectory, relativeFileName);
            string url = GetImageUrl(relativeFileName);

            if (TryLoadImageFromWeb(cacheFileName, url, out image))
            {
                if (key.Cropped)
                {
                    throw new NotImplementedException();
                    /*var cropped = new CroppedBitmap(image, GetCropRect(key.Card.Set));
                    cropped.Freeze();
                    image = cropped;*/
                }
                return true;
            }

            return false;
        }

        private static string GetRelativeFileName(CardInstanceInfo cardInstance)
        {
            return cardInstance.MultiverseId + ".jpg";
        }

        private static string GetImageUrl(string relativeFileName)
        {
            return BaseUrl + relativeFileName;
        }

        #region Set Identifier Mapping
        
        /*private static Int32Rect GetCropRect(SetInfo set)
        {
            if (IsPreEighth(set))
            {
                return new Int32Rect(29, 36, 254, 206);
            }

            return new Int32Rect(19, 46, 273, 202);
        }

        private static bool IsPreEighth(SetInfo set)
        {
            return set.ReleaseDate < new DateTime(2003, 07, 23);
        }*/

        #endregion

        #endregion
    }
}
