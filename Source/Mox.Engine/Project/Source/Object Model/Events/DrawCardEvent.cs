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

namespace Mox.Events
{
    public class DrawCardEvent
    {
        #region Variables

        private readonly Player m_player;
        private readonly Card m_card;

        #endregion

        #region Constructor

        public DrawCardEvent(Player player, Card card)
        {
            Throw.IfNull(player, "player");
            Throw.IfNull(card, "card");

            m_player = player;
            m_card = card;
        }

        #endregion

        #region Properties

        public Player Player
        {
            get { return m_player; }
        }

        public Card Card
        {
            get { return m_card; }
        }

        #endregion
    }
}
