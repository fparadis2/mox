using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    public static class ImageService
    {
        #region Constants

        private const string ImagesCache = "Images";

        internal static readonly bool ForceDownload = false;

        #endregion

        #region Variables

        private static readonly ImageServiceImplementation m_implementation = new ImageServiceImplementation();

        #endregion

        #region Properties

        public static string CachePath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Mox", ImagesCache); }
        }

        #endregion

        #region Methods

        public static BitmapSource LoadImage(ImageKey key, ImageLoadedCallback loadedCallback)
        {
            return m_implementation.LoadImage(key, loadedCallback);
        }

        public static BitmapSource LoadImageSynchronous(ImageKey key)
        {
            return m_implementation.LoadImageSynchronous(key);
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached("Key", typeof(ImageKey), typeof(ImageService), new FrameworkPropertyMetadata(null, OnKeyChanged));

        public static object GetKey(Image target)
        {
            return target.GetValue(KeyProperty);
        }

        public static void SetKey(Image target, ImageKey key)
        {
            target.SetValue(KeyProperty, key);
        }

        private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Image image = d as Image;
            if (image != null)
            {
                ImageKey key = (ImageKey)e.NewValue;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);

                if (key == null)
                {
                    image.Source = null;
                    return;
                }

                var result = LoadImage(key, (loadedKey, loadedImage) => image.Dispatcher.BeginInvoke((System.Action) (() => UpdateSource(image, loadedKey, loadedImage))));
                if (result != null)
                {
                    image.Source = result;
                }
            }
        }

        private static void UpdateSource(Image image, ImageKey key, BitmapSource loadedImage)
        {
            if (Equals(GetKey(image), key))
            {
                image.Source = loadedImage;
            }
        }

        #endregion
    }

    public delegate void ImageLoadedCallback(ImageKey key, BitmapSource image);
}
