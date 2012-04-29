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
using Mox.Flow;
using NUnit.Framework;

namespace Mox.UI.Game
{
    [TestFixture]
    public class InteractionControllerTargetTests : InteractionControllerTester
    {
        #region Utilities

        private static TargetChoice CreateChoice(bool allowCancel, int[] targets, TargetContextType type = TargetContextType.Normal)
        {
            return new TargetChoice(EmptyPlayer, new TargetContext(allowCancel, targets, type));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_BeginTarget_returns_InvalidIdentifier_if_cancelling()
        {
            InteractionController.BeginInteraction(CreateChoice(true, new int[0]));
            {
                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(1, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Model.Interaction.SelectChoice(Model.Interaction.UserChoiceInteraction.Choices.First());
            }
            Assert.IsTrue(IsCompleted);
            Assert.AreEqual(ObjectManager.InvalidIdentifier, Result);
        }

        [Test]
        public void Test_User_cannot_cancel_if_AllowCancel_is_false()
        {
            InteractionController.BeginInteraction(CreateChoice(false, new int[0]));
            {
                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.Collections.IsEmpty(Model.Interaction.UserChoiceInteraction.Choices);
            }
            Assert.IsFalse(IsCompleted);
        }

        [Test]
        public void Test_BeginTarget_returns_the_identifier_of_the_selected_card()
        {
            Card otherCard = CreateCard(m_playerA);

            InteractionController.BeginInteraction(CreateChoice(true, new[] { m_card.Identifier }));
            {
                Assert.IsFalse(m_synchronizer.GetCardViewModel(otherCard).CanBeChosen);

                CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(m_card);
                Assert.IsTrue(cardViewModel.CanBeChosen);
                cardViewModel.Choose();
            }
            Assert.IsTrue(IsCompleted);
            Assert.AreEqual(m_card.Identifier, Result);
        }

        [Test]
        public void Test_BeginTarget_returns_the_identifier_of_the_selected_player()
        {
            InteractionController.BeginInteraction(CreateChoice(true, new[] { m_playerB.Identifier }));
            {
                Assert.IsFalse(m_synchronizer.GetPlayerViewModel(m_playerA).CanBeChosen);

                PlayerViewModel playerViewModel = m_synchronizer.GetPlayerViewModel(m_playerB);
                Assert.IsTrue(playerViewModel.CanBeChosen);
                playerViewModel.Choose();
            }
            Assert.IsTrue(IsCompleted);
            Assert.AreEqual(m_playerB.Identifier, Result);
        }

        #endregion
    }
}
