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
	/// Interaction logic for EditDeckPage.xaml
	/// </summary>
	public partial class EditDeckPage : UserControl
    {
        #region Constructor

        public EditDeckPage()
		{
			this.InitializeComponent();
		}

        #endregion

        #region Properties

        public new EditDeckPageViewModel DataContext
        {
            get { return (EditDeckPageViewModel)base.DataContext; }
            set { base.DataContext = value; }
        }

        #endregion
    }
}