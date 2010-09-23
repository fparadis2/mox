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
using Rhino.Mocks;

namespace Mox.UI
{
    [TestFixture]
    public class InteractionControllerGivePriorityTests : InteractionControllerTester
    {
        #region Tests

        [Test]
        public void Test_BeginGivePriority_returns_null_if_passing()
        {
            m_mockAbility.Expect_CanPlay();
            m_mockery.ReplayAll();

            IInteraction<Action> interaction = InteractionController.BeginGivePriority();
            {
                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(1, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Model.Interaction.SelectChoiceCommand.Execute(Model.Interaction.UserChoiceInteraction.Choices.First());
            }
            Assert.IsTrue(interaction.IsCompleted);
            Assert.IsNull(interaction.Result);
        }

        [Test]
        public void Test_BeginGivePriority_returns_the_first_playable_cards_ability_when_choosing_a_card()
        {
            MockAbility playableAbility = CreateMockAbility(m_card, AbilityType.Normal);

            m_mockAbility.Expect_CannotPlay().Repeat.Twice();
            playableAbility.Expect_CanPlay().Repeat.Twice();
            m_mockery.ReplayAll();
            
            IInteraction<Action> interaction = InteractionController.BeginGivePriority();
            {
                CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(m_card);
                Assert.IsTrue(cardViewModel.CanBeChosen);
                cardViewModel.Choose();
            }
            Assert.IsTrue(interaction.IsCompleted);
            Assert.IsNotNull(interaction.Result);
            Assert.IsInstanceOf<PlayAbility>(interaction.Result);

            PlayAbility ability = (PlayAbility)interaction.Result;
            Assert.AreEqual(playableAbility, ability.Ability.Resolve(m_game));
        }

        [Test]
        public void Test_Cannot_choose_a_card_that_has_no_playable_ability()
        {
            m_mockAbility.Expect_CannotPlay();
            m_mockery.ReplayAll();

            IInteraction<Action> interaction = InteractionController.BeginGivePriority();
            {
                CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(m_card);
                Assert.IsFalse(cardViewModel.CanBeChosen);
                cardViewModel.Choose();
            }
            Assert.IsFalse(interaction.IsCompleted);
        }

        #endregion
    }
}
