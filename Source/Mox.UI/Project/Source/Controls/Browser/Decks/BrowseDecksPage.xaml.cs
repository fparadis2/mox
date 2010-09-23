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
using Mox.Database;

namespace Mox.UI.Browser
{
	/// <summary>
	/// Interaction logic for BrowseDecksPage.xaml
	/// </summary>
	public partial class BrowseDecksPage : UserControl
    {
        #region Constructor

        public BrowseDecksPage()
		{
			this.InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new BrowseDecksPageViewModel(MasterDeckLibrary.Instance, MasterCardDatabase.Instance);
            }
		}

        #endregion

        #region Properties

        public new BrowseDecksPageViewModel DataContext
        {
            get { return (BrowseDecksPageViewModel)base.DataContext; }
            set { base.DataContext = value; }
        }

        #endregion

        #region Event Handlers

        private void ImportDeck_Click(object sender, RoutedEventArgs e)
        {
            ImportDeckViewModel importViewModel = new ImportDeckViewModel(MasterCardDatabase.Instance);

#warning Set initial value from clipboard if compatible

            ImportDeckWindow importWindow = new ImportDeckWindow
            {
                DataContext = importViewModel
            };
            
            if (importWindow.ShowDialog() == true)
            {
                DataContext.Library.Add(importViewModel.Import());
            }
        }

        #endregion
    }
}