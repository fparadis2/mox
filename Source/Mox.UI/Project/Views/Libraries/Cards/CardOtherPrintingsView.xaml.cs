using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for CardOtherPrintingsView.xaml
    /// </summary>
    public partial class CardOtherPrintingsView : UserControl
    {
        public CardOtherPrintingsView()
        {
            InitializeComponent();
        }

        private void WhenImageMouseUp(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = (FrameworkElement)sender;
            var itemsControl = source.FindVisualParent<ItemsControl>();

            if (itemsControl != null)
            {
                var cardPrinting = (CardPrintingViewModel)source.DataContext;
                var card = (CardViewModel) itemsControl.DataContext;

                card.CurrentPrinting = cardPrinting;
            }
        }
    }
}
