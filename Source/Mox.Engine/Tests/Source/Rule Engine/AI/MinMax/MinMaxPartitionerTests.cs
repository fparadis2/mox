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
using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.AI
{
    [TestFixture]
    public class MinMaxPartitionerTests : BaseGameTests
    {
        #region Variables

        private IDispatchStrategy m_dispatchStrategy;
        private IEvaluationStrategy m_evaluationStrategy;
        private MinMaxPartitioner m_partitioner;
        private ICancellable m_mockCancellable;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_dispatchStrategy = m_mockery.StrictMock<IDispatchStrategy>();
            m_evaluationStrategy = m_mockery.StrictMock<IEvaluationStrategy>();
            m_mockCancellable = m_mockery.StrictMock<ICancellable>();
            m_partitioner = new MinMaxPartitioner(m_dispatchStrategy, m_evaluationStrategy);
        }

        #endregion

        #region Utilities

        private AIResult Execute(params object[] choices)
        {
            AIResult result = null;
            m_mockery.Test(() => result = m_partitioner.Execute(choices, choices.First(), m_mockCancellable));
            return result;
        }

        private void Expect_Dispatch(object choice, float result)
        {
            m_dispatchStrategy.Dispatch(null);
            LastCall.Callback<IWorkOrder>(wo =>
            {
                Assert.AreEqual(m_evaluationStrategy, wo.EvaluationStrategy);
                Assert.AreEqual(choice, wo.Choice);

                wo.Tree.BeginNode(true, choice);
                wo.Tree.Evaluate(result);
                wo.Tree.EndNode();
                return true;
            });
        }

        private void Expect_Wait()
        {
            m_dispatchStrategy.Wait();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new MinMaxPartitioner(null, m_evaluationStrategy));
            Assert.Throws<ArgumentNullException>(() => new MinMaxPartitioner(m_dispatchStrategy, null));
        }

        [Test]
        public void Test_Cannot_Execute_when_there_is_no_choices()
        {
            Assert.Throws<ArgumentException>(() => m_partitioner.Execute(new object[0], new object(), m_mockCancellable));
        }

        [Test]
        public void Test_Execute_returns_immediatly_if_theres_only_one_choice_to_try()
        {
            Expect.Call(m_evaluationStrategy.DriverType).Return(AIParameters.MinMaxDriverType.Iterative);

            object choice = new object();
            AIResult result = Execute(choice);

            Assert.AreEqual(choice, result.Result);
            Assert.AreEqual(0, result.NumEvaluations);
            Assert.AreEqual(AIParameters.MinMaxDriverType.Iterative, result.DriverType);
        }

        [Test]
        public void Test_Execute_dispatches_each_choice_waits_and_then_aggregates_the_results()
        {
            object choice1 = new object();
            object choice2 = new object();
            object choice3 = new object();

            Expect_Dispatch(choice1, -10f);
            Expect_Dispatch(choice2, 10f);
            Expect_Dispatch(choice3, 5f);

            Expect_Wait();

            Expect.Call(m_evaluationStrategy.DriverType).Return(AIParameters.MinMaxDriverType.Recursive);

            AIResult result = Execute(choice1, choice2, choice3);

            Assert.AreEqual(choice2, result.Result);
            Assert.AreEqual(10, result.PredictedScore);
            Assert.AreEqual(3, result.NumEvaluations);
            Assert.AreEqual(AIParameters.MinMaxDriverType.Recursive, result.DriverType);
        }

        [Test]
        public void Test_Execute_will_ignore_dispatches_that_did_not_work()
        {
            object choice1 = new object();
            object choice2 = new object();

            Expect_Dispatch(choice1, -10f);
            m_dispatchStrategy.Dispatch(null); LastCall.IgnoreArguments();

            Expect_Wait();

            Expect.Call(m_evaluationStrategy.DriverType).Return(AIParameters.MinMaxDriverType.Recursive);

            AIResult result = Execute(choice1, choice2);

            Assert.AreEqual(choice1, result.Result);
            Assert.AreEqual(-10f, result.PredictedScore);
            Assert.AreEqual(1, result.NumEvaluations);
            Assert.AreEqual(AIParameters.MinMaxDriverType.Recursive, result.DriverType);
        }

        [Test]
        public void Test_Execute_will_return_the_first_choice_if_no_dispatches_were_made_correctly()
        {
            object choice1 = new object();
            object choice2 = new object();

            m_dispatchStrategy.Dispatch(null); LastCall.IgnoreArguments();
            m_dispatchStrategy.Dispatch(null); LastCall.IgnoreArguments();

            Expect_Wait();

            Expect.Call(m_evaluationStrategy.DriverType).Return(AIParameters.MinMaxDriverType.Recursive);

            AIResult result = Execute(choice1, choice2);

            Assert.AreEqual(choice1, result.Result);
            Assert.AreEqual(0, result.NumEvaluations);
            Assert.AreEqual(AIParameters.MinMaxDriverType.Recursive, result.DriverType);
        }

        #endregion
    }
}
