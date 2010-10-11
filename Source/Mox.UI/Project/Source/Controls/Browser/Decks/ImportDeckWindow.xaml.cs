using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mox.UI.Browser;

namespace Mox.UI
{
	/// <summary>
	/// Interaction logic for ImportDeckWindow.xaml
	/// </summary>
	public partial class ImportDeckWindow : Dialog
    {
        #region Variables

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