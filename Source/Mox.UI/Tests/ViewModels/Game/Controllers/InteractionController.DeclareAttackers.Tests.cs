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
using System.Collections.Generic;

namespace Mox.UI.Game
{
    [TestFixture]
    public class InteractionControllerDeclareAttackersTests : InteractionControllerTester
    {
        #region Utilities

        private new DeclareAttackersResult Result
        {
            get { return (DeclareAttackersResult)base.Result; }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_BeginDeclareAttackers_returns_an_empty_result_if_passing()
        {
            m_mockAbility.Expect_CanPlay();
            m_mockery.ReplayAll();

            var context = new DeclareAttackersContext(new[] { m_card });
            InteractionController.BeginInteraction(new DeclareAttackersChoice(EmptyPlayer, context));
            {
                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(1, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Assert.AreEqual("Pass", Model.Interaction.UserChoiceInteraction.Choices[0].Text);
                Model.Interaction.SelectChoice(Model.Interaction.UserChoiceInteraction.Choices.First());
            }
            Assert.IsTrue(IsCompleted);
            Assert.IsTrue(Result.IsEmpty);
        }

        [Test]
        public void Test_BeginDeclareAttackers_returns_all_the_selected_attackers()
        {
            List<Card> cards = new List<Card>();
            for (int i = 0; i < 4; i++)
            {
                cards.Add(CreateCard(m_playerA));
            }

            var context = new DeclareAttackersContext(cards.Take(3));
            InteractionController.BeginInteraction(new DeclareAttackersChoice(EmptyPlayer, context));
            {
                for (int i = 0; i < 3; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    Assert.AreEqual(InteractionType.Attack, cardViewModel.InteractionType);
                }

                for (int i = 3; i < 4; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    Assert.AreEqual(InteractionType.None, cardViewModel.InteractionType);
                }

                for (int i = 0; i < 2; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    cardViewModel.Choose();

                    Assert.AreEqual(InteractionType.None, cardViewModel.InteractionType);
                }

                Assert.IsFalse(InteractionController.IsCompleted);

                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(2, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Assert.AreEqual("Continue", Model.Interaction.UserChoiceInteraction.Choices[0].Text);
                Model.Interaction.SelectChoice(Model.Interaction.UserChoiceInteraction.Choices.First());
            }
            Assert.IsTrue(IsCompleted);
            Assert.Collections.AreEquivalent(cards.Take(2), Result.GetAttackers(m_game));
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

            var context = new DeclareAttackersContext(cards);
            InteractionController.BeginInteraction(new DeclareAttackersChoice(EmptyPlayer, context));
            {
                for (int i = 0; i < 1; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    cardViewModel.Choose();
                }

                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(2, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Assert.AreEqual("Cancel", Model.Interaction.UserChoiceInteraction.Choices[1].Text);
                Model.Interaction.SelectChoice(Model.Interaction.UserChoiceInteraction.Choices[1]);

                for (int i = 0; i < 1; i++)
                {
                    CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(cards[i]);
                    Assert.AreEqual(InteractionType.Attack, cardViewModel.InteractionType);
                }

                Assert.IsFalse(IsCompleted);
            }
        }

        #endregion
    }
}
