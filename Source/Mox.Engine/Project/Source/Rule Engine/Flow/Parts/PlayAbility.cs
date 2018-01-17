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
using System.Diagnostics;
using System.Linq;

using Mox.Abilities;

namespace Mox.Flow.Parts
{
    /// <summary>
    /// Part used when a player plays an ability.
    /// </summary>
    public class PlayAbility : PlayerPart
    {
        #region Inner Types

        private class PaySpellCosts : PayCosts
        {
            #region Variables

            private readonly SpellContext m_spellContext;

            #endregion

            #region Constructor

            public PaySpellCosts(SpellContext spellContext)
            {
                m_spellContext = spellContext;
            }

            #endregion

            #region Methods

            protected override Part GetCosts(Context context, PayCostsContext costContext)
            {
                var ability = m_spellContext.Ability.Resolve(context.Game);
                foreach (var cost in ability.SpellDefinition.Costs)
                {
                    costContext.AddCost(cost, m_spellContext);
                }

                return new EndSpellPlay(m_spellContext);
            }

            #endregion
        }

        private class EndSpellPlay : Part
        {
            #region Variables

            private readonly SpellContext m_spellContext;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public EndSpellPlay(SpellContext spellContext)
            {
                m_spellContext = spellContext;
            }

            #endregion

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                bool result = context.PopArgument<bool>(PayCosts.ArgumentToken);

                if (result)
                {
                    var ability = m_spellContext.Ability.Resolve(context.Game);
                    ability.Push(context, m_spellContext.Controller.Resolve(context.Game));
                }

                return null;
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Resolvable<SpellAbility> m_ability;
        private readonly object m_abilityContext;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="player">Player that plays the ability.</param>
        /// <param name="ability">Ability to play.</param>
        /// <param name="context">Context of the ability, if any.</param>
        public PlayAbility(Player player, Resolvable<SpellAbility> ability, object context)
            : base(player)
        {
            Throw.InvalidArgumentIf(ability.IsEmpty, "Empty ability", "ability");

            m_ability = ability;
            m_abilityContext = context;
        }

        #endregion

        #region Overrides of Part

        public override Part Execute(Context context)
        {
#warning todo spell_v2 use ability context

            context.Schedule(new BeginTransactionPart(PayCosts.TransactionToken));

            var spellContext = new SpellContext(m_ability, ResolvablePlayer);
            return new PaySpellCosts(spellContext);
        }

        #endregion
    }
}
