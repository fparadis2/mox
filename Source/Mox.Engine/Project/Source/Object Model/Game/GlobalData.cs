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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mox
{
    /// <summary>
    /// Contains various internal data about the game state.
    /// </summary>
    /// <remarks>
    /// Ideally, this class could be synchronized only to other master games (and not to clients)
    /// </remarks>
    public class GlobalData : GameObject
    {
        #region Triggered Abilities

        [Serializable]
        private class TriggeredAbilitiesQueue
        {
            #region Variables

            private readonly List<QueuedTriggeredAbility> m_abilities;

            #endregion

            #region Constructor

            public TriggeredAbilitiesQueue(IEnumerable<QueuedTriggeredAbility> abilities)
            {
                Debug.Assert(abilities != null);
                m_abilities = new List<QueuedTriggeredAbility>(abilities);
            }

            #endregion

            #region Properties

            public ICollection<QueuedTriggeredAbility> Abilities
            {
                get { return m_abilities.AsReadOnly(); }
            }

            #endregion
        }

        private static readonly Property<TriggeredAbilitiesQueue> m_triggeredAbilitiesQueue = Property<TriggeredAbilitiesQueue>.RegisterProperty("TriggeredAbilities", typeof(GlobalData));

        public ICollection<QueuedTriggeredAbility> TriggeredAbilities
        {
            get 
            {
                TriggeredAbilitiesQueue queue = GetValue(m_triggeredAbilitiesQueue);
                return queue != null ? queue.Abilities : new QueuedTriggeredAbility[0];
            }
        }

        internal void TriggerAbility(TriggeredAbility ability, object context)
        {
            QueuedTriggeredAbility queuedAbility = new QueuedTriggeredAbility(ability, ability.Controller, context);

            List<QueuedTriggeredAbility> list = new List<QueuedTriggeredAbility>();
            TriggeredAbilitiesQueue queue = GetValue(m_triggeredAbilitiesQueue);

            if (queue != null)
            {
                list.AddRange(queue.Abilities);
            }

            list.Add(queuedAbility);
            queue = new TriggeredAbilitiesQueue(list);
            SetValue(m_triggeredAbilitiesQueue, queue);
        }

        public void ClearTriggeredAbilities()
        {
            SetValue(m_triggeredAbilitiesQueue, null);
        }

        #endregion
    }
}
