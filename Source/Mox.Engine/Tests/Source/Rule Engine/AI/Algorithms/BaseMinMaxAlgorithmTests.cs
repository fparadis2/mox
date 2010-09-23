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
using Rhino.Mocks;

namespace Mox.AI
{
    [TestFixture]
    public class BaseMinMaxAlgorithmTests : BaseGameTests
    {
        #region Variables

        private BaseMinMaxAlgorithm m_algorithm;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_algorithm = m_mockery.PartialMock<BaseMinMaxAlgorithm>(m_playerA, new AIParameters());
            m_mockery.Replay(m_algorithm);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_IsMaximizingPlayer_returns_true_for_the_player_passed_in_the_constructor()
        {
            Assert.That( m_algorithm.IsMaximizingPlayer(m_playerA));
            Assert.That(!m_algorithm.IsMaximizingPlayer(m_playerB));
            Assert.That(!m_algorithm.IsMaximizingPlayer(null));
        }

        #endregion
    }
}
