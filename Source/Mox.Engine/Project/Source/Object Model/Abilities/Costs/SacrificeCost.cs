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

namespace Mox
{
    /// <summary>
    /// A cost that requires the controller to sacrifice a permanent.
    /// </summary>
    public class TargetSacrificeCost : TargetCost<Card>
    {
        #region Constructor

        public TargetSacrificeCost(Predicate<ITargetable> filter)
            : base(filter)
        {
        }

        #endregion

        #region Methods

        public override bool Execute(Flow.Part<Flow.IGameController>.Context context, Player activePlayer)
        {
            bool result = base.Execute(context, activePlayer);

            if (result)
            {
                Card card = (Card) Result.Resolve(context.Game);
                card.Sacrifice();
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// A cost that requires the controller to sacrifice a specific permanent.
    /// </summary>
    public class SacrificeCost : ImmediateCost
    {
        #region Variables

        private readonly Card m_card;

        #endregion

        #region Constructor

        public SacrificeCost(Card card)
        {
            Throw.IfNull(card, "card");
            m_card = card;
        }

        #endregion

        #region Methods

        public override bool CanExecute(Game game, ExecutionEvaluationContext evaluationContext)
        {
            return m_card.Zone.ZoneId == Zone.Id.Battlefield;
        }

        public override bool Execute(Flow.Part<Flow.IGameController>.Context context, Player activePlayer)
        {
            m_card.Sacrifice();
            return true;
        }

        #endregion
    }
}
