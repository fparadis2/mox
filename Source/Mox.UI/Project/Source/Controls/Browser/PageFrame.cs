using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI.Browser
{
    public class PageFrame : ContentControl
    {
        static PageFrame()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PageFrame), new FrameworkPropertyMetadata(typeof(PageFrame)));
        }
    }
}
