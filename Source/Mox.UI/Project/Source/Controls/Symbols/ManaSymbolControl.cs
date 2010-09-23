using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class ManaSymbolControl : Control
    {
        #region Constructor

        static ManaSymbolControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ManaSymbolControl), new FrameworkPropertyMetadata(typeof(ManaSymbolControl)));
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ManaSymbolProperty = DependencyProperty.Register("ManaSymbol", typeof(ManaSymbol), typeof(ManaSymbolControl), new PropertyMetadata(OnManaSymbolChanged));

        public ManaSymbol ManaSymbol
        {
            get { return (ManaSymbol)GetValue(ManaSymbolProperty); }
            set { SetValue(ManaSymbolProperty, value); }
        }

        public static readonly DependencyProperty ManaSymbolImageSourceProperty = DependencyProperty.Register("ManaSymbolImageSource", typeof(IAsyncImage), typeof(ManaSymbolControl));

        public IAsyncImage ManaSymbolImageSource
        {
            get { return (IAsyncImage)GetValue(ManaSymbolImageSourceProperty); }
            set { SetValue(ManaSymbolImageSourceProperty, value); }
        }

        #endregion

        #region Event Handlers

        private static void OnManaSymbolChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ManaSymbolControl)sender).ManaSymbolImageSource = ImageService.GetManaSymbolImage((ManaSymbol)e.NewValue, ImageSize.Large);
        }

        #endregion
    }
}
