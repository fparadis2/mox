using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mox.UI
{
    [TemplatePart(Name = PART_ClearSearchButton, Type = typeof(ButtonBase))]
    public class SearchTextBox : TextBox
    {
        #region Constants

        private const string PART_ClearSearchButton = "PART_ClearSearchButton";

        #endregion

        #region Constructor

        static SearchTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTextBox), new FrameworkPropertyMetadata(typeof(SearchTextBox)));
        }

        #endregion

        #region Dependency Properties

        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string), typeof(SearchTextBox), new FrameworkPropertyMetadata("Search"));

        public bool HasText
        {
            get
            {
                return (bool)GetValue(HasTextProperty);
            }
        }

        private static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterReadOnly("HasText", typeof(bool), typeof(SearchTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var searchButton = GetTemplateChild(PART_ClearSearchButton) as ButtonBase;
            if (searchButton != null)
            {
                searchButton.Click += OnClearSearchButtonClick;
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            bool actuallyHasText = Text.Length > 0;
            if (actuallyHasText != HasText)
            {
                SetValue(HasTextPropertyKey, actuallyHasText);
            }
        }

        private void OnClearSearchButtonClick(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }

        #endregion
    }
}
