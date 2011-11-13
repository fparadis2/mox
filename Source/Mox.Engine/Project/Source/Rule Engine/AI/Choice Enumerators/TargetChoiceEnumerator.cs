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

using Mox.Flow;

namespace Mox.AI.ChoiceEnumerators
{
    internal class TargetChoiceEnumerator : ChoiceEnumerator
    {
        #region Overrides of ChoiceEnumerator

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        public override IEnumerable<object> EnumerateChoices(Game game, Choice choice)
        {
            TargetContext targetContext = ((TargetChoice)choice).Context;

            foreach (var result in new TargetEnumerator(game, targetContext.Targets).Enumerate())
            {
                yield return result;
            }
        }

        #endregion

        #region Inner Types

        private class TargetEnumerator
        {
            private readonly Game m_game;
            private readonly IEnumerable<int> m_targets;
            private readonly LinkedList<Card> m_triedCards = new LinkedList<Card>();

            public TargetEnumerator(Game game, IEnumerable<int> targets)
            {
                m_game = game;
                m_targets = targets;
            }

            public IEnumerable<TargetResult> Enumerate()
            {
                m_triedCards.Clear();

                foreach (var target in m_targets)
                {
                    IObject obj = m_game.GetObjectByIdentifier<IObject>(target);
                    Card card = obj as Card;
                    if (card != null)
                    {
                        if (!m_triedCards.Any(c => c.IsEquivalentTo(card)))
                        {
                            m_triedCards.AddLast(card);
                            yield return new TargetResult(target);
                        }
                    }
                    else
                    {
                        yield return new TargetResult(target);
                    }
                }
            }
        }

        #endregion
    }
}
