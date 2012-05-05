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
            m_manaPoolViewModel.Colorless.Amount = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Colorless.Amount);
        }

        [Test]
        public void Test_Can_get_set_White()
        {
            m_manaPoolViewModel.White.Amount = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.White.Amount);
        }

        [Test]
        public void Test_Can_get_set_Blue()
        {
            m_manaPoolViewModel.Blue.Amount = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Blue.Amount);
        }

        [Test]
        public void Test_Can_get_set_Black()
        {
            m_manaPoolViewModel.Black.Amount = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Black.Amount);
        }

        [Test]
        public void Test_Can_get_set_Red()
        {
            m_manaPoolViewModel.Red.Amount = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Red.Amount);
        }

        [Test]
        public void Test_Can_get_set_Green()
        {
            m_manaPoolViewModel.Green.Amount = 2;
            Assert.AreEqual(2, m_manaPoolViewModel.Green.Amount);
        }

        [Test]
        public void Test_Can_get_whether_a_mana_can_be_paid()
        {
            foreach (var mana in m_manaPoolViewModel.AllMana)
            {
                Assert.IsFalse(mana.CanPay);
                mana.CanPay = true;
                Assert.IsTrue(mana.CanPay);
            }
        }

        [Test]
        public void Test_PayMana_can_only_execute_if_the_mana_can_be_paid()
        {
            Assert.IsFalse(m_manaPoolViewModel.CanPayMana(m_manaPoolViewModel.Red));

            m_manaPoolViewModel.Red.CanPay = true;
            Assert.IsTrue(m_manaPoolViewModel.CanPayMana(m_manaPoolViewModel.Red));
        }

        [Test]
        public void Test_PayMana_command_triggers_the_ManaPaid_event()
        {
            m_manaPoolViewModel.Red.CanPay = true;

            EventSink<ItemEventArgs<Color>> sink = new EventSink<ItemEventArgs<Color>>(m_manaPoolViewModel);
            m_manaPoolViewModel.ManaPaid += sink;

            Assert.EventCalledOnce(sink, () => m_manaPoolViewModel.PayMana(m_manaPoolViewModel.Red));
            Assert.AreEqual(Color.Red, sink.LastEventArgs.Item);
        }

        [Test]
        public void Test_PayMana_command_does_not_trigger_the_ManaPaid_event_on_invalid_mana()
        {
            EventSink<ItemEventArgs<Color>> sink = new EventSink<ItemEventArgs<Color>>(m_manaPoolViewModel);
            m_manaPoolViewModel.ManaPaid += sink;

            Assert.EventNotCalled(sink, () => m_manaPoolViewModel.PayMana(m_manaPoolViewModel.Red));
        }

        #endregion
    }
}
