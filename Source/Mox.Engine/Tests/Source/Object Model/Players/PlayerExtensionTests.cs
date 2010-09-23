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

using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class PlayerExtensionTests : BaseGameTests
    {
        #region Test

        [Test]
        public void Test_GainLife()
        {
            Assert.AreEqual(20, m_playerA.Life);
            m_playerA.GainLife(3);
            Assert.AreEqual(23, m_playerA.Life);
            m_playerA.GainLife(0);
            Assert.AreEqual(23, m_playerA.Life);
        }

        [Test]
        public void Test_Invalid_GainLife_arguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => m_playerA.GainLife(-1));
        }

        [Test]
        public void Test_LoseLife()
        {
            Assert.AreEqual(20, m_playerA.Life);
            m_playerA.LoseLife(3);
            Assert.AreEqual(17, m_playerA.Life);
            m_playerA.LoseLife(0);
            Assert.AreEqual(17, m_playerA.Life);
        }

        [Test]
        public void Test_Invalid_LoseLife_arguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => m_playerA.LoseLife(-1));
        }

        #endregion
    }
}
