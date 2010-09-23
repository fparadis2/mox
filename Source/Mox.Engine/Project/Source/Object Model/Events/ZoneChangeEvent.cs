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

namespace Mox.Events
{
    public class ZoneChangeEvent
    {
        #region Variables

        private readonly Card m_card;
        private readonly Zone m_oldZone;
        private readonly Zone m_newZone;

        #endregion

        #region Constructor

        public ZoneChangeEvent(Card card, Zone oldZone, Zone newZone)
        {
            Throw.IfNull(card, "card");
            Throw.IfNull(oldZone, "oldZone");
            Throw.IfNull(newZone, "newZone");

            m_card = card;
            m_oldZone = oldZone;
            m_newZone = newZone;
        }

        #endregion

        #region Properties

        public Card Card
        {
            get { return m_card; }
        }

        public Zone OldZone
        {
            get { return m_oldZone; }
        }

        public Zone NewZone
        {
            get { return m_newZone; }
        }

        #endregion
    }
}
