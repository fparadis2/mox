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
using NUnit.Framework;

namespace Mox.AI
{
    [TestFixture]
    public class IterativeMinMaxDriverTests : MinMaxDriverTestsBase
    {
        #region Overrides

        protected override ChoiceExpectationBehavior Behavior
        {
            get { return ChoiceExpectationBehavior.Direct; }
        }

        protected override ChoiceExpectationBehavior RootBehavior
        {
            get { return ChoiceExpectationBehavior.Direct; }
        }

        protected override MinMaxDriver<IMockController> CreateMinMaxDriver(IMinimaxTree tree, IMinMaxAlgorithm algorithm, IChoiceResolverProvider choiceResolverProvider, params object[] firstChoices)
        {
            return firstChoices.Length == 0 ?
                   IterativeMinMaxDriver<IMockController>.CreateController(m_game, tree, algorithm, choiceResolverProvider) :
                   IterativeMinMaxDriver<IMockController>.CreateRootController(m_game, tree, algorithm, choiceResolverProvider, firstChoices);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Execution_will_stop_after_timeout()
        {
            CreateMockCancellable();

            using (OrderedExpectations)
            {
                using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
                {
                    Expect_Is_Cancelled(false);

                    using (BeginNode(true, ChoiceA.ResultX))
                    {
                        Try_Choice(ChoiceA.ResultX);

                        Expect_Evaluate_heuristic();

                        Expect_Is_Cancelled(true);
                    }
                }
            }

            Execute<SingleChoicePart>();
        }

        #endregion
    }
}


