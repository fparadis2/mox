using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI.Browser
{
    public enum CardControlStyle
    {
        Full,
        Simple
    }

    public class CardControl : Control
    {
        #region Dependency Properties

        public static readonly DependencyProperty TitleColorProperty = DependencyProperty.RegisterAttached("TitleColor", typeof(System.Windows.Media.Color), typeof(CardControl), new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.Inherits));

        public System.Windows.Media.Color TitleColor
        {
            get { return (System.Windows.Media.Color)GetValue(TitleColorProperty); }
            set { SetValue(TitleColorProperty, value); }
        }

        public static readonly DependencyProperty CardControlStyleProperty = DependencyProperty.Register("CardControlStyle", typeof (CardControlStyle), typeof (CardControl));

        public CardControlStyle CardControlStyle
        {
            get { return (CardControlStyle)GetValue(CardControlStyleProperty); }
            set { SetValue(CardControlStyleProperty, value); }
        }

        #endregion

        #region Constructor

        static CardControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CardControl), new FrameworkPropertyMetadata(typeof(CardControl)));
        }

        #endregion
    }
}
