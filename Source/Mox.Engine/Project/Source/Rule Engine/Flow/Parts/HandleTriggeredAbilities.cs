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

namespace Mox.Flow.Parts
{
    /// <summary>
    /// A part that handles triggered abilities and puts them on the stack.
    /// </summary>
    public class HandleTriggeredAbilities : PlayerPart
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public HandleTriggeredAbilities(Player player)
            : base(player)
        {
        }

        #endregion

        #region Overrides of Part

        public override Part Execute(Context context)
        {
            if (context.Game.GlobalData.TriggeredAbilities.Count > 0)
            {
                // TODO: Let controller choose trigger order

                List<QueuedTriggeredAbility> queuedTriggeredAbilities = new List<QueuedTriggeredAbility>(context.Game.GlobalData.TriggeredAbilities);

                // Sort triggered abilities by controller
                queuedTriggeredAbilities.Sort(new TriggeredAbilityComparer(context));

                // Empty list right away because more triggered abilities can be triggered.
                context.Game.GlobalData.ClearTriggeredAbilities();

                context.Schedule(new HandleTriggeredAbility(queuedTriggeredAbilities));

                return new CheckStateBasedActions();
            }

            return null;
        }

        #endregion

        #region Inner Types

        private class HandleTriggeredAbility : Part
        {
            #region Variables

            private readonly IList<QueuedTriggeredAbility> m_abilities;
            private readonly int m_currentIndex;

            #endregion

            #region Constructor

            public HandleTriggeredAbility(IList<QueuedTriggeredAbility> abilities)
                : this(abilities, 0)
            {
            }

            private HandleTriggeredAbility(IList<QueuedTriggeredAbility> abilities, int currentIndex)
            {
                Debug.Assert(abilities != null);
                Debug.Assert(abilities.Count > 0);
                Debug.Assert(currentIndex >= 0 && currentIndex < abilities.Count);

                m_abilities = abilities;
                m_currentIndex = currentIndex;
            }

            #endregion

            #region Methods

            public override Part Execute(Context context)
            {
                Debug.Assert(context.Game.State.ActivePlayer != null);

                QueuedTriggeredAbility queuedAbility = m_abilities[m_currentIndex];

                TriggeredAbility2 ability = queuedAbility.Ability.Resolve(context.Game);
                Player controller = queuedAbility.Controller.Resolve(context.Game);

#warning todo spell_v2
                //if (ability.CanPushOnStack(context.Game, queuedAbility.Context))
                {
                    context.Schedule(new PlayAbility(controller, ability, queuedAbility.Context));
                }

                return m_currentIndex == m_abilities.Count - 1 ? null : new HandleTriggeredAbility(m_abilities, m_currentIndex + 1);
            }

            #endregion
        }

        private class TriggeredAbilityComparer : Comparer<QueuedTriggeredAbility>
        {
            private readonly Context m_context;

            public TriggeredAbilityComparer(Context context)
            {
                m_context = context;
            }

            public override int Compare(QueuedTriggeredAbility x, QueuedTriggeredAbility y)
            {
                Debug.Assert(m_context.Game.Players.Count == 2);

                Player controllerX = x.Controller.Resolve(m_context.Game);
                Player controllerY = y.Controller.Resolve(m_context.Game);

                if (controllerX == controllerY)
                {
                    return 0;
                }

                return controllerX == m_context.Game.State.ActivePlayer ? -1 : +1;
            }
        }

        #endregion
    }
}
