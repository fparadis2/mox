using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mox.UI.Browser
{
	/// <summary>
    /// Interaction logic for CardBrowserPage.xaml
	/// </summary>
	public partial class CardBrowserPage : UserControl
	{
        public CardBrowserPage()
		{
			this.InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                cardsList.DataContext = new CardLibraryViewModel();
            }
		}
	}
}