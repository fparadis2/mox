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
using System.Linq;

using Mox.Flow.Parts;

namespace Mox.Flow.Phases
{
    internal abstract class PayAttackingCosts : PayCosts
    {
        #region Constructor

        protected PayAttackingCosts(Player player)
            : base(player)
        {
        }

        #endregion

        #region Properties

        protected abstract AbilityType Type
        {
            get;
        }

        protected abstract EvaluationContextType EvaluationType
        {
            get;
        }

        #endregion

        #region Methods

        protected abstract NewPart CreateNextPart(Context context);

        protected abstract IEnumerable<Card> GetInvolvedCards(Context context);

        protected override sealed IList<Cost> GetCosts(Context context, out NewPart nextPart)
        {
            Player player = GetPlayer(context);

            nextPart = CreateNextPart(context);

            List<Cost> costs = new List<Cost>();
            GetCosts(context, player, costs);
            return costs;
        }

        private void GetCosts(Context context, Player player, IList<Cost> costs)
        {
            ExecutionEvaluationContext evaluationContext = new ExecutionEvaluationContext
            {
                Type = EvaluationType
            };

            foreach (Ability ability in GetAbilities(context))
            {
                if (!ability.CanPlay(player, evaluationContext))
                {
                    costs.Add(Cost.CannotPlay);
                    return;
                }

                Spell spell = new Spell(context.Game, ability, player, null);
                ability.Play(spell);
                spell.Costs.ForEach(costs.Add);
            }
        }

        private IEnumerable<Ability> GetAbilities(Context context)
        {
            foreach (Card involvedCreature in GetInvolvedCards(context))
            {
                var abilitiesOnThisCreature = involvedCreature.Abilities.Where(ability => ability.AbilityType == Type);

                foreach (var ability in abilitiesOnThisCreature)
                {
                    yield return ability;
                }
            }
        }

        #endregion
    }
}
