using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public static class TextBoxHelper
    {
        #region Button

        public static readonly DependencyProperty ButtonTemplateProperty = DependencyProperty.RegisterAttached(
            "ButtonTemplate", typeof(DataTemplate), typeof(TextBoxHelper), new PropertyMetadata(default(DataTemplate)));

        public static void SetButtonTemplate(DependencyObject element, DataTemplate value)
        {
            element.SetValue(ButtonTemplateProperty, value);
        }

        public static DataTemplate GetButtonTemplate(DependencyObject element)
        {
            return (DataTemplate)element.GetValue(ButtonTemplateProperty);
        }

        public static readonly DependencyProperty IsClearButtonProperty = DependencyProperty.RegisterAttached(
            "IsClearButton", typeof (bool), typeof (TextBoxHelper), new PropertyMetadata(default(bool), OnIsClearButtonChanged));

        public static void SetIsClearButton(DependencyObject element, bool value)
        {
            element.SetValue(IsClearButtonProperty, value);
        }

        public static bool GetIsClearButton(DependencyObject element)
        {
            return (bool) element.GetValue(IsClearButtonProperty);
        }

        private static void OnIsClearButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Button button = d as Button;
            if (button != null)
            {
                if ((bool)e.NewValue)
                {
                    button.Click += WhenClearButtonClicked;
                }
                else
                {
                    button.Click -= WhenClearButtonClicked;
                }
            }
        }

        private static void WhenClearButtonClicked(object sender, RoutedEventArgs e)
        {
            Button button = (Button) sender;
            var textBox = button.FindVisualParent<TextBox>();
            if (textBox != null)
            {
                textBox.Clear();
            }
        }

        #endregion

        #region Text Length

        public static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "HasText", typeof (bool), typeof (TextBoxHelper), new PropertyMetadata(default(bool)));

        private static void SetHasText(DependencyObject element, bool value)
        {
            element.SetValue(HasTextPropertyKey, value);
        }

        public static bool GetHasText(DependencyObject element)
        {
            return (bool)element.GetValue(HasTextPropertyKey.DependencyProperty);
        }
        
        #endregion

        #region IsMonitoring

        public static readonly DependencyProperty IsMonitoringProperty = DependencyProperty.RegisterAttached("IsMonitoring", typeof (bool), typeof (TextBoxHelper), new PropertyMetadata(default(bool), OnIsMonitoringChanged));

        public static void SetIsMonitoring(DependencyObject element, bool value)
        {
            element.SetValue(IsMonitoringProperty, value);
        }

        public static bool GetIsMonitoring(DependencyObject element)
        {
            return (bool) element.GetValue(IsMonitoringProperty);
        }

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as TextBox;
            if (textBox != null)
            {
                if ((bool) e.NewValue)
                {
                    textBox.TextChanged += WhenTextChanged;

                    textBox.Dispatcher.BeginInvoke(new System.Action(() => WhenTextChanged(textBox, new TextChangedEventArgs(TextBox.TextChangedEvent, UndoAction.None))));
                }
                else
                {
                    textBox.TextChanged -= WhenTextChanged;
                }
            }
        }

        private static void WhenTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTextLength<TextBox>(sender, t => t.Text.Length);
        }

        private static void UpdateTextLength<T>(object sender, Func<T, int> lengthFunctor)
            where T : DependencyObject
        {
            if (sender is T)
            {
                DependencyObject obj = (DependencyObject) sender;
                var value = lengthFunctor((T)obj);
                SetHasText(obj, value > 0);
            }
        }

        #endregion
    }
}
