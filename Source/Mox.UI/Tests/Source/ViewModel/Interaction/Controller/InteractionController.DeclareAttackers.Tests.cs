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
using System.Collections.Generic;

namespace Mox.UI
{
    [TestFixture]
    public class InteractionControllerDeclareAttackersTests : InteractionControllerTester
    {
        #region Tests

        [Test]
        public void Test_BeginDeclareAttackers_returns_an_empty_result_if_passing()
        {
            m_mockAbility.Expect_CanPlay();
            m_mockery.ReplayAll();

            IInteraction<DeclareAttackersResult> interaction = InteractionController.BeginDeclareAttackers(new DeclareAttackersContext(new[] { m_card }));
            {
                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(1, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Assert.AreEqual("Pass", Model.Interaction.UserChoiceInteraction.Choices[0].Text);
                Model.Interaction.SelectChoiceCommand.Execute(Model.Interaction.UserChoiceInteraction.Choices.First());
            }
            Assert.IsTrue(interaction.IsCompleted);
            Assert.IsTrue(interaction.Result.IsEmpty);
        }

        [Test]
        public void Test_BeginDeclareAttackers_returns_all_the_selected_attackers()
        {
            List<Card> cards = new List<Card>();
            for (int i = 0; i < 4; i++)
            {
                cards.Add(CreateCard(m_playerA));
            }

            IInteraction<DeclareAttackersResult> interaction = InteractionController.BeginDeclareAttackers(new DeclareAttackersContext(cards.Take(3)));
            {
                for (int i = 0; i < 3; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    Assert.IsTrue(cardViewModel.CanBeChosen);
                }

                for (int i = 3; i < 4; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    Assert.IsFalse(cardViewModel.CanBeChosen);
                }

                for (int i = 0; i < 2; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    cardViewModel.Choose();

                    Assert.IsFalse(cardViewModel.CanBeChosen);
                }

                Assert.IsFalse(interaction.IsCompleted);

                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(2, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Assert.AreEqual("Continue", Model.Interaction.UserChoiceInteraction.Choices[0].Text);
                Model.Interaction.SelectChoiceCommand.Execute(Model.Interaction.UserChoiceInteraction.Choices.First());
            }
            Assert.IsTrue(interaction.IsCompleted);
            Assert.Collections.AreEquivalent(cards.Take(2), interaction.Result.GetAttackers(m_game));
        }

        [Test]
        public void Test_Can_cancel_the_selection()
        {
            List<Card> cards = new List<Card>();
            for (int i = 0; i < 2; i++)
            {
                cards.Add(CreateCard(m_playerA));
            }

            for (int i = 0; i < 1; i++)
            {
                Card card = cards[i];
                card.Zone = m_game.Zones.Battlefield;
            }

            IInteraction<DeclareAttackersResult> interaction = InteractionController.BeginDeclareAttackers(new DeclareAttackersContext(cards));
            {
                for (int i = 0; i < 1; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    cardViewModel.Choose();
                }

                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(2, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Assert.AreEqual("Cancel", Model.Interaction.UserChoiceInteraction.Choices[1].Text);
                Model.Interaction.SelectChoiceCommand.Execute(Model.Interaction.UserChoiceInteraction.Choices[1]);

                for (int i = 0; i < 1; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    Assert.That(cardViewModel.CanBeChosen);
                }

                Assert.IsFalse(interaction.IsCompleted);
            }
        }

        #endregion
    }
}
