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

using Mox.Abilities;
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
                var context = new AbilityEvaluationContext(player, AbilityEvaluationContextType.ManaPayment);
                var enumerator = new ManaAbilityEnumerator(context, evaluator.MissingMana);
                enumerator.EnumerateAbilities(m_results);
            }

            return m_results;
        }

        #endregion

        #region Inner Types

        private class ManaAbilityEnumerator : AbilityEnumerator
        {
            #region Nested Types

            private class Outcome : IManaAbilityOutcome
            {
                private readonly ManaColors m_missingMana;

                public Outcome(ManaColors missingMana)
                {
                    m_missingMana = missingMana;
                }

                public void Reset()
                {
                    CanProvide = false;
                }

                public bool CanProvide { get; private set; }

                public void Add(ManaAmount amount)
                {
                    if (m_missingMana.HasFlag(ManaColors.White) && amount.White > 0)
                    {
                        CanProvide = true;
                        return;
                    }

                    if (m_missingMana.HasFlag(ManaColors.Blue) && amount.Blue > 0)
                    {
                        CanProvide = true;
                        return;
                    }

                    if (m_missingMana.HasFlag(ManaColors.Black) && amount.Black > 0)
                    {
                        CanProvide = true;
                        return;
                    }

                    if (m_missingMana.HasFlag(ManaColors.Red) && amount.Red > 0)
                    {
                        CanProvide = true;
                        return;
                    }

                    if (m_missingMana.HasFlag(ManaColors.Green) && amount.Green > 0)
                    {
                        CanProvide = true;
                        return;
                    }

                    if (m_missingMana.HasFlag(ManaColors.Colorless) && amount.Colorless > 0)
                    {
                        CanProvide = true;
                        return;
                    }
                }

                public void AddAny()
                {
                    CanProvide = true;
                }
            }

            #endregion

            #region Variables

            private readonly Outcome m_outcome;

            #endregion

            #region Constructor

            public ManaAbilityEnumerator(AbilityEvaluationContext context, ManaColors missingMana)
                : base(context)
            {
                m_outcome = new Outcome(missingMana);
            }

            #endregion

            #region Methods

            protected override bool CanPlay(Ability ability)
            {
                bool canPlay = base.CanPlay(ability);
                if (canPlay)
                {
                    Debug.Assert(ability.IsManaAbility);

                    m_outcome.Reset();
                    ability.FillManaOutcome(m_outcome);
                    canPlay &= m_outcome.CanProvide;
                }

                return canPlay;
            }

            #endregion
        }

        #endregion
    }
}
