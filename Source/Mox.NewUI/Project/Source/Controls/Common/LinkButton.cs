using System;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class LinkButton : Button
    {
        #region Dependency Properties
        
        #endregion

        #region Constructor

        static LinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkButton), new FrameworkPropertyMetadata(typeof(LinkButton)));
        }

        #endregion
    }
}
