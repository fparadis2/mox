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

namespace Mox.UI.Game
{
    [TestFixture]
    public class ManaPoolViewModelTests
    {
        #region Variables

        private ManaPoolViewModel m_manaPoolViewModel;

        #endregion
        
        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_manaPoolViewModel = new ManaPoolViewModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_Colorless()
        {
            m_manaPoolViewModel.Mana[Color.None] = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Mana[Color.None]);
        }

        [Test]
        public void Test_Can_get_set_White()
        {
            m_manaPoolViewModel.Mana[Color.White] = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Mana[Color.White]);
        }

        [Test]
        public void Test_Can_get_set_Blue()
        {
            m_manaPoolViewModel.Mana[Color.Blue] = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Mana[Color.Blue]);
        }

        [Test]
        public void Test_Can_get_set_Black()
        {
            m_manaPoolViewModel.Mana[Color.Black] = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Mana[Color.Black]);
        }

        [Test]
        public void Test_Can_get_set_Red()
        {
            m_manaPoolViewModel.Mana[Color.Red] = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Mana[Color.Red]);
        }

        [Test]
        public void Test_Can_get_set_Green()
        {
            m_manaPoolViewModel.Mana[Color.Green] = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Mana[Color.Green]);
        }

        [Test]
        public void Test_Can_get_whether_a_mana_can_be_paid()
        {
            foreach(Color color in Enum.GetValues(typeof(Color)))
            {
                Assert.IsFalse(m_manaPoolViewModel.CanPay[color]);
                m_manaPoolViewModel.CanPay[color] = true;
                Assert.IsTrue(m_manaPoolViewModel.CanPay[color]);
            }
        }

        #endregion
    }
}
