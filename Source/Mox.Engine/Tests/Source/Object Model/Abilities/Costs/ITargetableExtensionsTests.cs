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
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class ITargetableExtensionsTests : BaseGameTests
    {
        #region Setup / Teardown

        #endregion

        #region Tests

        [Test]
        public void Test_DealDamage_to_creature_adds_damage()
        {
            m_card.Type = Type.Creature;
            m_card.Damage = 2;

            m_card.DealDamage(3);

            Assert.AreEqual(5, m_card.Damage);
        }

        [Test]
        public void Test_DealDamage_to_player_removes_life()
        {
            m_playerA.Life = 10;
            m_playerA.DealDamage(2);
            Assert.AreEqual(8, m_playerA.Life);
        }

        #endregion
    }
}
