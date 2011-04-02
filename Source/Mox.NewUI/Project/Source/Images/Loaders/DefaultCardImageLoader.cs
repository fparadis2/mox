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

        private const string BaseUrl = "http://magiccards.info/scans/en/";
        private const string RootDirectory = "magiccards.info";

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
                    var cropped = new CroppedBitmap(image, new Int32Rect(19, 46, 273, 202));
                    cropped.Freeze();
                    image = cropped;
                }
                return true;
            }

            return false;
        }

        private static string GetRelativeFileName(CardInstanceInfo cardInstance)
        {
            string setIdentifier = GetSetIdentifier(cardInstance.Set);
            Debug.Assert(!string.IsNullOrEmpty(setIdentifier));

            return Path.Combine(setIdentifier.ToLower(), cardInstance.Index + ".jpg");
        }

        private static string GetImageUrl(string relativeFileName)
        {
            return BaseUrl + relativeFileName;
        }

        #region Set Identifier Mapping

        private static string GetSetIdentifier(SetInfo set)
        {
            string setIdentifier = set.Identifier;

            switch (setIdentifier.ToLower())
            {
                case "2ed":
                    return "un";
                case "3ed":
                    return "rv";
                case "4ed":
                    return "4e";
                case "5ed":
                    return "5e";
                case "6ed":
                    return "6e";
                case "7ed":
                    return "7e";
                case "8ed":
                    return "8e";
                case "9ed":
                    return "9e";
                case "all":
                    return "ai";
                case "apc":
                    return "ap";
                case "arn":
                    return "an";
                case "atq":
                    return "aq";
                case "chr":
                    return "ch";
                case "con":
                    return "cfx";
                case "csp":
                    return "cs";
                case "dis":
                    return "di";
                case "drk":
                    return "dk";
                case "dst":
                    return "ds";
                case "exo":
                    return "ex";
                case "fem":
                    return "fe";
                case "gpt":
                    return "gp";
                case "hml":
                    return "hl";
                case "ice":
                    return "ia";
                case "inv":
                    return "in";
                case "jud":
                    return "ju";
                case "lea":
                    return "al";
                case "leb":
                    return "be";
                case "leg":
                    return "lg";
                case "lgn":
                    return "le";
                case "lrw":
                    return "lw";
                case "mir":
                    return "mr";
                case "mmq":
                    return "mm";
                case "mor":
                    return "mt";
                case "mrd":
                    return "mi";
                case "nem":
                    return "ne";
                case "ody":
                    return "od";
                case "ons":
                    return "on";
                case "p02":
                    return "po2";
                case "pcy":
                    return "pr";
                case "plc":
                    return "pc";
                case "pls":
                    return "ps";
                case "por":
                    return "po";
                case "ptk":
                    return "p3k";
                case "s00":
                    return "st2k";
                case "s99":
                    return "st";
                case "scg":
                    return "sc";
                case "sth":
                    return "sh";
                case "tmp":
                    return "tp";
                case "tor":
                    return "tr";
                case "tsb":
                    return "tsts";
                case "tsp":
                    return "ts";
                case "uds":
                    return "ud";
                case "ulg":
                    return "ul";
                case "usg":
                    return "us";
                case "vis":
                    return "vi";
                case "wth":
                    return "wl";

                default:
                    return setIdentifier;
            }
        }

        #endregion

        #endregion
    }
}
