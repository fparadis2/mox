using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class HeaderControl : ContentControl
    {
        static HeaderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderControl), new FrameworkPropertyMetadata(typeof(HeaderControl)));
        }
    }
}
