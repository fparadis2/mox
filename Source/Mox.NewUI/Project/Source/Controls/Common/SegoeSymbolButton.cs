using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class SegoeSymbolButton : Button
    {
        static SegoeSymbolButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SegoeSymbolButton), new FrameworkPropertyMetadata(typeof(SegoeSymbolButton)));
        }

        public static readonly DependencyProperty SymbolTextProperty = DependencyProperty.Register(
            "SymbolText", typeof (string), typeof (SegoeSymbolButton), new PropertyMetadata(default(string)));

        public string SymbolText
        {
            get { return (string) GetValue(SymbolTextProperty); }
            set { SetValue(SymbolTextProperty, value); }
        }

        public static readonly DependencyProperty ShowEllipseProperty = DependencyProperty.Register(
            "ShowEllipse", typeof (bool), typeof (SegoeSymbolButton), new PropertyMetadata(true));

        public bool ShowEllipse
        {
            get { return (bool) GetValue(ShowEllipseProperty); }
            set { SetValue(ShowEllipseProperty, value); }
        }
    }
}
