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

namespace Mox.Rules
{
    [TestFixture]
    public class SummoningSicknessTests : BaseGameTests
    {
        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_card.Type = Type.Creature;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Cards_dont_have_summoning_sickness_by_default()
        {
            Assert.IsFalse(m_card.HasSummoningSickness());
        }

        [Test]
        public void Test_Can_set_and_remove_summoning_sickness()
        {
            SummoningSickness.SetSickness(m_card);
            Assert.IsTrue(m_card.HasSummoningSickness());
            SummoningSickness.RemoveSickness(m_card);
            Assert.IsFalse(m_card.HasSummoningSickness());
        }

        [Test]
        public void Test_Cards_with_haste_dont_have_summoning_sickness()
        {
            SummoningSickness.SetSickness(m_card);
            m_game.CreateAbility<HasteAbility>(m_card);
            Assert.IsFalse(m_card.HasSummoningSickness());
        }

        [Test]
        public void Test_Non_creatures_never_have_sickness()
        {
            SummoningSickness.SetSickness(m_card);

            m_card.Type = Type.Artifact;
            Assert.IsFalse(m_card.HasSummoningSickness());

            m_card.Type = Type.Creature; // Can put it back and get sickness back
            Assert.IsTrue(m_card.HasSummoningSickness());
        }

        [Test]
        public void Test_A_card_that_comes_into_play_has_sickness()
        {
            Assert.IsFalse(m_card.HasSummoningSickness());

            m_card.Zone = m_game.Zones.Battlefield;

            Assert.IsTrue(m_card.HasSummoningSickness());
        }

        [Test]
        public void Test_HasSummoningSickness_always_returns_false_when_bypassing()
        {
            SummoningSickness.SetSickness(m_card);

            using (SummoningSickness.Bypass())
            {
                Assert.IsFalse(m_card.HasSummoningSickness());
            }
        }

        #endregion
    }
}
