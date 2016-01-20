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

        protected const string ImagesRootPath = HQCGSymbolLoader.ImagesRootPath;

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

        protected Rect ToRenderCoordinates(Rect bounds)
        {
            var renderSize = RenderSize;

            return new Rect(
                bounds.X / CardFrame.DefaultWidth * renderSize.Width,
                bounds.Y / CardFrame.DefaultHeight * renderSize.Height,
                bounds.Width / CardFrame.DefaultWidth * renderSize.Width,
                bounds.Height / CardFrame.DefaultWidth * renderSize.Width
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

        protected class ColoredImageCache
        {
            private readonly string m_folder;
            private readonly string m_format;
            private readonly Dictionary<Color, string> m_cache = new Dictionary<Color, string>();

            public ColoredImageCache(string folder, string format)
            {
                m_folder = folder;
                m_format = format;

                CreateCache(folder);
            }

            public string Get(Color color)
            {
                string result;
                return Get(color, out result);
            }

            public string Get(Color color, out string colorString)
            {
                if (!m_cache.TryGetValue(color, out colorString))
                    throw new Exception();
                return FormatPath(colorString);
            }

            public string FormatPath(string value)
            {
                return Path.Combine(m_folder, string.Format(m_format, value));
            }

            private void CreateCache(string folder)
            {
                foreach (var file in Directory.GetFiles(folder))
                {
                    var filename = Path.GetFileNameWithoutExtension(file);

                    Color color;
                    if (TryParseColor(filename, out color))
                        m_cache.Add(color, filename);
                }
            }

            private bool TryParseColor(string text, out Color color)
            {
                color = Color.None;

                if (text == "C")
                    return true;

                foreach (char c in text)
                {
                    switch (c)
                    {
                        case 'W':
                            color |= Color.White;
                            break;
                        case 'U':
                            color |= Color.Blue;
                            break;
                        case 'R':
                            color |= Color.Red;
                            break;
                        case 'G':
                            color |= Color.Green;
                            break;
                        case 'B':
                            color |= Color.Black;
                            break;

                        default:
                            return false;
                    }
                }

                return true;
            }
        }

        #endregion
    }
}
