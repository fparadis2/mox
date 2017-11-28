using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class BarHeaderControl : ContentControl
    {
        static BarHeaderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BarHeaderControl), new FrameworkPropertyMetadata(typeof(BarHeaderControl)));
        }
    }
}
