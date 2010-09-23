using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    public static class ImageSourceBehavior
    {
        public static bool GetSource(Image image)
        {
            return (bool)image.GetValue(SourceProperty);
        }

        public static void SetSource(Image image, ImageSource source)
        {
            image.SetValue(SourceProperty, source);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(ImageSource), typeof(ImageSourceBehavior), new UIPropertyMetadata(OnImageSourceChanged));

        static void OnImageSourceChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            Image image = depObj as Image;
            if (image == null)
            {
                return;
            }

            if (!(e.NewValue is BitmapSource))
            {
                return;
            }

            BitmapSource bitmap = (BitmapSource)e.NewValue;
            image.Source = bitmap;
            image.Width = bitmap.PixelWidth;
            image.Height = bitmap.PixelHeight;
        }
    }
}