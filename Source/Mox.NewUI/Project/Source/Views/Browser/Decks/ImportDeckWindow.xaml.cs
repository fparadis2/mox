using System;
using System.Windows;

namespace Mox.UI.Browser
{
	/// <summary>
	/// Interaction logic for ImportDeckWindow.xaml
	/// </summary>
	public partial class ImportDeckWindow : Window
    {
        #region Constructor

        public ImportDeckWindow()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.
        }

        #endregion

        #region Properties

        public new ImportDeckViewModel DataContext
        {
            get { return (ImportDeckViewModel)base.DataContext; }
            set { base.DataContext = value; }
        }

        #endregion

        #region Event Handlers

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #endregion
	}
}