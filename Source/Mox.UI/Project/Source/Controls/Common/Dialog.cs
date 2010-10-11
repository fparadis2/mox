using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class Dialog : MoxWindow
    {
        static Dialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Dialog), new FrameworkPropertyMetadata(typeof(Dialog)));
        }
    }
}
