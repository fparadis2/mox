﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mox.Database;

namespace Mox.UI.ImageGenerator
{
    public abstract class CardFrameGenerator
    {
        #region Constants

        public const int Width = 312;
        public const int Height = 445;
        private const double Dpi = 96;

#warning Make more flexible :)
        protected const string ImagesRootPath = @"D:\Programmation\HQCG\images\";

        #endregion

        #region Properties

        public CardInstanceInfo Card
        {
            get;
            set;
        }

        public DrawingContext Context
        {
            get;
            set;
        }

        #endregion

        #region Methods

        protected abstract void Render();

        public static BitmapSource RenderFrame(CardInstanceInfo card)
        {
            CardFrameGenerator generator = new CardFrameGenerator_Eight
            {
                Card = card
            };

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                generator.Context = context;
                generator.Render();
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap(Width, Height, Dpi, Dpi, PixelFormats.Default);

            bitmap.Render(visual);
            bitmap.Freeze();

            return bitmap;
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
