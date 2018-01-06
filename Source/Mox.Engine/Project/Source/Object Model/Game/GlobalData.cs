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
using System.Diagnostics;

using Mox.Abilities;

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
        private class TriggeredAbilitiesQueue : IHashable
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

            #region Methods

            public void ComputeHash(Hash hash, HashContext context)
            {
                // Ignore pending triggered abilities for now.. much simpler that way
            }

            #endregion
        }

        private TriggeredAbilitiesQueue m_triggeredAbilities;
        private static readonly Property<TriggeredAbilitiesQueue> TriggeredAbilitiesQueueProperty = Property<TriggeredAbilitiesQueue>.RegisterProperty<GlobalData>("TriggeredAbilities", g => g.m_triggeredAbilities);

        public ICollection<QueuedTriggeredAbility> TriggeredAbilities
        {
            get 
            {
                return m_triggeredAbilities != null ? m_triggeredAbilities.Abilities : new QueuedTriggeredAbility[0];
            }
        }

        internal void TriggerAbility(TriggeredAbility ability, object context)
        {
            QueuedTriggeredAbility queuedAbility = new QueuedTriggeredAbility(ability, ability.Controller, context);

            List<QueuedTriggeredAbility> list = new List<QueuedTriggeredAbility>();
            TriggeredAbilitiesQueue queue = m_triggeredAbilities;

            if (queue != null)
            {
                list.AddRange(queue.Abilities);
            }

            list.Add(queuedAbility);
            queue = new TriggeredAbilitiesQueue(list);
            SetValue(TriggeredAbilitiesQueueProperty, queue, ref m_triggeredAbilities);
        }

        public void ClearTriggeredAbilities()
        {
            SetValue(TriggeredAbilitiesQueueProperty, null, ref m_triggeredAbilities);
        }

        #endregion
    }
}
