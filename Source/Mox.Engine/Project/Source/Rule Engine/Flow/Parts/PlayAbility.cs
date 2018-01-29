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

            private readonly Resolvable<Spell2> m_spell;

            #endregion

            #region Constructor

            public PaySpellCosts(Resolvable<Spell2> spell)
            {
                m_spell = spell;
            }

            #endregion

            #region Methods

            protected override Spell2 GetSpell(Context context, out Part nextPart)
            {
                nextPart = new EndSpellPlay(m_spell);
                return m_spell.Resolve(context.Game);
            }

            #endregion
        }

        private class EndSpellPlay : Part
        {
            #region Variables

            private readonly Resolvable<Spell2> m_spell;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public EndSpellPlay(Resolvable<Spell2> spell)
            {
                m_spell = spell;
            }

            #endregion

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                var spell = m_spell.Resolve(context.Game);

                bool result = context.PopArgument<bool>(PayCosts.ArgumentToken);
                if (result)
                {
                    spell.Push(context);
                }
                else
                {
                    spell.Cancel();
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

            var ability = m_ability.Resolve(context.Game);
            var player = GetPlayer(context);
            var spell = context.Game.CreateSpell(ability, player);
            return new PaySpellCosts(spell);
        }

        #endregion
    }
}
