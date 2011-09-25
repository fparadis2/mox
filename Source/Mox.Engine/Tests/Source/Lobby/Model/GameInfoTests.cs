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

namespace Mox.Lobby
{
    [TestFixture]
    public class GameInfoTests
    {
        #region Variables

        private GameInfo m_gameInfo;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_gameInfo = new GameInfo();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(2, m_gameInfo.NumberOfPlayers);
        }

        [Test]
        public void Test_Clone()
        {
            m_gameInfo.NumberOfPlayers = 3;

            var clone = m_gameInfo.Clone();
            Assert.IsNotNull(clone, "clone");

            foreach (var property in typeof(GameInfo).GetProperties())
            {
                Assert.AreEqual(property.GetValue(m_gameInfo, null), property.GetValue(clone, null));
            }
        }

        #endregion
    }
}
