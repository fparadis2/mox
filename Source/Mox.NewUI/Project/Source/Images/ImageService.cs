using System.IO;
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
    }

    public delegate void ImageLoadedCallback(ImageKey key, BitmapImage image);
}
