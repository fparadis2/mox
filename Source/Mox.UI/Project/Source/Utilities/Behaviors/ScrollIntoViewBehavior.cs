using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mox.UI
{
    public class ScrollIntoViewBehavior
    {
        #region Properties

        public static readonly DependencyProperty WhenSelectedProperty = DependencyProperty.RegisterAttached("WhenSelected", typeof(bool), typeof(ScrollIntoViewBehavior), new PropertyMetadata(false, OnWhenSelectedChanged));

        public static bool GetWhenSelected(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(WhenSelectedProperty);
        }

        public static void SetWhenSelected(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(WhenSelectedProperty, value);
        }

        #endregion

        #region Event Handlers

        private static void OnWhenSelectedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var element = (Selector)dependencyObject;
            
            if ((bool)e.NewValue)
            {
                element.SelectionChanged += SelectionChanged;
            }
            else
            {
                element.SelectionChanged -= SelectionChanged;
            }
        }

        static void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Selector element = (Selector)sender;

            var item = element.ItemContainerGenerator.ContainerFromItem(element.SelectedItem) as FrameworkElement;
            if (item != null)
            {
                item.BringIntoView();
            }
        }

        #endregion
    }
}
