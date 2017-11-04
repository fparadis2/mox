﻿// Copyright (c) François Paradis
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
        #region Overrides of ChoiceEnumerator

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        public override IEnumerable<object> EnumerateChoices(Game game, Choice choice)
        {
            m_results.Clear();

            Player player = choice.Player.Resolve(game);
            ManaCost manaCost = ((PayManaChoice)choice).ManaCost;

            if (manaCost.IsEmpty)
                return m_results;

            ManaPaymentEvaluator evaluator = new ManaPaymentEvaluator(manaCost);

            if (evaluator.EnumerateCompletePayments(player.ManaPool))
            {
                foreach (var payment in evaluator.CompletePayments)
                {
                    m_results.Add(new PayManaAction(payment));
                }
            }
            else // Don't return other choices if we can make a complete payment
            {
                var enumerator = CreateAbilityEnumerator(manaCost, player);
                enumerator.EnumerateAbilities(m_results);
            }

            return m_results;
        }

        private AbilityEnumerator CreateAbilityEnumerator(ManaCost cost, Player player)
        {
            var context = new ExecutionEvaluationContext(player, EvaluationContextType.ManaPayment);

            if (cost.IsEmpty)
            {
                return new AbilityEnumerator(context);
            }

            ManaPayment remainingPayment = ManaPayment.GetMaximalRemainingPayment(cost, player.ManaPool);
            Debug.Assert(remainingPayment.Payments.Any(), "Inconsistency - Not supposed to be able to complete payment");
            return new ManaAbilityEnumerator(context, remainingPayment);
        }

        #endregion

        #region Inner Types

        private class ManaAbilityEnumerator : AbilityEnumerator
        {
            #region Variables

            private readonly ManaPayment m_remainingPayment;

            #endregion

            #region Constructor

            public ManaAbilityEnumerator(ExecutionEvaluationContext context, ManaPayment remainingPayment)
                : base(context)
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
