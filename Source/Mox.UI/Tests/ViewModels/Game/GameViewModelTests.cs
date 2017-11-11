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
    public class GameViewModelTests : BaseGameViewModelTests
    {
        #region Variables

        private CardViewModel m_cardViewModel;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_cardViewModel = new CardViewModel(m_gameViewModel);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_access_the_interaction_model()
        {
            Assert.IsNotNull(m_gameViewModel.Interaction);
        }

        [Test]
        public void Test_GetSet_the_main_player()
        {
            PlayerViewModel player = CreatePlayerViewModel();

            Assert.IsNull(m_gameViewModel.MainPlayer);
            m_gameViewModel.MainPlayer = player;
            Assert.AreEqual(player, m_gameViewModel.MainPlayer);
        }

        [Test]
        public void Test_Can_access_the_players()
        {
            Assert.IsNotNull(m_gameViewModel.Players);
        }

        [Test]
        public void Test_Can_access_the_stack()
        {
            Assert.IsNotNull(m_gameViewModel.SpellStack);
        }

        [Test]
        public void Test_ResetInteraction_removes_the_user_choice_interaction()
        {
            m_gameViewModel.Interaction.UserChoiceInteraction = new UserChoiceInteractionModel();
            m_gameViewModel.ResetInteraction();
            Assert.IsNull(m_gameViewModel.Interaction.UserChoiceInteraction);
        }

        [Test]
        public void Test_ResetInteraction_resets_InteractionType_on_all_cards()
        {
            m_gameViewModel.AllCards.Add(m_cardViewModel);
            m_cardViewModel.InteractionType = InteractionType.Play;
            m_gameViewModel.ResetInteraction();
            Assert.AreEqual(InteractionType.None, m_cardViewModel.InteractionType);
        }

        [Test]
        public void Test_ResetInteraction_resets_CanBeChosen_on_all_players()
        {
            PlayerViewModel player = CreatePlayerViewModel();
            player.CanBeChosen = true;
            m_gameViewModel.Players.Add(player);

            m_gameViewModel.ResetInteraction();
            Assert.IsFalse(m_gameViewModel.Players[0].CanBeChosen);
        }

        [Test]
        public void Test_Contains_a_State()
        {
            Assert.IsNotNull(m_gameViewModel.State);
        }

        [Test]
        public void Test_IsActivePlayer_returns_true_when_there_is_no_active_player()
        {
            m_gameViewModel.MainPlayer = CreatePlayerViewModel();
            m_gameViewModel.State.ActivePlayer = null;

            Assert.IsTrue(m_gameViewModel.IsActivePlayer);
        }

        [Test]
        public void Test_IsActivePlayer_returns_true_if_the_MainPlayer_is_the_ActivePlayer()
        {
            PlayerViewModel playerA = CreatePlayerViewModel();
            PlayerViewModel playerB = CreatePlayerViewModel();

            m_gameViewModel.MainPlayer = playerA;
            m_gameViewModel.State.ActivePlayer = playerB;

            Assert.IsFalse(m_gameViewModel.IsActivePlayer);

            m_gameViewModel.State.ActivePlayer = playerA;

            Assert.IsTrue(m_gameViewModel.IsActivePlayer);
        }

        [Test]
        public void Test_PropertyChanged_is_triggered_when_the_value_of_the_IsActivePlayer_property_changes()
        {
            PlayerViewModel playerA = CreatePlayerViewModel();
            PlayerViewModel playerB = CreatePlayerViewModel();

            m_gameViewModel.MainPlayer = playerA;
            m_gameViewModel.State.ActivePlayer = playerB;

            Assert.ThatProperty(m_gameViewModel, g => g.IsActivePlayer).RaisesChangeNotificationWhen(() => m_gameViewModel.State.ActivePlayer = playerA);
        }

        #endregion
    }
}
