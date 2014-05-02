using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI.Game
{
    public class SpellControl : Control
    {
        static SpellControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpellControl), new FrameworkPropertyMetadata(typeof(SpellControl)));
        }
    }
}
