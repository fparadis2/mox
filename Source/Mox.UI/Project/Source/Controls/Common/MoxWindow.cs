using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mox.UI
{
    public class MoxWindow : Window
    {
        #region Constants

        private const string PART_Caption = "PART_Caption";
        private const string PART_Close = "PART_Close";

        #endregion

        #region Constructor

        static MoxWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MoxWindow), new FrameworkPropertyMetadata(typeof(MoxWindow)));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Determines if window should override the chorme.
        /// </summary>
        public virtual bool IsOverridingWindowsChrome
        {
            get { return true; }
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (IsOverridingWindowsChrome)
            {
                GetTemplatedChild<FrameworkElement>(PART_Caption, e => e.MouseLeftButtonDown += DoDragMove);
                GetTemplatedChild<Button>(PART_Close, e=> e.Click += Close);
            }
        }

        private void GetTemplatedChild<TElement>(string elementName, Action<TElement> action) where TElement : DependencyObject
        {
            var element = GetTemplateChild(elementName) as TElement;
            if (element != null)
            {
                action(element);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            if (IsOverridingWindowsChrome)
            {
                SetResourceReference(StyleProperty, "WindowsChromeOverride");
            }

            base.OnInitialized(e);
        }

        private void Close(object sender, EventArgs e)
        {
            Close();
        }

        private void DoDragMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        #endregion
    }
}
