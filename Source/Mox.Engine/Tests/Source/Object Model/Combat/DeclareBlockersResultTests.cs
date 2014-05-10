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
using System.Linq;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class DeclareBlockersResultTests : BaseGameTests
    {
        #region Variables

        private DeclareBlockersResult m_result;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_result = new DeclareBlockersResult();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_construction_values()
        {
            Assert.Collections.IsEmpty(m_result.Blockers);
            Assert.IsTrue(m_result.Blockers.IsReadOnly);
        }

        [Test]
        public void Test_Empty_returns_an_empty_result_with_no_attackers()
        {
            Assert.Collections.IsEmpty(DeclareBlockersResult.Empty.Blockers);
        }

        [Test]
        public void Test_Can_construct_with_blockers()
        {
            var blocker = new DeclareBlockersResult.BlockingCreature(m_card, m_card);
            m_result = new DeclareBlockersResult(blocker);
            Assert.Collections.AreEqual(new[] { blocker }, m_result.Blockers);
        }

        [Test]
        public void Test_Is_serializable()
        {
            Assert.IsSerializable(m_result);
        }

        [Test]
        public void Test_Can_get_blockers()
        {
            Card card2 = CreateCard(m_playerA);

            var blocker1 = new DeclareBlockersResult.BlockingCreature(m_card, m_card);
            var blocker2 = new DeclareBlockersResult.BlockingCreature(card2, m_card);
            m_result = new DeclareBlockersResult(blocker1, blocker2);

            Assert.Collections.AreEqual(new[] { m_card, card2 }, m_result.GetBlockers(m_game));
        }

        [Test]
        public void Test_Can_get_blocker_pairs()
        {
            Card card2 = CreateCard(m_playerA);

            var blocker1 = new DeclareBlockersResult.BlockingCreature(m_card, m_card);
            var blocker2 = new DeclareBlockersResult.BlockingCreature(card2, m_card);
            m_result = new DeclareBlockersResult(blocker1, blocker2);

            var result = m_result.GetBlockerPairs(m_game);

            Assert.Collections.AreEqual(new[] { m_card, m_card }, from r in result select r.Key);
            Assert.Collections.AreEqual(new[] { m_card, card2 }, from r in result select r.Value);
        }

        [Test]
        public void Test_ComputeHash()
        {
            m_card.Zone = m_game.Zones.Battlefield;
            m_card.Power = 3;

            var card2 = CreateCard(m_playerB);
            card2.Zone = m_game.Zones.Battlefield;
            card2.Power = 5; // Make sure cards are different

            var blocker1 = new DeclareBlockersResult.BlockingCreature(m_card, m_card);
            var blocker2 = new DeclareBlockersResult.BlockingCreature(card2, m_card);

            Assert_HashIsEqual(new DeclareBlockersResult(), new DeclareBlockersResult());
            Assert_HashIsEqual(new DeclareBlockersResult(blocker1), new DeclareBlockersResult(blocker1));
            Assert_HashIsEqual(new DeclareBlockersResult(blocker1, blocker2), new DeclareBlockersResult(blocker1, blocker2));
            Assert_HashIsEqual(new DeclareBlockersResult(blocker2, blocker1), new DeclareBlockersResult(blocker1, blocker2));

            Assert_HashIsNotEqual(new DeclareBlockersResult(blocker1), new DeclareBlockersResult(blocker2));
        }

        #endregion
    }
}
