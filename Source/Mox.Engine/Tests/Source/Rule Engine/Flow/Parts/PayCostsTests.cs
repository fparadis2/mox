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
            private readonly List<Cost> m_costs = new List<Cost>();

            public MockPayCosts(Player player, IEnumerable<Cost> costs)
                : base(player)
            {
                m_costs.AddRange(costs);
            }

            protected override IList<Cost> GetCosts(Context context, out NewPart nextPart)
            {
                nextPart = null;
                return m_costs;
            }
        }

        #endregion

        #region Variables

        private NewPart m_part;

        private Cost m_cost1;
        private Cost m_cost2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_cost1 = m_mockery.StrictMock<Cost>();
            m_cost2 = m_mockery.StrictMock<Cost>();

            m_part = CreatePart(m_playerA, new[] { m_cost1, m_cost2 });
        }

        #endregion

        #region Utilities

        private void Run(bool expectedResult)
        {
            m_sequencerTester.Run(m_part);
            Assert.AreEqual(expectedResult, m_sequencerTester.Sequencer.PopArgument<bool>(PayCosts.ArgumentToken));
        }

        private static NewPart CreatePart(Player player, IEnumerable<Cost> costs)
        {
            return new PayCostsProxy(new MockPayCosts(player, costs));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Each_cost_of_the_spell_is_paid_before_the_spell_is_pushed_on_the_stack()
        {
            m_part = CreatePart(m_playerA, new[] { m_cost1, m_cost2 });

            using (OrderedExpectations)
            {
                m_cost1.Execute(null, null);
                LastCall.IgnoreArguments().Callback<NewPart.Context, Player>((context, player) =>
                {
                    Assert.AreEqual(m_playerA, player);
                    Cost.PushResult(context, true);
                    return true;
                });

                m_cost2.Execute(null, null);
                LastCall.IgnoreArguments().Callback<NewPart.Context, Player>((context, player) =>
                {
                    Assert.AreEqual(m_playerA, player);
                    Cost.PushResult(context, true);
                    return true;
                });
            }

            Run(true);
        }

        [Test]
        public void Test_If_a_cost_returns_false_when_executing_the_whole_ability_is_undone()
        {
            m_part = CreatePart(m_playerA, new [] { m_cost1 });

            using (OrderedExpectations)
            {
                m_cost1.Execute(null, null);

                LastCall.IgnoreArguments().Callback<NewPart.Context, Player>((context, player) =>
                {
                    Cost.PushResult(context, false);

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
