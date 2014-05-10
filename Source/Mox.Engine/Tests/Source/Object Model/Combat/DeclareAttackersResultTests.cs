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
using Mox.Flow;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class DeclareAttackersResultTests : BaseGameTests
    {
        #region Variables

        private DeclareAttackersResult m_result;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_result = new DeclareAttackersResult();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_construction_values()
        {
            Assert.Collections.IsEmpty(m_result.GetAttackers(m_game));
        }

        [Test]
        public void Test_Empty_returns_an_empty_result_with_no_attackers()
        {
            Assert.Collections.IsEmpty(DeclareAttackersResult.Empty.GetAttackers(m_game));
        }

        [Test]
        public void Test_Can_construct_with_attackers()
        {
            m_result = new DeclareAttackersResult(m_card, m_card); // Duplicates are removed.
            Assert.Collections.AreEqual(new [] { m_card }, m_result.GetAttackers(m_game));
        }

        [Test]
        public void Test_Is_serializable()
        {
            Assert.IsSerializable(m_result);
        }

        [Test]
        public void Test_IsEmpty()
        {
            Assert.IsTrue(new DeclareAttackersResult().IsEmpty);
            Assert.IsFalse(new DeclareAttackersResult(m_card).IsEmpty);
        }

        [Test]
        public void Test_ComputeHash()
        {
            var otherCard = CreateCard(m_playerB, "Another card");

            Assert_HashIsEqual(new DeclareAttackersResult(), new DeclareAttackersResult());
            Assert_HashIsEqual(new DeclareAttackersResult(m_card), new DeclareAttackersResult(m_card));
            Assert_HashIsEqual(new DeclareAttackersResult(m_card), new DeclareAttackersResult(m_card, m_card));
            Assert_HashIsEqual(new DeclareAttackersResult(otherCard, m_card), new DeclareAttackersResult(m_card, otherCard));

            Assert_HashIsNotEqual(new DeclareAttackersResult(m_card), new DeclareAttackersResult(otherCard));
        }

        #endregion
    }
}
