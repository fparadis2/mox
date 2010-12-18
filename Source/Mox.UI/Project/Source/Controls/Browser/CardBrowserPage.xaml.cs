using System;
using System.ComponentModel;
using System.Windows.Controls;

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
                cardsList.DataContext = CardCollectionViewModel.FromMaster();
            }
		}
	}
}