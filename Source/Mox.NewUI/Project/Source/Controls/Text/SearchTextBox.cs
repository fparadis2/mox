using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Mox.UI
{
    [TemplatePart(Name = PART_ClearSearchButton, Type = typeof(ButtonBase))]
    public class SearchTextBox : HintTextBox
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

        private void OnClearSearchButtonClick(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }

        #endregion
    }
}
