using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mox.UI
{
    public class HintTextBox : TextBox
    {
        #region Constructor

        static HintTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HintTextBox), new FrameworkPropertyMetadata(typeof(HintTextBox)));
        }

        #endregion

        #region Dependency Properties

        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string), typeof(HintTextBox), new FrameworkPropertyMetadata("Search"));

        public bool HasText
        {
            get
            {
                return (bool)GetValue(HasTextProperty);
            }
        }

        private static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterReadOnly("HasText", typeof(bool), typeof(HintTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        #endregion

        #region Methods

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            bool actuallyHasText = Text.Length > 0;
            if (actuallyHasText != HasText)
            {
                SetValue(HasTextPropertyKey, actuallyHasText);
            }
        }

        #endregion
    }
}
