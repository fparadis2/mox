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

        #region Constructor

        protected CardFrameRenderer(CardFrame frame, CardInstanceInfo card)
        {
            Card = card;
            Frame = frame;
        }

        #endregion

        #region Properties

        public CardInstanceInfo Card
        {
            get;
            private set;
        }

        public CardFrame Frame
        {
            get; 
            private set; 
        }

        public DrawingContext Context
        {
            get;
            private set;
        }

        public Size RenderSize
        {
            get; 
            private set; 
        }

        protected Rect Bounds
        {
            get { return new Rect(new Point(), RenderSize);}
        }

        protected double RenderRatio
        {
            get { return RenderSize.Width / CardFrame.DefaultWidth; }
        }

        #endregion

        #region Methods

        protected abstract void Render();

        public void Render(DrawingContext context, Size renderSize)
        {
            Context = context;
            RenderSize = renderSize;
            Render();
        }

        public static CardFrameRenderer Create(CardFrame frame, CardInstanceInfo card)
        {
            return new CardFrameRenderer_Eight(frame, card);
        }

        protected void RenderImage(ImageKey key, Rect bounds)
        {
            var image = Frame.LoadImage(key);

            if (image != null)
            {
                Context.DrawImage(image, bounds);
            }
        }

        protected static ImageSource LoadImage(string filename)
        {
            return m_cache.Get(filename);
        }

        protected Point ToRenderCoordinates(Point cardCoords)
        {
            var renderSize = RenderSize;

            return new Point(
                cardCoords.X / CardFrame.DefaultWidth * renderSize.Width,
                cardCoords.Y / CardFrame.DefaultHeight * renderSize.Height
                );
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
