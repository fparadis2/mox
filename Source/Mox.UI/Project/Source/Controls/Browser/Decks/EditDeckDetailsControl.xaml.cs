using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for EditDeckDetailsControl.xaml
    /// </summary>
    public partial class EditDeckDetailsControl : UserControl
    {
        public EditDeckDetailsControl()
        {
            InitializeComponent();
        }

        #region Event Handlers

        private void EditNameButton_Click(object sender, RoutedEventArgs e)
        {
            NameTextBox.IsInEditMode = true;
        }

        private void EditAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorTextBox.IsInEditMode = true;
        }

        private void EditDescriptionButton_Click(object sender, RoutedEventArgs e)
        {
            DescriptionTextBox.IsInEditMode = true;
        }

        #endregion
    }
}
