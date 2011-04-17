using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    /// <summary>
    /// The purpose of this control is to be able to restyle the top part of a TabControl.
    /// </summary>
    public class HeaderedTabControl : TabControl
    {
        #region Dependency Properties

        public static DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(ControlTemplate), typeof(HeaderedTabControl));

        public ControlTemplate HeaderTemplate
        {
            get { return (ControlTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        #endregion

        #region Constructor

        static HeaderedTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderedTabControl), new FrameworkPropertyMetadata(typeof(HeaderedTabControl)));
        }

        #endregion
    }
}
