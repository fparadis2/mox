﻿// Copyright (c) François Paradis
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
    public class SimpleManaPoolTests
    {
        #region Variables

        private ManaPool m_manaPool;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_manaPool = new ManaPool();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_mana()
        {
            m_manaPool.Blue = 4;
            Assert.AreEqual(4, m_manaPool.Blue);
        }

        [Test]
        public void Test_Can_construct_from_original_to_create_an_independent_copy()
        {
            m_manaPool.Blue = 4;
            m_manaPool.Colorless = 2;

            ManaPool clone = new ManaPool(m_manaPool);
            Assert.AreEqual(4, clone.Blue);
            Assert.AreEqual(2, clone.Colorless);

            // Make sure it's independent
            clone.Blue = 3;
            Assert.AreEqual(4, m_manaPool.Blue);
        }

        [Test]
        public void Test_Can_get_total_mana_amount()
        {
            m_manaPool.Blue = 4;
            m_manaPool.Red = 5;
            m_manaPool.Colorless = 2;

            Assert.AreEqual(11, m_manaPool.TotalManaAmount);
        }

        #endregion
    }
}
