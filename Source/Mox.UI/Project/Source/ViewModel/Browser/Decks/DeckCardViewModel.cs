using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class DeckCardViewModel : CardViewModel
    {
        #region Variables

        private readonly DeckViewModel m_owner;
        private readonly CardIdentifier m_identifier;

        #endregion

        #region Constructor

        public DeckCardViewModel(DeckViewModel owner, CardIdentifier cardIdentifier)
            : base(owner.Editor.Database.Cards[cardIdentifier.Card])
        {
            m_owner = owner;
            m_identifier = cardIdentifier;
        }

        #endregion

        #region Properties

        public int Quantity
        {
            get { return m_owner.Deck.Cards[m_identifier]; }
            set
            {
                if (Quantity != value && value > 0)
                {
                    m_owner.Modify(deck => deck.Cards[m_identifier] = value);
                }
            }
        }

        #endregion
    }
}
