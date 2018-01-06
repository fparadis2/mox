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
using Mox.Flow;

namespace Mox.Abilities
{
    /// <summary>
    /// A cost that requires the controller to sacrifice a permanent.
    /// </summary>
    public class TargetSacrificeCost : TargetCost<Card>
    {
        #region Constructor

        public TargetSacrificeCost(Predicate<GameObject> filter)
            : base(filter)
        {
        }

        #endregion

        #region Methods

        public override void Execute(Part.Context context, Player activePlayer)
        {
            base.Execute(context, activePlayer);
            context.Schedule(new SacrificePart(this));
        }

        #endregion

        #region Inner Parts

        private class SacrificePart : Part
        {
            #region Variables

            private readonly TargetCost m_target;

            #endregion

            #region Constructor

            public SacrificePart(TargetCost target)
            {
                m_target = target;
            }

            #endregion

            #region Methods

            public override Part Execute(Context context)
            {
                var result = PopResult(context);
                if (result)
                {
                    Card card = (Card)m_target.Resolve(context.Game);
                    card.Sacrifice();
                }
                PushResult(context, result);
                return null;
            }

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// A cost that requires the controller to sacrifice a specific permanent.
    /// </summary>
    public class SacrificeCost : Cost
    {
        #region Variables

        private readonly Resolvable<Card> m_card;

        #endregion

        #region Constructor

        public SacrificeCost(Card card)
        {
            Throw.IfNull(card, "card");
            m_card = card;
        }

        #endregion

        #region Methods

        public override bool CanExecute(Game game, AbilityEvaluationContext evaluationContext)
        {
            var card = m_card.Resolve(game);
            return CanExecuteImpl(card);
        }

        public override void Execute(Part.Context context, Player activePlayer)
        {
            var card = m_card.Resolve(context.Game);

            if (!CanExecuteImpl(card))
            {
                PushResult(context, false);
                return;
            }

            card.Sacrifice();
            PushResult(context, true);
        }

        private static bool CanExecuteImpl(Card card)
        {
            return card.Zone.ZoneId == Zone.Id.Battlefield;
        }

        #endregion
    }
}
