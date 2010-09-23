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
using System.Collections.Generic;
using System.Diagnostics;

namespace Mox.Flow.Parts
{
    /// <summary>
    /// Part used when a player plays an ability.
    /// </summary>
    public class PlayAbility : MTGPart
    {
        #region Inner Types

        private class BeginSpellPlay : PayCosts
        {
            #region Variables

            private readonly Spell m_spell;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="spell"></param>
            public BeginSpellPlay(Spell spell)
                : base(spell.Controller)
            {
                Throw.IfNull(spell, "spell");
                m_spell = spell;
                Debug.Assert(spell.DelayedCosts.Count == 0, "Should be 0 because it is reset in GetCosts anyway");
            }

            #endregion

            #region Methods

            protected override IEnumerable<ImmediateCost> GetCosts(Context context, out IList<DelayedCost> delayedCosts, out MTGPart nextPart)
            {
                Spell workingSpell = m_spell.Resolve(context.Game, true);
                delayedCosts = workingSpell.DelayedCosts;
                nextPart = new EndSpellPlay(workingSpell);
                return workingSpell.Ability.Play(workingSpell);
            }

            #endregion
        }

        private class EndSpellPlay : MTGPart
        {
            #region Variables

            private readonly Spell m_spell;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public EndSpellPlay(Spell spell)
                : base(spell.Controller)
            {
                Throw.IfNull(spell, "spell");
                m_spell = spell;
            }

            #endregion

            #region Overrides of Part

            public override Part<IGameController> Execute(Context context)
            {
                bool result = context.PopArgument<bool>(BeginSpellPlay.ArgumentToken);

                if (result)
                {
                    Spell spell = m_spell.Resolve(context.Game, false);

                    if (spell.PreEffect != null)
                    {
                        spell.PreEffect(spell, context);
                    }

                    if (spell.UseStack)
                    {
                        context.Game.SpellStack.Push(spell);
                        context.Game.Events.Trigger(new Events.SpellPlayed(spell));
                    }
                    else
                    {
                        context.Schedule(new ResolveSpell(spell));
                    }
                }

                return null;
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Resolvable<Ability> m_ability;
        private readonly object m_abilityContext;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="player">Player that plays the ability.</param>
        /// <param name="ability">Ability to play.</param>
        /// <param name="context">Context of the ability, if any.</param>
        public PlayAbility(Player player, Resolvable<Ability> ability, object context)
            : base(player)
        {
            Throw.InvalidArgumentIf(ability.IsEmpty, "Empty ability", "ability");

            m_ability = ability;
            m_abilityContext = context;
        }

        #endregion

        #region Overrides of Part

        public override Part<IGameController> Execute(Context context)
        {
            Ability ability = m_ability.Resolve(context.Game);
            Spell spell = new Spell(context.Game, ability, GetPlayer(context), m_abilityContext);

            context.Schedule(new BeginTransactionPart<IGameController>(BeginSpellPlay.TransactionToken));
            return new BeginSpellPlay(spell);
        }

        #endregion
    }
}
