using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mox.Database;

namespace Mox.UI.ImageGenerator
{
    public abstract class CardFrameGenerator
    {
        #region Constants

#warning Make more flexible :)
        protected const string ImagesRootPath = @"D:\Programmation\HQCG\images\";

        #endregion

        #region Properties

        public CardInstanceInfo Card
        {
            get;
            set;
        }

        #endregion

        #region Methods

        protected abstract UIElement Generate();

        public static UIElement Generate(CardInstanceInfo card)
        {
            CardFrameGenerator generator = new CardFrameGenerator_Eight
            {
                Card = card
            };

            return generator.Generate();
        }

        protected static ImageSource LoadImage(string filename)
        {
            Debug.Assert(File.Exists(filename));

            BitmapImage image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // Forces the load on EndInit

            image.UriSource = new Uri(filename);

            image.EndInit();
            image.Freeze();

            return image;
        }

        protected static Image CreateImage(ImageKey key)
        {
            Image image = new Image();
            ImageService.SetKey(image, key);
            return image;
        }

        protected static Image CreateImage(ImageSource source)
        {
            return new Image { Source = source };
        }

        #endregion
    }
}
