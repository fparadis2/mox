using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI
{
    public class IconButton : Button
    {
        #region Dependency Properties

        public static readonly DependencyProperty ImageBrushProperty = DependencyProperty.Register("ImageBrush", typeof(Brush), typeof(IconButton), new FrameworkPropertyMetadata(null));

        public Brush ImageBrush
        {
            get { return (Brush)GetValue(ImageBrushProperty); }
            set { SetValue(ImageBrushProperty, value); }
        }
        
        #endregion

        #region Constructor

        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }

        #endregion
    }
}
