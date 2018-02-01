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

namespace Mox.Abilities
{
    [TestFixture]
    public class SacrificeCostTests : CostTestsBase
    {
        #region Variables

        private SacrificeCost m_cost;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_cost = new SacrificeCost(m_card);
            m_card.Zone = m_game.Zones.Battlefield;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new SacrificeCost(null); });
        }

        [Test]
        public void Test_Can_only_be_paid_if_the_permanents_are_in_play()
        {
            m_card.Zone = m_game.Zones.Graveyard;
            Execute(m_cost, false);

            m_card.Zone = m_game.Zones.Battlefield;
            Execute(m_cost, true);
        }

        [Test]
        public void Test_Can_only_be_paid_if_the_permanents_are_controlled_by_the_spell_controller()
        {
            m_card.Controller = m_playerB;
            Execute(m_cost, false);

            m_card.Controller = m_playerA;
            Execute(m_cost, true);
        }

        [Test]
        public void Test_Execute()
        {
            Execute(m_cost, true);
            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
        }

        #endregion
    }
}
