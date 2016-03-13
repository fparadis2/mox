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
        protected readonly List<object> m_results = new List<object>();

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
            m_results.Clear();

            Player player = choice.Player.Resolve(game);

            if (ShouldConsiderPlayingAbilities(player))
            {
                var enumerator = new AbilityEnumerator(player, Context);
                enumerator.EnumerateAbilities(m_results);
            }

            m_results.Add(null);

            return m_results;
        }

        private bool ShouldConsiderPlayingAbilities(Player player)
        {
            Debug.Assert(Context.Type != EvaluationContextType.ManaPayment);

            SpellStack spellStack = player.Manager.SpellStack;

            if (spellStack.IsEmpty)
                return true;

            Spell topSpell = spellStack.Peek();
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

            public void EnumerateAbilities(List<object> playAbilityChoices)
            {
                HashContext context = new HashContext(m_player.Manager);
                HashSet<int> triedAbilities = new HashSet<int>();

                foreach (var zone in GetPlayableZones())
                {
                    foreach (var card in zone)
                    {
                        foreach (Ability ability in card.Abilities)
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
                yield return m_player.Hand;
                yield return m_player.Battlefield;
                yield return m_player.Exile;
                yield return m_player.Graveyard;
                yield return m_player.PhasedOut;
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
