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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Mox.Abilities;
using Mox.Flow;

namespace Mox.AI.ChoiceEnumerators
{
    internal class GivePriorityChoiceEnumerator : ChoiceEnumerator
    {
        #region Variables

        protected readonly List<object> m_results = new List<object>();

        #endregion

        #region Overrides of ChoiceEnumerator

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        public override IEnumerable<object> EnumerateChoices(Game game, Choice choice)
        {
            m_results.Clear();

            Player player = choice.Player.Resolve(game);

            if (ShouldConsiderPlayingAbilities(player))
            {
                var context = new AbilityEvaluationContext(player, AbilityEvaluationContextType.Normal);
                var enumerator = new AbilityEnumerator(context);
                enumerator.EnumerateAbilities(m_results);
            }

            m_results.Add(null);

            return m_results;
        }

        private bool ShouldConsiderPlayingAbilities(Player player)
        {
            var spellStack = player.Manager.SpellStack2;

            if (spellStack.IsEmpty)
                return true;

            Spell2 topSpell = spellStack.Peek();
            return topSpell.Controller != player;
        }

        #region Helper methods

        private static bool ContainsLessThan(IEnumerable collection, int count)
        {
            int i = 0;
            foreach (object o in collection)
            {
                if (++i >= count)
                {
                    return false;
                }
            }

            return i < count;
        }

        #endregion

        #endregion

        #region Inner Types

        protected class AbilityEnumerator
        {
            #region Variables
            
            private readonly AbilityEvaluationContext m_context;

            #endregion

            #region Constructor

            public AbilityEnumerator(AbilityEvaluationContext context)
            {
                m_context = context;
            }

            #endregion

            #region Methods

            public void EnumerateAbilities(List<object> playAbilityChoices)
            {
                HashContext context = new HashContext(m_context.Player.Manager);
                HashSet<int> triedAbilities = new HashSet<int>();

                foreach (var zone in GetPlayableZones())
                {
                    foreach (var card in zone)
                    {
                        foreach (SpellAbility ability in card.Abilities.OfType<SpellAbility>())
                        {
                            if (!triedAbilities.Add(ability.ComputeHash(context)))
                                continue;

                            if (CanPlay(ability))
                            {
                                playAbilityChoices.Add(new PlayAbility(ability));
                            }
                        }
                    }
                }
            }

            private IEnumerable<IReadOnlyCollection<Card>> GetPlayableZones()
            {
                yield return m_context.Player.Hand;
                yield return m_context.Player.Battlefield;
                yield return m_context.Player.Exile;
                yield return m_context.Player.Graveyard;
                yield return m_context.Player.PhasedOut;
            }

            protected virtual bool CanPlay(Ability ability)
            {
                // Don't play mana abilities unless we are in mana payment
                if (ability.IsManaAbility && m_context.Type != AbilityEvaluationContextType.ManaPayment)
                {
                    return false;
                }

                return ability.CanPlay(m_context);
            }

            #endregion
        }

        #endregion
    }
}
