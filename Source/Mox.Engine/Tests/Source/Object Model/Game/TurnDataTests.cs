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
using Mox.Rules;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class TurnDataTests : BaseGameTests
    {
        #region Setup / Teardown

        #endregion

        #region Tests

        [Test]
        public void Test_Can_access_the_turn_data_from_the_game()
        {
            Assert.IsNotNull(m_game.TurnData);
            Assert.Collections.Contains(m_game.TurnData, m_game.Objects);
        }

        [Test]
        public void Test_CanPlayLand_returns_true_for_the_first_land_of_the_turn()
        {
            Assert.IsTrue(m_game.TurnData.CanPlayLand);
        }

        [Test]
        public void Test_CanPlayLand_returns_false_for_the_other_lands_of_the_turn()
        {
            m_game.TurnData.PlayOneLand();
            Assert.IsFalse(m_game.TurnData.CanPlayLand);
        }

        [Test]
        public void Test_CanPlayLand_always_returns_true_when_bypassing()
        {
            m_game.TurnData.PlayOneLand();

            using (OneLandPerTurn.Bypass())
            {
                Assert.IsTrue(m_game.TurnData.CanPlayLand);
            }
        }

        #endregion
    }
}
