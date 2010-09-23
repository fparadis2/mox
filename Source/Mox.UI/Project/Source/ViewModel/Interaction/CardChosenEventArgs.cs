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

namespace Mox.UI
{
    /// <summary>
    /// Event args used when a card is choosed by the user.
    /// </summary>
    public class CardChosenEventArgs : EventArgs
    {
        #region Variables

        private readonly CardViewModel m_card;

        #endregion

        #region Constructor

        public CardChosenEventArgs(CardViewModel card)
        {
            m_card = card;
        }

        #endregion

        #region Properties

        public CardViewModel Card
        {
            get { return m_card; }
        }

        #endregion
    }
}
