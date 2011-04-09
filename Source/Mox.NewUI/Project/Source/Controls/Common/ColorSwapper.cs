using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    internal static class ColorSwapper
    {
        #region Delegates

        public delegate System.Windows.Media.Color ColorCallback(System.Windows.Media.Color color);

        #endregion

        #region Methods

        public static Brush SwapColors(Brush brush, ColorCallback colorCallback)
        {
            Throw.IfNull(colorCallback, "colorCallback");

            if (brush != null)
            {
                brush = brush.Clone();
                SwapColorsWithoutCloning(brush, colorCallback);
                brush.Freeze();
            }
            return brush;
        }

        public static Drawing SwapColors(Drawing drawing, ColorCallback colorCallback)
        {
            Throw.IfNull(colorCallback, "colorCallback");

            if (drawing != null)
            {
                drawing = drawing.Clone();
                SwapColorsWithoutCloning(drawing, colorCallback);
                drawing.Freeze();
            }
            return drawing;
        }

        public static ImageSource SwapColors(ImageSource imageSource, ColorCallback colorCallback)
        {
            Throw.IfNull(colorCallback, "colorCallback");

            if (imageSource == null)
            {
                return null;
            }

            DrawingImage image = imageSource as DrawingImage;
            if (image != null)
            {
                image = image.Clone();
                SwapColorsWithoutCloning(image.Drawing, colorCallback);
                image.Freeze();
                return image;
            }

            BitmapSource bitmapSource = imageSource as BitmapSource;
            Debug.Assert(bitmapSource != null, "Expected DrawingImage or BitmapSource");
            return SwapColors(bitmapSource, colorCallback);
        }

        public static BitmapSource SwapColors(BitmapSource bitmapSource, ColorCallback colorCallback)
        {
            Throw.IfNull(colorCallback, "colorCallback");

            if (bitmapSource != null)
            {
                PixelFormat DestinationFormat = PixelFormats.Bgra32;
                const BitmapPalette DestinationPalette = null;
                const double AlphaThreshold = 0.0;

                FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bitmapSource, DestinationFormat, DestinationPalette, AlphaThreshold);

                int pixelWidth = bitmap.PixelWidth;
                int pixelHeight = bitmap.PixelHeight;
                int stride = 4 * pixelWidth;

                byte[] pixels = new byte[stride * pixelHeight];
                bitmap.CopyPixels(pixels, stride, 0);
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
                    System.Windows.Media.Color convertedColor = colorCallback(color);
                    if (convertedColor != color)
                    {
                        pixels[i] = convertedColor.B;
                        pixels[i + 1] = convertedColor.G;
                        pixels[i + 2] = convertedColor.R;
                        pixels[i + 3] = convertedColor.A;
                    }
                }
                bitmapSource = BitmapSource.Create(pixelWidth, pixelHeight, bitmap.DpiX, bitmap.DpiY, DestinationFormat, DestinationPalette, pixels, stride);
                bitmapSource.Freeze();
            }

            return bitmapSource;
        }

        private static void SwapColorsWithoutCloning(Brush brush, ColorCallback colorCallback)
        {
            if (brush != null)
            {
                var solidColorBrush = brush as SolidColorBrush;
                if (solidColorBrush != null)
                {
                    solidColorBrush.Color = colorCallback(solidColorBrush.Color);
                    return;
                }

                var gradientBrush = brush as GradientBrush;
                if (gradientBrush != null)
                {
                    foreach (GradientStop stop in gradientBrush.GradientStops)
                    {
                        stop.Color = colorCallback(stop.Color);
                    }
                    return;
                }

                var drawingBrush = brush as DrawingBrush;
                if (drawingBrush != null)
                {
                    SwapColorsWithoutCloning(drawingBrush.Drawing, colorCallback);
                    return;
                }

                var imageBrush = brush as ImageBrush;
                if (imageBrush != null)
                {
                    imageBrush.ImageSource = SwapColorsWithoutCloningIfPossible(imageBrush.ImageSource, colorCallback);
                    return;
                }

                Debug.Assert(brush is VisualBrush, "Unexpected type of brush");
            }
        }

        private static void SwapColorsWithoutCloning(Drawing drawing, ColorCallback colorCallback)
        {
            if (drawing != null)
            {
                DrawingGroup group = drawing as DrawingGroup;
                if (group != null)
                {
                    foreach (Drawing child in group.Children)
                    {
                        SwapColorsWithoutCloning(child, colorCallback);
                    }
                    return;
                }

                GeometryDrawing geometry = drawing as GeometryDrawing;
                if (geometry != null)
                {
                    SwapColorsWithoutCloning(geometry.Brush, colorCallback);
                    if (geometry.Pen != null)
                    {
                        SwapColorsWithoutCloning(geometry.Pen.Brush, colorCallback);
                    }
                    return;
                }

                GlyphRunDrawing glyphRun = drawing as GlyphRunDrawing;
                if (glyphRun != null)
                {
                    SwapColorsWithoutCloning(glyphRun.ForegroundBrush, colorCallback);
                    return;
                }

                ImageDrawing imageDrawing = drawing as ImageDrawing;
                if (imageDrawing != null)
                {
                    imageDrawing.ImageSource = SwapColorsWithoutCloningIfPossible(imageDrawing.ImageSource, colorCallback);
                    return;
                }

                Debug.Assert(drawing is VideoDrawing, "Unexpected type of drawing");
            }
        }

        private static ImageSource SwapColorsWithoutCloningIfPossible(ImageSource imageSource, ColorCallback colorCallback)
        {
            if (imageSource == null)
            {
                return null;
            }

            DrawingImage image = imageSource as DrawingImage;
            if (image != null)
            {
                SwapColorsWithoutCloning(image.Drawing, colorCallback);
                return imageSource;
            }

            BitmapSource bitmapSource = imageSource as BitmapSource;
            Debug.Assert(bitmapSource != null, "Expected DrawingImage or BitmapSource");
            return SwapColors(bitmapSource, colorCallback);
        }

        #endregion
    }
}
