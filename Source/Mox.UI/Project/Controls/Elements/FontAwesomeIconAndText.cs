using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Mox.UI
{
    [ContentProperty("Content")]
    public class FontAwesomeIconAndText : Control
    {
        static FontAwesomeIconAndText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontAwesomeIconAndText), new FrameworkPropertyMetadata(typeof(FontAwesomeIconAndText)));
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof (object), typeof (FontAwesomeIconAndText), new PropertyMetadata(default(object)));

        public object Content
        {
            get { return (object) GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof (FontAwesome.WPF.FontAwesomeIcon), typeof (FontAwesomeIconAndText), new PropertyMetadata(default(FontAwesome.WPF.FontAwesomeIcon)));

        public FontAwesome.WPF.FontAwesomeIcon Icon
        {
            get { return (FontAwesome.WPF.FontAwesomeIcon) GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
    }
}
