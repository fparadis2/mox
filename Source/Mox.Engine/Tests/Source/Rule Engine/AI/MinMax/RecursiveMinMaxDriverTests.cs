//// Copyright (c) François Paradis
//// This file is part of Mox, a card game simulator.
//// 
//// Mox is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, version 3 of the License.
//// 
//// Mox is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
//using NUnit.Framework;

//namespace Mox.AI
//{
//    [TestFixture]
//    public class RecursiveMinMaxDriverTests : MinMaxDriverTestsBase
//    {
//        #region Overrides

//        protected override ChoiceExpectationBehavior Behavior
//        {
//            get { return ChoiceExpectationBehavior.Delayed; }
//        }

//        protected override ChoiceExpectationBehavior RootBehavior
//        {
//            get { return ChoiceExpectationBehavior.Delayed; }
//        }

//        protected override MinMaxDriver<IMockController> CreateMinMaxDriverImpl(AIEvaluationContext context, params object[] firstChoices)
//        {
//            return firstChoices.Length == 0 ?
//                RecursiveMinMaxDriver<IMockController>.CreateController(context) :
//                RecursiveMinMaxDriver<IMockController>.CreateRootController(context, firstChoices);
//        }

//        #endregion
//    }
//}