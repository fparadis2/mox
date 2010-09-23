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

namespace Mox
{
    [TestFixture]
    public class GameStateTests : BaseGameTests
    {
        #region Setup / Teardown

        #endregion

        #region Tests

        [Test]
        public void Test_Can_access_the_game_state_from_the_game()
        {
            Assert.IsNotNull(m_game.State);
            Assert.Collections.Contains(m_game.State, m_game.Objects);
        }

        [Test]
        public void Test_Can_get_set_CurrentStep()
        {
            m_game.State.CurrentStep = Steps.Draw;
            Assert.AreEqual(Steps.Draw, m_game.State.CurrentStep);
        }

        [Test]
        public void Test_Can_get_set_CurrentPhase()
        {
            m_game.State.CurrentPhase = Phases.Beginning;
            Assert.AreEqual(Phases.Beginning, m_game.State.CurrentPhase);
        }

        [Test]
        public void Test_Can_get_set_ActivePlayer()
        {
            Assert.IsNull(m_game.State.ActivePlayer);
            m_game.State.ActivePlayer = m_playerA;
            Assert.AreEqual(m_playerA, m_game.State.ActivePlayer);
        }

        [Test]
        public void Test_Can_get_set_Winner()
        {
            Assert.IsNull(m_game.State.Winner);
            m_game.State.Winner = m_playerA;
            Assert.AreEqual(m_playerA, m_game.State.Winner);
        }

        [Test]
        public void Test_Can_get_set_HasEnded()
        {
            Assert.That(!m_game.State.HasEnded);
            m_game.State.Winner = m_playerA;
            Assert.That(m_game.State.HasEnded);
        }

        [Test]
        public void Test_Can_get_set_CurrentTurn()
        {
            Assert.AreEqual(0, m_game.State.CurrentTurn);
            m_game.State.CurrentTurn = 3;
            Assert.AreEqual(3, m_game.State.CurrentTurn);
        }

        #endregion
    }
}
