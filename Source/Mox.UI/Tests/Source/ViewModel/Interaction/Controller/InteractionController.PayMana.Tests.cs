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
    public class InteractionControllerPayManaTests : InteractionControllerTester
    {
        #region Tests

        [Test]
        public void Test_BeginPayMana_returns_null_if_passing()
        {
            ManaCost cost = new ManaCost(3, ManaSymbol.R);

            IInteraction<Action> interaction = InteractionController.BeginPayMana(cost);
            {
                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual("Pay 3R", Model.Interaction.UserChoiceInteraction.Text);
                Assert.AreEqual(1, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Assert.AreEqual("Cancel", Model.Interaction.UserChoiceInteraction.Choices.First().Text);
                Model.Interaction.SelectChoiceCommand.Execute(Model.Interaction.UserChoiceInteraction.Choices.First());
            }
            Assert.IsTrue(interaction.IsCompleted);
            Assert.IsNull(interaction.Result);
        }

        [Test]
        public void Test_cannot_play_a_non_mana_ability_during_mana_payment()
        {
            m_mockAbility.Expect_CanPlay();
            m_mockery.ReplayAll();

            ManaCost cost = new ManaCost(3, ManaSymbol.R);

            IInteraction<Action> interaction = InteractionController.BeginPayMana(cost);
            {
                CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(m_card);
                Assert.IsFalse(cardViewModel.CanBeChosen);
                cardViewModel.Choose();
            }
            Assert.IsFalse(interaction.IsCompleted);
        }

        [Test]
        public void Test_BeginPayMana_returns_PlayAbility_if_playing_mana_ability()
        {
            m_mockAbility.MockedIsManaAbility = true;
            m_mockAbility.Expect_CanPlay().Repeat.Any();
            m_mockery.ReplayAll();

            ManaCost cost = new ManaCost(3, ManaSymbol.R);

            IInteraction<Action> interaction = InteractionController.BeginPayMana(cost);
            {
                CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(m_card);
                Assert.IsTrue(cardViewModel.CanBeChosen);
                cardViewModel.Choose();
            }
            Assert.IsTrue(interaction.IsCompleted);
            Assert.IsNotNull(interaction.Result);
            Assert.IsInstanceOf<PlayAbility>(interaction.Result);

            PlayAbility ability = (PlayAbility)interaction.Result;
            Assert.AreEqual(m_mockAbility, ability.Ability.Resolve(m_game));
        }

        [Test]
        public void Test_BeginPayMana_returns_PayMana_if_the_player_chooses_a_mana_token()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.G);

            ManaPoolViewModel manaPool = Model.MainPlayer.ManaPool;

            manaPool.Mana[Color.None] = 2;
            manaPool.Mana[Color.Red] = 2;
            manaPool.Mana[Color.Blue] = 2;

            IInteraction<Action> interaction = InteractionController.BeginPayMana(cost);
            {
                Assert.IsFalse(manaPool.CanPay[Color.None]);
                Assert.IsTrue(manaPool.CanPay[Color.Red]);
                Assert.IsFalse(manaPool.CanPay[Color.Blue]);
                Assert.IsFalse(manaPool.CanPay[Color.Green]);

                Model.MainPlayer.PayMana.Execute(Color.Red);
            }
            Assert.IsTrue(interaction.IsCompleted);
            Assert.IsNotNull(interaction.Result);
            Assert.IsInstanceOf<PayManaAction>(interaction.Result);

            PayManaAction action = (PayManaAction)interaction.Result;
            Assert.Collections.AreEqual(new[] { Color.Red }, action.Payment.Payments);
        }

        [Test]
        public void Test_all_mana_can_be_played_as_colorless()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.G);

            ManaPoolViewModel manaPool = Model.MainPlayer.ManaPool;

            manaPool.Mana[Color.None] = 2;
            manaPool.Mana[Color.Red] = 2;
            manaPool.Mana[Color.Blue] = 2;

            IInteraction<Action> interaction = InteractionController.BeginPayMana(cost);
            {
                Assert.IsTrue(manaPool.CanPay[Color.None]);
                Assert.IsTrue(manaPool.CanPay[Color.Red]);
                Assert.IsTrue(manaPool.CanPay[Color.Blue]);
                Assert.IsFalse(manaPool.CanPay[Color.Green]);

                Model.MainPlayer.PayMana.Execute(Color.Blue);
            }
            Assert.IsTrue(interaction.IsCompleted);
            Assert.IsNotNull(interaction.Result);
            Assert.IsInstanceOf<PayManaAction>(interaction.Result);

            PayManaAction action = (PayManaAction)interaction.Result;
            Assert.Collections.AreEqual(new[] { Color.Blue }, action.Payment.Payments);
        }

        #endregion
    }
}
