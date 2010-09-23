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
using NUnit.Framework;

namespace Mox.AI
{
    [TestFixture]
    public class RecursiveMinMaxDriverTests : MinMaxDriverTestsBase
    {
        #region Overrides

        protected override ChoiceExpectationBehavior Behavior
        {
            get { return ChoiceExpectationBehavior.Delayed; }
        }

        protected override ChoiceExpectationBehavior RootBehavior
        {
            get { return ChoiceExpectationBehavior.Delayed; }
        }

        protected override MinMaxDriver<IMockController> CreateMinMaxDriver(IMinimaxTree tree, IMinMaxAlgorithm algorithm, IChoiceResolverProvider choiceResolverProvider, params object[] firstChoices)
        {
            return firstChoices.Length == 0 ?
                                                RecursiveMinMaxDriver<IMockController>.CreateController(m_game, tree, algorithm, choiceResolverProvider) :
                                                                                                                                                             RecursiveMinMaxDriver<IMockController>.CreateRootController(m_game, tree, algorithm, choiceResolverProvider, firstChoices);
        }

        //protected override IDisposable Expect_Choice_Impl<TChoice>()
        //{
        //    return new DisposableHelper(Try_Choice_Anything<TChoice>);
        //}

        //protected override IDisposable Expect_Root_Choice_Impl<TChoice>()
        //{
        //    return Expect_Choice_Impl<TChoice>();
        //}

        //protected override IDisposable Expect_Garbage<TChoice>()
        //{
        //    return new DisposableHelper(() =>
        //    {
        //        Expect_GetDefaultChoice(default(TChoice));
        //        Try_Choice_Anything<TChoice>();
        //    });
        //}

        #endregion
    }
}