using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mox.Database;
using Mox.UI;
using Mox.UI.ImageGenerator;

namespace Mox.TestCardRendering.Source
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public readonly List<CardInstanceInfo> Cards = new List<CardInstanceInfo>();
        public ICollectionView CardsView { get; set; }

        public TestWindow()
        {
            var database = MasterCardDatabase.Instance;
            var set = database.Sets["10E"];
            Cards = new List<CardInstanceInfo>(set.CardInstances);

            CardsView = CollectionViewSource.GetDefaultView(Cards);
            CardsView.Filter = FilterCards;

            DataContext = this;
            InitializeComponent();
        }

        private string m_searchText;
        public string SearchText
        {
            get { return m_searchText; }
            set
            {
                m_searchText = value;
                CardsView.Refresh();
            }
        }

        private bool FilterCards(object obj)
        {
            if (string.IsNullOrEmpty(m_searchText))
                return true;

            CardInstanceInfo card = (CardInstanceInfo) obj;
            if (card.Card.Name.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (card.Card.TypeLine.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (!string.IsNullOrEmpty(card.Card.Text) && card.Card.Text.IndexOf(m_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }
    }
}
