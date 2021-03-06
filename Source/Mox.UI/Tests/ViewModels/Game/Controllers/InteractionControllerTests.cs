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

namespace Mox.UI.Game
{
    [TestFixture]
    public class InteractionControllerTests : InteractionControllerTester
    {
        #region Tests

        [Test]
        public void Test_At_the_end_of_an_interaction_the_game_view_model_is_reset()
        {
            CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(m_card);

            IMockInteraction interaction = InteractionController.BeginMockInteraction();
            {
                Model.Interaction.UserChoiceInteraction = new UserChoiceInteractionModel();
                Model.MainPlayer.ManaPool.Green.CanPay = true;
                cardViewModel.InteractionType = InteractionType.Attack;
                Model.MainPlayer.CanBeChosen = true;
                interaction.End(3);
            }
            Assert.IsNull(Model.Interaction.UserChoiceInteraction);
            Assert.IsFalse(Model.MainPlayer.ManaPool.Green.CanPay);
            Assert.AreEqual(InteractionType.None, cardViewModel.InteractionType);
            Assert.IsFalse(Model.MainPlayer.CanBeChosen);
        }

        [Test]
        public void Test_At_the_end_of_an_interaction_the_result_is_sent()
        {
            IMockInteraction interaction = InteractionController.BeginMockInteraction();
            {
                interaction.End(3);
            }

            Assert.That(InteractionController.IsCompleted);
            Assert.AreEqual(3, InteractionController.Result);
        }

        #endregion
    }
}
