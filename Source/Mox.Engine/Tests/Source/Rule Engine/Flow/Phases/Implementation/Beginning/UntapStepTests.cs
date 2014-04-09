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
using System.Linq;
using System.Text;
using Mox.Rules;
using NUnit.Framework;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class UntapStepTests : BaseStepTests<UntapStep>
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_step = new UntapStep();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(Steps.Untap, m_step.Type);
        }

        [Test]
        public void Test_Step_untaps_all_permanents()
        {
            Card card1 = CreateCard(m_playerA); card1.Tapped = true;
            Card card2 = CreateCard(m_playerB); card2.Tapped = true;

            RunStep(m_playerA);

            Assert.IsFalse(card1.Tapped);
            Assert.IsTrue(card2.Tapped); // Can only untap cards the active player controls
        }

        [Test]
        public void Test_Step_removes_summoning_sickness_on_controlled_cards()
        {
            Card card1 = CreateCard(m_playerA); card1.Type = Type.Creature;
            Card card2 = CreateCard(m_playerB); card2.Type = Type.Creature;

            card1.HasSummoningSickness = true;
            card2.HasSummoningSickness = true;

            Assert.IsTrue(card1.HasSummoningSickness, "Sanity check");

            RunStep(m_playerA);

            Assert.IsFalse(card1.HasSummoningSickness);
            Assert.IsTrue(card2.HasSummoningSickness); // Doesn't remove sickness for opponents
        }

        #endregion
    }
}
