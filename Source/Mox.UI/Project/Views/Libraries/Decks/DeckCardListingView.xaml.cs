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
using System.Windows.Threading;

namespace Mox.UI.Library
{
    /// <summary>
    /// Interaction logic for DeckCardListingView.xaml
    /// </summary>
    public partial class DeckCardListingView : UserControl
    {
        private readonly DispatcherTimer m_cardHoverTimer = new DispatcherTimer();
        private DeckCardViewModel m_hoveredCard;

        public DeckCardListingView()
        {
            InitializeComponent();

            m_cardHoverTimer.Interval = TimeSpan.FromSeconds(0.2);
            m_cardHoverTimer.Tick += WhenCardHoverTimerTick;
        }

        private void OnCardMouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement cardElement = (FrameworkElement) sender;
            m_hoveredCard = (DeckCardViewModel)cardElement.DataContext;
            m_cardHoverTimer.Start();
        }

        private void OnCardMouseLeave(object sender, MouseEventArgs e)
        {
            m_hoveredCard = null;
            m_cardHoverTimer.Stop();
        }

        private void WhenCardHoverTimerTick(object sender, EventArgs e)
        {
            DeckViewModel deck = (DeckViewModel)DataContext;

            deck.HoveredCard = m_hoveredCard;
            m_cardHoverTimer.Stop();
        }
    }
}
