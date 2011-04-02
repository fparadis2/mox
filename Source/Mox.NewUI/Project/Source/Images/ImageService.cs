using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    public static class ImageService
    {
        #region Constants

        private const string ImagesCache = "Images";

        #endregion

        #region Variables

        private static readonly ImageServiceImplementation m_implementation = new ImageServiceImplementation();

        #endregion

        #region Properties

        public static string CachePath
        {
            get { return Path.GetFullPath(ImagesCache); }
        }

        #endregion

        #region Methods

        public static BitmapImage LoadImage(ImageKey key, ImageLoadedCallback loadedCallback)
        {
            return m_implementation.LoadImage(key, loadedCallback);
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
                image.Source = key == null ? 
                    null : 
                    LoadImage(key, (loadedKey, loadedImage) => image.Dispatcher.BeginInvoke((System.Action)(() => image.Source = loadedImage)));
            }
        }

        #endregion
    }

    public delegate void ImageLoadedCallback(ImageKey key, BitmapImage image);
}
