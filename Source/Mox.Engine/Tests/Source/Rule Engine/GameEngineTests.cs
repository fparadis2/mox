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
using System.Collections.Generic;
using Rhino.Mocks;

namespace Mox
{
    [TestFixture]
    public class GameEngineTests : BaseGameTests
    {
        #region Variables

        private GameEngine m_gameEngine;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_gameEngine = new GameEngine(m_game);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_Values()
        {
            Assert.IsNotNull(m_gameEngine.Controller);
            Assert.AreEqual(m_game, m_gameEngine.Game);
            Assert.IsNotNull(m_gameEngine.AISupervisor);
        }

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new GameEngine(null); });
        }

        #endregion
    }
}
