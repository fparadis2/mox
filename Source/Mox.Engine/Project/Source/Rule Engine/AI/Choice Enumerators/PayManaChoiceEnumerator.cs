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
using System.Linq;
using Mox.Flow;

namespace Mox.AI.ChoiceEnumerators
{
    internal class PayManaChoiceEnumerator : GivePriorityChoiceEnumerator
    {
        #region Ctor

        public PayManaChoiceEnumerator()
            : base(new ExecutionEvaluationContext { Type = EvaluationContextType.ManaPayment })
        {
        }

        #endregion

        #region Overrides of ChoiceEnumerator

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        public override IEnumerable<object> EnumerateChoices(Game game, Choice choice)
        {
            Player player = choice.Player.Resolve(game);
            ManaCost manaCost = ((PayManaChoice)choice).ManaCost;

            bool canMakeCompletePayment = false;

            if (!manaCost.IsEmpty)
            {
                foreach (ManaPayment payment in ManaPayment.EnumerateCompletePayments(manaCost, player.ManaPool))
                {
                    canMakeCompletePayment = true;
                    yield return new PayManaAction(payment);
                }
            }

            // Don't return other choices if we can make a complete payment
            if (!canMakeCompletePayment)
            {
                var enumerator = CreateAbilityEnumerator(manaCost, player);

                foreach (var ability in EnumerateAbilities(enumerator))
                {
                    yield return ability;
                }

                yield return null;
            }
        }

        private AbilityEnumerator CreateAbilityEnumerator(ManaCost cost, Player player)
        {
            if (cost.IsEmpty)
            {
                return new AbilityEnumerator(player, Context);
            }

            ManaPayment remainingPayment = ManaPayment.GetMaximalRemainingPayment(cost, player.ManaPool);
            Debug.Assert(remainingPayment.Payments.Any(), "Inconsistency - Not supposed to be able to complete payment");
            return new ManaAbilityEnumerator(player, Context, remainingPayment);
        }

        #endregion

        #region Inner Types

        private class ManaAbilityEnumerator : AbilityEnumerator
        {
            #region Variables

            private readonly ManaPayment m_remainingPayment;

            #endregion

            #region Constructor

            public ManaAbilityEnumerator(Player player, ExecutionEvaluationContext context, ManaPayment remainingPayment)
                : base(player, context)
            {
                m_remainingPayment = remainingPayment;
            }

            #endregion

            #region Methods

            protected override bool CanPlay(Ability ability)
            {
                bool canPlay = base.CanPlay(ability);
                if (canPlay)
                {
                    Debug.Assert(ability.IsManaAbility);
                    canPlay &= ability.ManaOutcome.CanProvide(m_remainingPayment);
                }

                return canPlay;
            }

            #endregion
        }

        #endregion
    }
}
