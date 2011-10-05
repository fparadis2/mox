using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Mox.UI
{
    public class DropDownButton : ButtonBase
    {
        #region Dependency Properties

        public static readonly DependencyProperty DropDownContentProperty = DependencyProperty.Register("DropDownContent", typeof(object), typeof(DropDownButton));
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(DropDownButton));

        public object DropDownContent
        {
            get
            {
                return GetValue(DropDownContentProperty);
            }
            set
            {
                SetValue(DropDownContentProperty, value);
            }
        }

        public bool IsOpen
        {
            get
            {
                return (bool)GetValue(IsOpenProperty);
            }
            set
            {
                SetValue(IsOpenProperty, value);
            }
        }

        #endregion

        #region Constructor

        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
        }

        public DropDownButton()
        {
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion

        #region Methods

        private void CloseDropDown()
        {
            if (IsOpen)
            {
                IsOpen = false;
            }
            ReleaseMouseCapture();
        }

        #endregion

        #region Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseDropDown();
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseDropDown();
        }

        #endregion

    }

    public class FadeDropDownButton : DropDownButton
    {
        static FadeDropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FadeDropDownButton), new FrameworkPropertyMetadata(typeof(FadeDropDownButton)));
        }

    }
}
