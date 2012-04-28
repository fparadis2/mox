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
    public class PlayerViewModelTests : BaseGameViewModelTests
    {
        #region Variables

        private PlayerViewModel m_playerViewModel;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_playerViewModel = new PlayerViewModel(m_gameViewModel);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_life()
        {
            m_playerViewModel.Life = 20;
            Assert.AreEqual(20, m_playerViewModel.Life);
        }

        [Test]
        public void Test_Can_get_set_Name()
        {
            m_playerViewModel.Name = "My player";
            Assert.AreEqual("My player", m_playerViewModel.Name);
        }

        [Test]
        public void Test_Can_get_Hand()
        {
            Assert.IsNotNull(m_playerViewModel.Hand);
        }

        [Test]
        public void Test_Can_get_Library()
        {
            Assert.IsNotNull(m_playerViewModel.Library);
        }

        [Test]
        public void Test_Can_get_Graveyard()
        {
            Assert.IsNotNull(m_playerViewModel.Graveyard);
        }

        [Test]
        public void Test_Can_get_ManaPool()
        {
            Assert.IsNotNull(m_playerViewModel.ManaPool);
        }

        [Test]
        public void Test_Can_get_set_CanBeChosen()
        {
            Assert.IsFalse(m_playerViewModel.CanBeChosen);
            m_playerViewModel.CanBeChosen = true;
            Assert.IsTrue(m_playerViewModel.CanBeChosen);
        }

        [Test]
        public void Test_IsMainPlayer()
        {
            m_gameViewModel.MainPlayer = null;
            Assert.IsFalse(m_playerViewModel.IsMainPlayer);

            m_gameViewModel.MainPlayer = m_playerViewModel;
            Assert.IsTrue(m_playerViewModel.IsMainPlayer);
        }

        #region ChooseCommand

        [Test]
        public void Test_Cannot_choose_a_player_that_cannot_be_chosen()
        {
            m_playerViewModel.CanBeChosen = false;
            Assert.IsFalse(m_playerViewModel.ChooseCommand.CanExecute(null));
        }

        [Test]
        public void Test_Can_choose_a_player_that_can_be_chosen()
        {
            m_playerViewModel.CanBeChosen = true;
            Assert.IsTrue(m_playerViewModel.ChooseCommand.CanExecute(null));
        }

        [Test]
        public void Test_Choosing_a_player_triggers_the_PlayerChosen_event()
        {
            m_playerViewModel.CanBeChosen = true;

            EventSink<PlayerChosenEventArgs> sink = new EventSink<PlayerChosenEventArgs>(m_gameViewModel.Interaction);
            m_gameViewModel.Interaction.PlayerChosen += sink;
            Assert.EventCalledOnce(sink, () => m_playerViewModel.ChooseCommand.Execute(null));
            Assert.EventCalledOnce(sink, () => m_playerViewModel.Choose());

            Assert.AreEqual(m_playerViewModel, sink.LastEventArgs.Player);
        }

        #endregion

        #region PayManaCommand

        [Test]
        public void Test_PayMana_command_can_only_execute_if_the_mana_can_be_paid()
        {
            Assert.IsFalse(m_playerViewModel.PayMana.CanExecute(Color.Red));

            m_playerViewModel.ManaPool.CanPay[Color.Red] = true;
            Assert.IsTrue(m_playerViewModel.PayMana.CanExecute(Color.Red));
        }

        [Test]
        public void Test_PayMana_command_triggers_the_ManaPaid_event()
        {
            m_playerViewModel.ManaPool.CanPay[Color.Red] = true;

            EventSink<ItemEventArgs<Color>> sink = new EventSink<ItemEventArgs<Color>>(m_playerViewModel);
            m_playerViewModel.ManaPaid += sink;

            Assert.EventCalledOnce(sink, () => m_playerViewModel.PayMana.Execute(Color.Red));
            Assert.AreEqual(Color.Red, sink.LastEventArgs.Item);
        }

        [Test]
        public void Test_PayMana_command_does_not_trigger_the_ManaPaid_event_on_invalid_mana()
        {
            EventSink<ItemEventArgs<Color>> sink = new EventSink<ItemEventArgs<Color>>(m_playerViewModel);
            m_playerViewModel.ManaPaid += sink;

            Assert.EventNotCalled(sink, () => m_playerViewModel.PayMana.Execute(Color.Red));
        }

        #endregion

        #endregion
    }
}
