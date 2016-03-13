using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mox.UI.Library
{
    /// <summary>
    /// Interaction logic for DeckListView.xaml
    /// </summary>
    public partial class DeckListView : UserControl
    {
        public DeckListView()
        {
            InitializeComponent();
        }

        private DeckLibraryViewModel Library
        {
            get
            {
                return (DeckLibraryViewModel)DataContext;
            }
        }

        private DeckViewModel GetDeck(object sender)
        {
            FrameworkElement source = (FrameworkElement)sender;
            return (DeckViewModel)source.DataContext;
        }

        private void EditClick(object sender, RoutedEventArgs e)
        {
            Library.EditDeck(GetDeck(sender));
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Library.AcceptDeck(GetDeck(sender));
        }
    }
}
