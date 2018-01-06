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

namespace Mox.Abilities
{
    /// <summary>
    /// Represents a triggered ability that was triggered and is waiting to be played.
    /// </summary>
    [Serializable]
    public struct QueuedTriggeredAbility
    {
        #region Variables

        private readonly Resolvable<TriggeredAbility> m_ability;
        private readonly Resolvable<Player> m_controller;
        private readonly object m_context;

        #endregion

        #region Constructor

        public QueuedTriggeredAbility(TriggeredAbility ability, Player controller, object context)
        {
            Throw.IfNull(ability, "ability");
            Throw.IfNull(controller, "controller");

            m_ability = ability;
            m_controller = controller;
            m_context = context;
        }

        #endregion

        #region Properties

        public Resolvable<TriggeredAbility> Ability
        {
            get { return m_ability; }
        }

        public Resolvable<Player> Controller
        {
            get { return m_controller; }
        }

        public object Context
        {
            get { return m_context; }
        }

        #endregion
    }
}
