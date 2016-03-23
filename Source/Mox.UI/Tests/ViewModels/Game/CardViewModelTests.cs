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
    public class CardViewModelTests : BaseGameViewModelTests
    {
        #region Variables

        private CardViewModel m_cardViewModel;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_cardViewModel = new CardViewModel(m_gameViewModel);
            m_cardViewModel.Source = m_card;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new CardViewModel(null));
        }

        [Test]
        public void Test_construction_arguments()
        {
            Assert.AreEqual(m_gameViewModel, m_cardViewModel.GameViewModel);
        }

        [Test]
        public void Test_Can_get_set_CanBeChoosed()
        {
            Assert.IsFalse(m_cardViewModel.CanChoose);
            m_cardViewModel.CanChoose = true;
            Assert.IsTrue(m_cardViewModel.CanChoose);
        }

        [Test]
        public void Test_Can_get_set_Power_and_toughness()
        {
            Assert.AreEqual(0, m_cardViewModel.Power);
            Assert.AreEqual(0, m_cardViewModel.Toughness);

            m_cardViewModel.PowerAndToughness = new PowerAndToughness { Power = 1, Toughness = 2 };

            Assert.AreEqual(new PowerAndToughness { Power = 1, Toughness = 2 }, m_cardViewModel.PowerAndToughness);
            Assert.AreEqual(1, m_cardViewModel.Power);
            Assert.AreEqual(2, m_cardViewModel.Toughness);
        }

        [Test]
        public void Test_Power_and_toughness_are_only_visible_when_creatures_are_on_the_battlefield()
        {
            m_card.Type = Type.Artifact;
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsFalse(m_cardViewModel.ShowPowerAndToughness);

            m_card.Type = Type.Creature | Type.Artifact;
            m_card.Zone = m_game.Zones.Exile;
            Assert.IsFalse(m_cardViewModel.ShowPowerAndToughness);

            m_card.Type = Type.Creature | Type.Artifact;
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsTrue(m_cardViewModel.ShowPowerAndToughness);
        }

        [Test]
        public void Test_Cannot_choose_a_card_that_cannot_be_chosen()
        {
            Assert.IsFalse(m_cardViewModel.ChooseCommand.CanExecute(null));
        }

        [Test]
        public void Test_Can_choose_a_card_that_can_be_chosen()
        {
            m_cardViewModel.CanChoose = true;
            Assert.IsTrue(m_cardViewModel.ChooseCommand.CanExecute(null));
        }

        [Test]
        public void Test_Choosing_a_card_triggers_the_CardChosen_event()
        {
            m_cardViewModel.CanChoose = true;

            EventSink<CardChosenEventArgs> sink = new EventSink<CardChosenEventArgs>(m_gameViewModel.Interaction);
            m_gameViewModel.Interaction.CardChosen += sink;
            Assert.EventCalledOnce(sink, () => m_cardViewModel.ChooseCommand.Execute(null));
            Assert.EventCalledOnce(sink, () => m_cardViewModel.Choose());

            Assert.AreEqual(m_cardViewModel, sink.LastEventArgs.Card);
        }

        #endregion
    }
}
