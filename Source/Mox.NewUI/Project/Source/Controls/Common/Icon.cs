using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    public class Icon : Image
    {
        #region Dependency Properties

        public static readonly DependencyProperty BlueChromaProperty = DependencyProperty.RegisterAttached("BlueChroma", typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty GreenChromaProperty = DependencyProperty.RegisterAttached("GreenChroma", typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty RedChromaProperty = DependencyProperty.RegisterAttached("RedChroma", typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SourceBrushProperty = DependencyProperty.Register("SourceBrush", typeof(DrawingBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Constructor

        static Icon()
        {
            StretchProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(Stretch.None));
        }

        #endregion

        #region Properties

        public SolidColorBrush BlueChroma
        {
            get
            {
                return (SolidColorBrush)GetValue(BlueChromaProperty);
            }
            set
            {
                SetValue(BlueChromaProperty, value);
            }
        }

        public SolidColorBrush GreenChroma
        {
            get
            {
                return (SolidColorBrush)GetValue(GreenChromaProperty);
            }
            set
            {
                SetValue(GreenChromaProperty, value);
            }
        }

        public SolidColorBrush RedChroma
        {
            get
            {
                return (SolidColorBrush)GetValue(RedChromaProperty);
            }
            set
            {
                SetValue(RedChromaProperty, value);
            }
        }

        public DrawingBrush SourceBrush
        {
            get
            {
                return (DrawingBrush)GetValue(SourceBrushProperty);
            }
            set
            {
                SetValue(SourceBrushProperty, value);
            }
        }

        private ImageSource RenderSource
        {
            get
            {
                if (Source == null)
                {
                    return null;
                }

                if (RedChroma == null && GreenChroma == null && BlueChroma == null)
                {
                    return Source;
                }

                return ColorSwapper.SwapColors(Source, ConvertColor);
            }
        }

        private DrawingBrush RenderSourceBrush
        {
            get
            {
                if (SourceBrush == null)
                {
                    return null;
                }

                if (RedChroma == null && GreenChroma == null && BlueChroma == null)
                {
                    return SourceBrush;
                }

                return (DrawingBrush)ColorSwapper.SwapColors(SourceBrush, ConvertColor);
            }
        }

        #endregion

        #region Methods

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Source == null)
            {
                return finalSize;
            }

            return base.ArrangeOverride(finalSize);
        }

        private System.Windows.Media.Color ConvertColor(System.Windows.Media.Color color)
        {
            if (color.R != color.G || color.R != color.B)
            {
                if (color.G == color.B && RedChroma != null)
                {
                    return ScaleColor(RedChroma.Color, color.R, color.G, color.A);
                }
                if (color.R == color.B && GreenChroma != null)
                {
                    return ScaleColor(GreenChroma.Color, color.G, color.R, color.A);
                }
                if (color.R == color.G && BlueChroma != null)
                {
                    return ScaleColor(BlueChroma.Color, color.B, color.R, color.A);
                }
            }

            return color;
        }

        public static SolidColorBrush GetBlueChroma(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(BlueChromaProperty);
        }

        public static void SetBlueChroma(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(BlueChromaProperty, value);
        }

        public static SolidColorBrush GetGreenChroma(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(GreenChromaProperty);
        }

        public static void SetGreenChroma(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(GreenChromaProperty, value);
        }

        public static SolidColorBrush GetRedChroma(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(RedChromaProperty);
        }

        public static void SetRedChroma(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(RedChromaProperty, value);
        }

        public static DrawingBrush GetSourceBrush(DependencyObject obj)
        {
            return (DrawingBrush)obj.GetValue(SourceBrushProperty);
        }

        public static void SetSourceBrush(DependencyObject obj, DrawingBrush value)
        {
            obj.SetValue(SourceBrushProperty, value);
        }

        public static Point GetPixelSnappingOffset(Visual visual)
        {
            PresentationSource source = PresentationSource.FromVisual(visual);
            if (source != null)
            {
                return GetPixelSnappingOffset(visual, source.RootVisual);
            }
            return new Point();
        }

        private static Point GetPixelSnappingOffset(Visual visual, Visual rootVisual)
        {
            Point point = new Point();
            if (rootVisual != null)
            {
                Transform transform = visual.TransformToAncestor(rootVisual) as Transform;
                if ((transform != null) && transform.Value.HasInverse)
                {
                    point = visual.PointFromScreen(visual.PointToScreen(point));
                }
            }
            return point;
        }

        private static bool IsClose(double num1, double num2)
        {
            return num1 > num2 * 0.9;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect rectangle = new Rect(0.0, 0.0, RenderSize.Width, RenderSize.Height);

            BitmapSource source = Source as BitmapSource;
            if (SourceBrush == null || (source != null && IsClose(source.Width, RenderSize.Width) && IsClose(source.Height, RenderSize.Height)))
            {
                ImageSource renderSource = RenderSource;
                if (renderSource != null)
                {
                    drawingContext.DrawImage(renderSource, rectangle);
                }
            }
            else
            {
                drawingContext.DrawRectangle(RenderSourceBrush, null, rectangle);
            }
        }

        private static System.Windows.Media.Color ScaleColor(System.Windows.Media.Color color, byte primary, byte white, byte alpha)
        {
            return System.Windows.Media.Color.FromArgb(
                (byte)(((double)alpha * color.A) / 0xff),
                (byte)((((double)color.R / 0xff) * (primary - white)) + white),
                (byte)((((double)color.G / 0xff) * (primary - white)) + white),
                (byte)((((double)color.B / 0xff) * (primary - white)) + white));
        }

        #endregion
    }
}
