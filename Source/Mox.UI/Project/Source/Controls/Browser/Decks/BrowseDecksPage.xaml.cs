using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
                DataContext = new BrowseDecksPageViewModel(MasterDeckLibrary.Instance, EditDeckViewModel.FromMaster());
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

        private void NewDeck_Click(object sender, RoutedEventArgs e)
        {
            var deckModel = DataContext.Library.Add(new Deck());
            deckModel.Edit();
        }

        private void ImportDeck_Click(object sender, RoutedEventArgs e)
        {
            ImportDeckViewModel importViewModel = new ImportDeckViewModel(MasterCardDatabase.Instance);

            UseClipboardContentIfPossible(importViewModel);
            
            ImportDeckWindow importWindow = new ImportDeckWindow
            {
                DataContext = importViewModel
            };
            
            if (importWindow.ShowDialog() == true)
            {
                DataContext.Library.Add(importViewModel.Import());
            }
        }

        private static void UseClipboardContentIfPossible(ImportDeckViewModel importViewModel)
        {
            if (Clipboard.ContainsText())
            {
                importViewModel.Text = Clipboard.GetText();

                if (!importViewModel.CanImport)
                {
                    importViewModel.Text = string.Empty;
                }
            }
        }

        #endregion
    }
}