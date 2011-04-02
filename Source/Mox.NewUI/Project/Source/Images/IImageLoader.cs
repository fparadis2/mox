using System.Windows.Media.Imaging;

namespace Mox.UI
{
    public interface IImageLoader
    {
        bool TryLoadImage(ImageKey key, out BitmapSource image);
    }
}