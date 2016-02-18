using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mox.UI
{
    public class PageControl : ContentControl
    {
        static PageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PageControl), new FrameworkPropertyMetadata(typeof(PageControl)));
        }

        #region Dependency Properties

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            "Footer", typeof (object), typeof (PageControl), new PropertyMetadata(default(object)));

        public object Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        #endregion
    }
}
