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
using System.Collections.Generic;

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class PayCostsTests : PartTestBase
    {
        #region Inner Types

        private class PayCostsProxy : NewPart
        {
            private readonly PayCosts m_payCosts;

            public PayCostsProxy(PayCosts payCosts)
            {
                Throw.IfNull(payCosts, "payCosts");
                m_payCosts = payCosts;
            }

            public override NewPart Execute(Context context)
            {
                // MUST begin a transaction for PayCosts to work properly
                context.Schedule(new BeginTransactionPart(PayCosts.TransactionToken));
                return m_payCosts;
            }
        }

        private class MockPayCosts : PayCosts
        {
            private readonly List<ImmediateCost> m_immediateCosts = new List<ImmediateCost>();
            private readonly List<DelayedCost> m_delayedCosts = new List<DelayedCost>();

            public MockPayCosts(Player player, IEnumerable<ImmediateCost> immediateCosts, IEnumerable<DelayedCost> delayedCosts)
                : base(player)
            {
                m_immediateCosts.AddRange(immediateCosts);
                m_delayedCosts.AddRange(delayedCosts);
            }

            protected override IEnumerable<ImmediateCost> GetCosts(Context context, out IList<DelayedCost> delayedCosts, out NewPart nextPart)
            {
                delayedCosts = m_delayedCosts;
                nextPart = null;
                return m_immediateCosts;
            }
        }

        #endregion

        #region Variables

        private NewPart m_part;

        private ImmediateCost m_immediateCost1;
        private ImmediateCost m_immediateCost2;

        private DelayedCost m_delayedCost1;
        private DelayedCost m_delayedCost2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_immediateCost1 = m_mockery.StrictMock<ImmediateCost>();
            m_immediateCost2 = m_mockery.StrictMock<ImmediateCost>();

            m_delayedCost1 = m_mockery.StrictMock<DelayedCost>();
            m_delayedCost2 = m_mockery.StrictMock<DelayedCost>();

            m_part = CreatePart(m_playerA, new[] { m_immediateCost1, m_immediateCost2 }, new[] { m_delayedCost1, m_delayedCost2 });
        }

        #endregion

        #region Utilities

        private void Run(bool expectedResult)
        {
            m_sequencerTester.Run(m_part);
            Assert.AreEqual(expectedResult, m_sequencerTester.Sequencer.PopArgument<bool>(PayCosts.ArgumentToken));
        }

        private static NewPart CreatePart(Player player, IEnumerable<ImmediateCost> immediateCosts, IEnumerable<DelayedCost> delayedCosts)
        {
            return new PayCostsProxy(new MockPayCosts(player, immediateCosts, delayedCosts));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Each_cost_of_the_spell_is_paid_before_the_spell_is_pushed_on_the_stack()
        {
            m_part = CreatePart(m_playerA, new[] { m_immediateCost1, m_immediateCost2 }, new DelayedCost[0]);

            using (OrderedExpectations)
            {
                Expect.Call(m_immediateCost1.Execute(null, null)).IgnoreArguments().Return(true);
                Expect.Call(m_immediateCost2.Execute(null, null)).IgnoreArguments().Return(true);
            }

            Run(true);
        }

        [Test]
        public void Test_Each_delayed_cost_of_the_spell_is_paid_before_the_spell_is_pushed_on_the_stack()
        {
            m_part = CreatePart(m_playerA, new ImmediateCost[0], new[] { m_delayedCost1, m_delayedCost2 });

            using (OrderedExpectations)
            {
                m_delayedCost1.Execute(null, null);
                LastCall.IgnoreArguments().Callback<MTGPart.Context, Player>((context, player) =>
                {
                    Assert.AreEqual(m_playerA, player);
                    context.PushArgument(true, DelayedCost.ArgumentToken);
                    return true;
                });

                m_delayedCost2.Execute(null, null);
                LastCall.IgnoreArguments().Callback<MTGPart.Context, Player>((context, player) =>
                {
                    Assert.AreEqual(m_playerA, player);
                    context.PushArgument(true, DelayedCost.ArgumentToken);
                    return true;
                });
            }

            Run(true);
        }

        [Test]
        public void Test_If_an_immediate_cost_returns_false_when_executing_the_whole_ability_is_undone()
        {
            m_part = CreatePart(m_playerA, new[] { m_immediateCost1 }, new DelayedCost[0]);

            Expect.Call(m_immediateCost1.Execute(null, null)).IgnoreArguments().Return(false).Callback<Part<IGameController>.Context, Player>((context, player) =>
            {
                m_playerA.Life = 42;
                Assert.AreEqual(42, m_playerA.Life);
                return true;
            });

            Run(false);

            Assert.AreNotEqual(42, m_playerA.Life);
        }

        [Test]
        public void Test_If_a_delayed_cost_returns_false_when_executing_the_whole_ability_is_undone()
        {
            m_part = CreatePart(m_playerA, new ImmediateCost[0], new [] { m_delayedCost1 });

            using (OrderedExpectations)
            {
                m_delayedCost1.Execute(null, null);

                LastCall.IgnoreArguments().Callback<MTGPart.Context, Player>((context, player) =>
                {
                    context.PushArgument(false, DelayedCost.ArgumentToken);

                    m_playerA.Life = 42;
                    Assert.AreEqual(42, m_playerA.Life);
                    return true;
                });
            }

            Run(false);

            Assert.AreNotEqual(42, m_playerA.Life);
        }

        #endregion
    }
}
