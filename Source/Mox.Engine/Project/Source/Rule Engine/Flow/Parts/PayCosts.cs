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
using System.Collections.Generic;

using Mox.Abilities;

namespace Mox.Flow.Parts
{
    /// <summary>
    /// Part used when a player has to pay some costs.
    /// </summary>
    public abstract class PayCosts : Part
    {
        #region Argument Token

        public static readonly object ArgumentToken = "PayCosts_Argument";
        public static readonly object TransactionToken = "PayCosts_Transaction";

        #endregion

        #region Inner Types

        private class PlayDelayedCost : Part
        {
            #region Variables

            private readonly Resolvable<Spell2> m_spell;
            private readonly IReadOnlyList<Cost> m_costs;
            private readonly int m_currentIndex;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public PlayDelayedCost(Spell2 spell, IReadOnlyList<Cost> costs)
                : this(spell, costs, 0)
            {
            }

            private PlayDelayedCost(Resolvable<Spell2> spell, IReadOnlyList<Cost> costs, int currentIndex)
            {
                m_spell = spell;
                m_costs = costs;
                m_currentIndex = currentIndex;
            }

            #endregion

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                if (m_currentIndex > 0 && !Cost.PopResult(context))
                {
                    context.PushArgument(false, ArgumentToken);
                    return new EndTransactionPart(true, TransactionToken);
                }

                if (m_currentIndex < m_costs.Count)
                {
                    var spell = m_spell.Resolve(context.Game);
                    m_costs[m_currentIndex].Execute(context, spell);
                    return new PlayDelayedCost(m_spell, m_costs, m_currentIndex + 1);
                }

                // Done
                context.PushArgument(true, ArgumentToken);
                return new EndTransactionPart(false, TransactionToken);
            }

            #endregion
        }

        #endregion

        #region Methods

        public override Part Execute(Context context)
        {
            var spell = GetSpell(context, out Part nextPart);

            List<Cost> costs = new List<Cost>(spell.Ability.SpellDefinition.Costs);
            context.Schedule(new PlayDelayedCost(spell, costs));

            return nextPart;
        }

        protected abstract Spell2 GetSpell(Context context, out Part nextPart);

        #endregion
    }
}
