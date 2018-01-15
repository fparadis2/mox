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

using Mox.Abilities;
using Mox.Flow.Parts;

namespace Mox.Flow.Phases
{
    internal abstract class PayAttackingCosts : PayCosts
    {
        #region Variables

        private readonly Resolvable<Player> m_player;

        #endregion

        #region Constructor

        protected PayAttackingCosts(Resolvable<Player> player)
        {
            m_player = player;
        }

        #endregion

        #region Properties

        protected abstract AbilityType Type
        {
            get;
        }

        protected abstract AbilityEvaluationContextType EvaluationType
        {
            get;
        }

        #endregion

        #region Methods

        protected abstract Part CreateNextPart(Context context);

        protected abstract IEnumerable<Card> GetInvolvedCards(Context context);

        protected override sealed Part GetCosts(Context context, PayCostsContext costContext)
        {
            Player player = m_player.Resolve(context.Game);
            GetCosts(context, costContext, player);
            return CreateNextPart(context);
        }

        private void GetCosts(Context context, PayCostsContext costContext, Player player)
        {
            AbilityEvaluationContext evaluationContext = new AbilityEvaluationContext(player, EvaluationType);

            foreach (SpellAbility2 ability in GetAbilities(context))
            {
                SpellContext spellContext = new SpellContext(ability, player);

                if (!ability.CanPlay(evaluationContext))
                {
                    costContext.AddCost(Cost.CannotPlay, spellContext);
                    return;
                }

                foreach (var cost in ability.SpellDefinition.Costs)
                {
                    costContext.AddCost(cost, spellContext);
                }
            }
        }

        private IEnumerable<SpellAbility2> GetAbilities(Context context)
        {
            foreach (Card involvedCreature in GetInvolvedCards(context))
            {
                var abilitiesOnThisCreature = involvedCreature.Abilities.OfType<SpellAbility2>().Where(ability => ability.AbilityType == Type);

                foreach (var ability in abilitiesOnThisCreature)
                {
                    yield return ability;
                }
            }
        }

        protected Player GetPlayer(Context context)
        {
            return m_player.Resolve(context.Game);
        }

        #endregion
    }
}
