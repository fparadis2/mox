using System.Windows.Media.Imaging;

namespace Mox.UI
{
    public static class ImageService
    {
        #region Variables

        private static readonly ImageServiceImplementation m_implementation = new ImageServiceImplementation();

        #endregion

        #region Methods

        public static BitmapImage LoadImage(ImageKey key, ImageLoadedCallback loadedCallback)
        {
            return m_implementation.LoadImage(key, loadedCallback);
        }

        #endregion
    }

    public delegate void ImageLoadedCallback(ImageKey key, BitmapImage image);
}
