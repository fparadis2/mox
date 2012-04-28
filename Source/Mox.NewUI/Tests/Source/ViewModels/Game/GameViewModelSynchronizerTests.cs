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
using System.Linq;
using NUnit.Framework;

namespace Mox.UI.Game
{
    [TestFixture]
    public class GameViewModelSynchronizerTests : BaseGameTests
    {
        #region Variables

        private GameViewModel m_gameViewModel;
        private GameViewModelSynchronizer m_synchronizer;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_playerA.Life = 42;

            m_gameViewModel = new GameViewModel();
            m_synchronizer = new GameViewModelSynchronizer(m_gameViewModel, m_game, m_playerA, null);
        }

        [TearDown]
        public void TearDown()
        {
            m_synchronizer.Dispose();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new GameViewModelSynchronizer(null, m_game, m_playerA, null); });
            Assert.Throws<ArgumentNullException>(delegate { new GameViewModelSynchronizer(m_gameViewModel, null, m_playerA, null); });
            Assert.Throws<ArgumentNullException>(delegate { new GameViewModelSynchronizer(m_gameViewModel, m_game, null, null); });

            Assert.Throws<ArgumentException>(delegate { new GameViewModelSynchronizer(m_gameViewModel, m_game, new Mox.Game().CreatePlayer(), null); });
        }

        [Test]
        public void Test_Cards_in_the_hand_of_the_player_are_synchronized()
        {
            m_card.Controller = m_playerA;
            m_card.Zone = m_game.Zones.Hand;

            Assert.AreEqual(1, m_gameViewModel.MainPlayer.Hand.Count);
            Assert.AreEqual(m_card.Identifier, m_gameViewModel.MainPlayer.Hand[0].Identifier);

            m_card.Zone = m_game.Zones.Library;
            Assert.AreEqual(0, m_gameViewModel.MainPlayer.Hand.Count);
        }

        [Test]
        public void Test_Cards_are_synchronized_in_the_main_model_list()
        {
            m_card.Controller = m_playerA;
            m_card.Zone = m_game.Zones.Hand;

            Assert.AreEqual(1, m_gameViewModel.AllCards.Count);
            Assert.Collections.AreEquivalent(m_gameViewModel.MainPlayer.Hand, m_gameViewModel.AllCards);

            m_game.Cards.Remove(m_card);

            Assert.Collections.IsEmpty(m_gameViewModel.AllCards);
        }

        [Test]
        public void Test_Stack_is_synchronized()
        {
            m_card.Zone = m_game.Zones.Stack;

            Assert.AreEqual(1, m_gameViewModel.Stack.Count);
        }

        [Test]
        public void Test_Player_properties_are_synchronized()
        {
            m_playerA.Life = 10;
            Assert.AreEqual(10, m_gameViewModel.MainPlayer.Life);
        }

        [Test]
        public void Test_Player_properties_are_synchronized_even_for_properties_set_before_synchronization_began()
        {
            Assert.AreEqual(42, m_gameViewModel.MainPlayer.Life);
        }

        [Test]
        public void Test_Player_manapool_is_correctly_synchronized()
        {
            m_playerA.ManaPool[Color.None] += 2;
            m_playerA.ManaPool[Color.Red] += 10;

            Assert.AreEqual(2, m_gameViewModel.MainPlayer.ManaPool.Mana[Color.None]);
            Assert.AreEqual(10, m_gameViewModel.MainPlayer.ManaPool.Mana[Color.Red]);
        }

        [Test]
        public void Test_Players_are_synchronized()
        {
            Assert.AreEqual(2, m_gameViewModel.Players.Count);
            Assert.AreEqual(m_playerA, m_gameViewModel.Players[0].Source);
            Assert.AreEqual(m_playerB, m_gameViewModel.Players[1].Source);
        }

        [Test]
        public void Test_MainPlayer_is_synchronized()
        {
            Assert.AreEqual(m_playerA, m_gameViewModel.MainPlayer.Source);
        }

        [Test]
        public void Test_State_is_synchronized()
        {
            m_game.State.ActivePlayer = m_playerA;
            m_game.State.CurrentPhase = Phases.Beginning;
            m_game.State.CurrentStep = Steps.Draw;

            Assert.AreEqual(m_gameViewModel.Players[0], m_gameViewModel.State.ActivePlayer);
            Assert.AreEqual(MTGSteps.Draw, m_gameViewModel.State.CurrentMTGStep);
        }

        [Test]
        public void Test_Card_properties_are_synchronized()
        {
            m_card.Power = 10;
            m_card.Tapped = true;

            CardViewModel cardViewModel = m_gameViewModel.AllCards.First();
            Assert.AreEqual(10, cardViewModel.Power);
            Assert.AreEqual(true, cardViewModel.Tapped);
        }

        #endregion
    }
}
