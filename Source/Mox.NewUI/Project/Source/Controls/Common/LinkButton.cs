using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI
{
    public class LinkButton : Button
    {
        #region Dependency Properties

        public static readonly DependencyProperty ImageBrushProperty = DependencyProperty.Register("ImageBrush", typeof(Brush), typeof(LinkButton), new FrameworkPropertyMetadata(null));

        public Brush ImageBrush
        {
            get { return (Brush)GetValue(ImageBrushProperty); }
            set { SetValue(ImageBrushProperty, value); }
        }
        
        #endregion

        #region Constructor

        static LinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkButton), new FrameworkPropertyMetadata(typeof(LinkButton)));
        }

        #endregion
    }
}
