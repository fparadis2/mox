using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Mox.UI.Shaders
{
    public class VignetteEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(VignetteEffect), 0);
        public static readonly DependencyProperty SmoothnessProperty = DependencyProperty.Register("Smoothness", typeof(double), typeof(VignetteEffect), new UIPropertyMetadata(((double)(0.02D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty RoundnessProperty = DependencyProperty.Register("Roundness", typeof(double), typeof(VignetteEffect), new UIPropertyMetadata(((double)(18D)), PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(VignetteEffect), new UIPropertyMetadata(((double)(0.47D)), PixelShaderConstantCallback(2)));
        public static readonly DependencyProperty AspectRatioProperty = DependencyProperty.Register("AspectRatio", typeof(double), typeof(VignetteEffect), new UIPropertyMetadata(((double)(1D)), PixelShaderConstantCallback(3)));
        public static readonly DependencyProperty VignetteColorProperty = DependencyProperty.Register("VignetteColor", typeof(System.Windows.Media.Color), typeof(VignetteEffect), new UIPropertyMetadata(System.Windows.Media.Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(4)));

        public VignetteEffect()
        {
            PixelShader pixelShader = new PixelShader();
            pixelShader.UriSource = new Uri("/Mox.NewUI;component/Source/Shaders/vignette.ps", UriKind.Relative);
            this.PixelShader = pixelShader;

            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(SmoothnessProperty);
            this.UpdateShaderValue(RoundnessProperty);
            this.UpdateShaderValue(RadiusProperty);
            this.UpdateShaderValue(AspectRatioProperty);
            this.UpdateShaderValue(VignetteColorProperty);
        }

        public Brush Input
        {
            get { return ((Brush) (this.GetValue(InputProperty))); }
            set { this.SetValue(InputProperty, value); }
        }

        public double Smoothness
        {
            get { return ((double) (this.GetValue(SmoothnessProperty))); }
            set { this.SetValue(SmoothnessProperty, value); }
        }

        public double Roundness
        {
            get { return ((double) (this.GetValue(RoundnessProperty))); }
            set { this.SetValue(RoundnessProperty, value); }
        }

        public double Radius
        {
            get { return ((double) (this.GetValue(RadiusProperty))); }
            set { this.SetValue(RadiusProperty, value); }
        }

        public double AspectRatio
        {
            get { return ((double) (this.GetValue(AspectRatioProperty))); }
            set { this.SetValue(AspectRatioProperty, value); }
        }

        public System.Windows.Media.Color VignetteColor
        {
            get { return ((System.Windows.Media.Color)(this.GetValue(VignetteColorProperty))); }
            set { this.SetValue(VignetteColorProperty, value); }
        }
    }
}
