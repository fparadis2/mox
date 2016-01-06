using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mox.Database;

namespace Mox.UI
{
    public abstract class CardFrameRenderer
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

        public CardFrame Frame
        {
            get; 
            set;
        }

        public DrawingContext Context
        {
            get; 
            set;
        }

        protected Rect Bounds
        {
            get { return new Rect(new Point(), Frame.RenderSize);}
        }

        #endregion

        #region Methods

        protected abstract void Render();

        public static void Render(CardFrame frame, DrawingContext context, CardInstanceInfo card)
        {
            CardFrameRenderer renderer = new CardFrameRenderer_Eight
            {
                Frame = frame,
                Context = context,
                Card = card
            };

            renderer.Render();
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

        #endregion
    }
}
