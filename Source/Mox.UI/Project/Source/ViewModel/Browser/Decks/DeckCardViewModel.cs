// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
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
