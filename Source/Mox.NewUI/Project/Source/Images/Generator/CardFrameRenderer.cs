using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mox.Database;

namespace Mox.UI
{
    public abstract class CardFrameRenderer
    {
        #region Variables

        private static readonly ImageCache m_cache = new ImageCache();

        #endregion

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
            return m_cache.Get(filename);
        }

        #endregion

        #region Inner Types

        private class ImageCache
        {
            private readonly Dictionary<string, ImageSource> m_cache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

            public ImageSource Get(string filename)
            {
                ImageSource result;
                if (!m_cache.TryGetValue(filename, out result))
                {
                    result = LoadImage(filename);
                    m_cache.Add(filename, result);
                }

                return result;
            }

            private static ImageSource LoadImage(string filename)
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
        }

        #endregion
    }
}
