using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI.Browser
{
    public class DeckControl : Control
    {
        #region Dependency Properties

        public static readonly DependencyProperty AuthorForegroundProperty = DependencyProperty.Register("AuthorForeground", typeof(Brush), typeof(DeckControl), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Gray)));
        public static readonly DependencyProperty ShowContextButtonsProperty = DependencyProperty.Register("ShowContextButtons", typeof (bool), typeof (DeckControl), new FrameworkPropertyMetadata(false));
        
        #endregion

        #region Constructor

        static DeckControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DeckControl), new FrameworkPropertyMetadata(typeof(DeckControl)));
        }

        #endregion

        #region Properties

        public bool ShowContextButtons
        {
            get { return (bool)GetValue(ShowContextButtonsProperty); }
            set { SetValue(ShowContextButtonsProperty, value); }
        }

        #endregion
    }
}
