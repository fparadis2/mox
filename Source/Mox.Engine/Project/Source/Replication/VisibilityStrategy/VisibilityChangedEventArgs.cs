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

namespace Mox.Replication
{
    public class VisibilityChangedEventArgs : EventArgs
    {
        #region Variables

        private readonly Object m_object;
        private readonly Player m_player;
        private readonly bool m_visibility;

        #endregion

        #region Constructor

        public VisibilityChangedEventArgs(Object obj, Player player, bool visibility)
        {
            m_object = obj;
            m_player = player;
            m_visibility = visibility;
        }

        #endregion

        #region Properties

        public Object Object
        {
            get { return m_object; }
        }

        public Player Player
        {
            get { return m_player; }
        }

        public bool Visibility
        {
            get { return m_visibility; }
        }

        #endregion
    }
}
