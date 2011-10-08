using System;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Lobby
{
    public class DeckViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly Deck m_deck;

        #endregion

        #region Constructor

        public DeckViewModel(Deck deck)
        {
            m_deck = deck;
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return m_deck.Name; }
        }

        public Deck Deck
        {
            get 
            {
                return m_deck;
            }
        }

        #endregion
    }
}
