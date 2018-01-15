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

        protected class PayCostsContext
        {
            private readonly List<Cost> m_costs = new List<Cost>();
            private readonly List<SpellContext> m_spellContexts = new List<SpellContext>();

            public int Count => m_costs.Count;
            public IReadOnlyList<Cost> Costs => m_costs;
            public IReadOnlyList<SpellContext> SpellContexts => m_spellContexts;

            public void AddCost(Cost cost, SpellContext spellContext)
            {
                m_costs.Add(cost);
                m_spellContexts.Add(spellContext);
            }

            internal void Execute(int index, Context context)
            {
                m_costs[index].Execute(context, m_spellContexts[index]);
            }
        }

        private class PlayDelayedCost : Part
        {
            #region Variables

            private readonly PayCostsContext m_context;
            private readonly int m_currentIndex;

            private readonly bool m_checkLastCost;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public PlayDelayedCost(PayCostsContext context)
                : this(context, 0, false)
            {
            }

            private PlayDelayedCost(PayCostsContext context, int currentIndex, bool checkLastCost)
            {
                Throw.IfNull(context, "context");

                m_context = context;
                m_currentIndex = currentIndex;
                m_checkLastCost = checkLastCost;
            }

            #endregion

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                if (m_checkLastCost && !Cost.PopResult(context))
                {
                    context.PushArgument(false, ArgumentToken);
                    return new EndTransactionPart(true, TransactionToken);
                }

                if (m_currentIndex < m_context.Count)
                {
                    m_context.Execute(m_currentIndex, context);
                    return new PlayDelayedCost(m_context, m_currentIndex + 1, true);
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
            PayCostsContext costContext = new PayCostsContext();
            Part nextPart = GetCosts(context, costContext);
            context.Schedule(new PlayDelayedCost(costContext));
            return nextPart;
        }

        protected abstract Part GetCosts(Context context, PayCostsContext costContext);

        #endregion
    }
}
