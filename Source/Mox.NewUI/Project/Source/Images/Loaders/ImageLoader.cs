using System;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    internal abstract class ImageLoader : IImageLoader
    {
        #region Implementation of IImageLoader

        public abstract bool TryLoadImage(ImageKey key, out BitmapSource image);

        #endregion

        #region Utilities

        protected static bool TryLoadImageFromWeb(string cachePath, string url, out BitmapSource image)
        {
            try
            {
                if (!File.Exists(cachePath))
                {
                    DownloadImage(cachePath, url);
                }

                return TryLoadImageFromDisk(cachePath, out image);
            }
            catch
            {
                image = null;
                return false;
            }
        }

        private static void DownloadImage(string cachePath, string url)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath));

            string tempFile = cachePath + ".downloading";

            WebClient client = new WebClient();
            using (Stream stream = client.OpenRead(url))
            using (Stream fileStream = File.Create(tempFile))
            {
                stream.CopyTo(fileStream);
            }

            File.Move(tempFile, cachePath);
        }

        protected static bool TryLoadImageFromDisk(string path, out BitmapSource image)
        {
            if (!File.Exists(path))
            {
                image = null;
                return false;
            }

            image = Load(i => i.UriSource = new Uri(path));
            return true;
        }

        private static BitmapImage Load(Action<BitmapImage> loadAction)
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // Forces the load on EndInit

                loadAction(image);

                image.EndInit();
                image.Freeze();

                return image;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
