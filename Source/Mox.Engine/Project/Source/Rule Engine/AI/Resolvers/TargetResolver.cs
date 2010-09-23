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
using System.Reflection;
using System.Text;
using Mox.Flow;

namespace Mox.AI.Resolvers
{
    internal class TargetResolver : BaseMTGChoiceResolver
    {
        #region Overrides of BaseMTGChoiceResolver

        /// <summary>
        /// Expected method name, used for asserts...
        /// </summary>
        public override string ExpectedMethodName
        {
            get { return "Target"; }
        }

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        /// <param name="choiceMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override IEnumerable<object> ResolveChoices(MethodBase choiceMethod, object[] args)
        {
            TargetContext targetContext = GetTargetContext(args);
            var context = GetContext<IGameController>(choiceMethod, args);

            foreach (var result in new TargetEnumerator(context.Game, targetContext.Targets).Enumerate())
            {
                yield return result;
            }
        }

        /// <summary>
        /// Returns the default choice for the choice context.
        /// </summary>
        /// <remarks>
        /// The actual value is not so important, only that it returns a valid value.
        /// </remarks>
        /// <param name="choiceMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object GetDefaultChoice(MethodBase choiceMethod, object[] args)
        {
            TargetContext targetContext = GetTargetContext(args);
            Debug.Assert(targetContext.Targets.Any());
            return targetContext.Targets[0];
        }

        private static TargetContext GetTargetContext(object[] args)
        {
            return (TargetContext)args[2];
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

            public IEnumerable<int> Enumerate()
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
                            yield return target;
                        }
                    }
                    else
                    {
                        yield return target;
                    }
                }
            }
        }

        #endregion
    }
}
