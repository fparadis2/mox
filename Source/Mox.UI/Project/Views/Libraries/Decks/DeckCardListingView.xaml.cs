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
    /// Interaction logic for DeckCardListingView.xaml
    /// </summary>
    public partial class DeckCardListingView : UserControl
    {
        public DeckCardListingView()
        {
            InitializeComponent();
        }

        private void OnCardMouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement cardElement = (FrameworkElement) sender;
            DeckCardViewModel card = (DeckCardViewModel)cardElement.DataContext;

            DeckCardListingView listingView = cardElement.FindVisualParent<DeckCardListingView>();
            DeckViewModel deck = (DeckViewModel) listingView.DataContext;

            deck.HoveredCard = card;
        }
    }
}
