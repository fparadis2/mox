using Mox.Database;

namespace Mox.UI.Library
{
    public class DeckCardViewModel
    {
        private readonly CardInstanceInfo m_card;

        public DeckCardViewModel(CardIdentifier card, int quantity)
        {
            Card = card;
            Quantity = quantity;
        }

        public CardIdentifier Card
        {
            get; 
            private set; 
        }

        public int Quantity
        {
            get;
            private set;
        }

        public string Text
        {
            get { return string.Format("{0} {1}", Quantity, Card.Card); }
        }

        private CardInstanceInfo m_cardInstanceInfo;
        public CardInstanceInfo CardInstanceInfo
        {
            get
            {
                if (m_cardInstanceInfo == null)
                {
                    m_cardInstanceInfo = MasterCardDatabase.Instance.GetCardInstance(Card);
                }

                return m_cardInstanceInfo;
            }
        }

        public bool IsValid
        {
            get { return CardInstanceInfo != null; }
        }

        public ImageKey Image
        {
            get { return ImageKey.ForCardImage(CardInstanceInfo, false); }
        }
    }
}