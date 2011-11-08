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

namespace Mox.Flow.Parts
{
    /// <summary>
    /// Part used when a player has to pay some costs.
    /// </summary>
    public abstract class PayCosts : PlayerPart
    {
        #region Argument Token

        public static readonly object ArgumentToken = "PayCosts_Argument";
        public static readonly object TransactionToken = "PayCosts_Transaction";

        #endregion

        #region Inner Types

        private class PlayDelayedCost : PlayerPart
        {
            #region Variables

            private readonly IList<Cost> m_costs;
            private readonly int m_currentIndex;

            private readonly bool m_checkLastCost;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public PlayDelayedCost(Player player, IList<Cost> costs)
                : this(player, costs, 0, false)
            {
            }

            private PlayDelayedCost(Player player, IList<Cost> costs, int currentIndex, bool checkLastCost)
                : base(player)
            {
                Throw.IfNull(costs, "costs");

                m_costs = costs;
                m_currentIndex = currentIndex;
                m_checkLastCost = checkLastCost;
            }

            #endregion

            #region Overrides of Part

            public override NewPart Execute(Context context)
            {
                if (m_checkLastCost && !context.PopArgument<bool>(Cost.ArgumentToken))
                {
                    context.PushArgument(false, ArgumentToken);
                    return new RollbackTransactionPart(TransactionToken);
                }

                if (m_currentIndex < m_costs.Count)
                {
                    Player player = GetPlayer(context);

                    m_costs[m_currentIndex].Execute(context, player);
                    return new PlayDelayedCost(player, m_costs, m_currentIndex + 1, true);
                }

                // Done
                context.PushArgument(true, ArgumentToken);
                return new EndTransactionPart(TransactionToken);
            }

            #endregion
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="player">Player that plays the ability.</param>
        protected PayCosts(Player player)
            : base(player)
        {
        }

        #endregion

        #region Methods

        public override NewPart Execute(Context context)
        {
            NewPart nextPart;
            IList<Cost> costs = GetCosts(context, out nextPart);

            Player player = GetPlayer(context);
            context.Schedule(new PlayDelayedCost(player, costs));
            return nextPart;
        }

        protected abstract IList<Cost> GetCosts(Context context, out NewPart nextPart);

        #endregion
    }
}
