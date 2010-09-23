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

namespace Mox.AI
{
    [TestFixture]
    public class AIParametersTests
    {
        #region Variables

        private AIParameters m_params;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_params = new AIParameters();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_MinimumTreeDepth()
        {
            Assert.AreEqual(4, m_params.MinimumTreeDepth);
            m_params.MinimumTreeDepth = 6;
            Assert.AreEqual(6, m_params.MinimumTreeDepth);
        }

        [Test]
        public void Test_MinimumTreeDepth_invalid_values()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { m_params.MinimumTreeDepth = -1; });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { m_params.MinimumTreeDepth = 0; });
            m_params.MinimumTreeDepth = 1; // 1 is ok
        }

        [Test]
        public void Test_MaximumSpellStackDepth()
        {
#if DEBUG
            Assert.AreEqual(1, m_params.MaximumSpellStackDepth);
#else
            Assert.AreEqual(2, m_params.MaximumSpellStackDepth);
#endif
            m_params.MaximumSpellStackDepth = 6;
            Assert.AreEqual(6, m_params.MaximumSpellStackDepth);
        }

        [Test]
        public void Test_MaximumSpellStackDepth_invalid_values()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { m_params.MaximumSpellStackDepth = -1; });
            m_params.MaximumSpellStackDepth = 0; // ok
            m_params.MinimumTreeDepth = 1; // ok
        }

        [Test]
        public void Test_MinMaxDriverType()
        {
            Assert.AreEqual(AIParameters.MinMaxDriverType.Iterative, m_params.DriverType);
            m_params.DriverType = AIParameters.MinMaxDriverType.Recursive;
            Assert.AreEqual(AIParameters.MinMaxDriverType.Recursive, m_params.DriverType);
        }

        #endregion
    }
}
