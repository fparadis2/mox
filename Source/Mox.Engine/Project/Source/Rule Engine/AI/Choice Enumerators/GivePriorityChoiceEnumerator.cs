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

using Mox.Flow;

namespace Mox.AI.ChoiceEnumerators
{
    internal class GivePriorityChoiceEnumerator : ChoiceEnumerator
    {
        #region Variables

        private readonly ExecutionEvaluationContext m_evaluationContext;

        #endregion

        #region Constructor

        public GivePriorityChoiceEnumerator()
            : this(new ExecutionEvaluationContext())
        {
        }

        protected GivePriorityChoiceEnumerator(ExecutionEvaluationContext context)
        {
            context.UserMode = true;
            m_evaluationContext = context;
        }

        #endregion

        #region Overrides of ChoiceEnumerator

        protected ExecutionEvaluationContext Context
        {
            get { return m_evaluationContext; }
        }

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        public override IEnumerable<object> EnumerateChoices(Game game, Choice choice)
        {
            Player player = choice.Player.Resolve(game);

            if (ShouldConsiderPlayingAbilities(player))
            {
                foreach (var playAbility in EnumerateAbilities(new AbilityEnumerator(player, Context)))
                {
                    yield return playAbility;
                }
            }

            yield return null;
        }

        protected IEnumerable<PlayAbility> EnumerateAbilities(AbilityEnumerator enumerator)
        {
            foreach (Ability ability in enumerator.EnumerateAbilities())
            {
                yield return new PlayAbility(ability);
            }
        }

        private bool ShouldConsiderPlayingAbilities(Player player)
        {
            Debug.Assert(Context.Type != EvaluationContextType.ManaPayment);

            SpellStack spellStack = player.Manager.SpellStack;

            // If stack is empty, it's always ok to play
            if (spellStack.IsEmpty)
            {
                SessionData.PassUntilStackIsEmpty = false;
                return true;
            }

            // If we control the top spell on the stack, we always pass.
            Spell topSpell = spellStack.Peek();
            if (topSpell.Controller == player || SessionData.PassUntilStackIsEmpty)
            {
                return false;
            }

            // Otherwise, we play until we get to the max allowed spell stack depth
            if (ContainsLessThan(spellStack, Parameters.MaximumSpellStackDepth))
            {
                return true;
            }
            else
            {
                // Pass until the stack becomes empty again
                SessionData.PassUntilStackIsEmpty = true;
                return false;
            }
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

            private readonly Player m_player;
            private readonly ExecutionEvaluationContext m_context;

            #endregion

            #region Constructor

            public AbilityEnumerator(Player player, ExecutionEvaluationContext context)
            {
                m_player = player;
                m_context = context;
            }

            #endregion

            #region Methods

            public IEnumerable<Ability> EnumerateAbilities()
            {
                return GetPlayableZones()
                        .SelectMany(zone => zone)
                        .OrderBy(c => c.Identifier) // In order to always get the same result
                        .GroupBy(c => c.Name)
                        .SelectMany(EnumerateDistinctAbilities);
            }

            private IEnumerable<ICollection<Card>> GetPlayableZones()
            {
                yield return m_player.Hand;
                yield return m_player.Battlefield;
                yield return m_player.Exile;
                yield return m_player.Graveyard;
                yield return m_player.PhasedOut;
            }

            private IEnumerable<Ability> EnumerateDistinctAbilities(IEnumerable<Card> cards)
            {
                // LinkedList because I want insertion to be fast + we only need normal enumeration
                var triedAbilities = new LinkedList<Ability>();

                foreach (Card card in cards)
                {
                    foreach (Ability ability in card.Abilities)
                    {
                        if (CanPlay(ability) && ConsiderAbility(triedAbilities, card, ability))
                        {
                            yield return ability;
                        }
                    }
                }
            }

            private static bool ConsiderAbility(ICollection<Ability> triedAbilities, Card card, Ability ability)
            {
                if (ContainsEquivalentAbility(triedAbilities, card, ability))
                {
                    return false;
                }

                triedAbilities.Add(ability);
                return true;
            }

            private static bool ContainsEquivalentAbility(IEnumerable<Ability> triedAbilities, Card card, Ability ability)
            {
                return triedAbilities.Any(a => a.IsEquivalentTo(ability, Ability.SourceProperty) && a.Source.IsEquivalentTo(card));
            }

            protected virtual bool CanPlay(Ability ability)
            {
                // Don't play mana abilities unless we are in mana payment
                if (ability.IsManaAbility && m_context.Type != EvaluationContextType.ManaPayment)
                {
                    return false;
                }

                return ability.CanPlay(m_player, m_context);
            }

            #endregion
        }

        #endregion
    }
}
