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
    public class InteractionControllerPayManaTests : InteractionControllerTester
    {
        #region Tests

        [Test]
        public void Test_BeginPayMana_returns_null_if_passing()
        {
            ManaCost cost = new ManaCost(3, ManaSymbol.R);

            InteractionController.BeginInteraction(new PayManaChoice(EmptyPlayer, cost));
            {
                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual("Pay 3R", Model.Interaction.UserChoiceInteraction.Text);
                Assert.AreEqual(1, Model.Interaction.UserChoiceInteraction.Choices.Count);
                Assert.AreEqual("Cancel", Model.Interaction.UserChoiceInteraction.Choices.First().Text);
                Model.Interaction.SelectChoice(Model.Interaction.UserChoiceInteraction.Choices.First());
            }
            Assert.IsTrue(IsCompleted);
            Assert.IsNull(Result);
        }

        [Test]
        public void Test_cannot_play_a_non_mana_ability_during_mana_payment()
        {
            m_mockAbility.Expect_CanPlay();
            m_mockery.ReplayAll();

            ManaCost cost = new ManaCost(3, ManaSymbol.R);

            InteractionController.BeginInteraction(new PayManaChoice(EmptyPlayer, cost));
            {
                CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(m_card);
                Assert.AreEqual(InteractionType.None, cardViewModel.InteractionType);
                cardViewModel.Choose();
            }
            Assert.IsFalse(IsCompleted);
        }

        [Test]
        public void Test_BeginPayMana_returns_PlayAbility_if_playing_mana_ability()
        {
            m_mockAbility.MockedIsManaAbility = true;
            m_mockAbility.Expect_CanPlay().Repeat.Any();
            m_mockery.ReplayAll();

            ManaCost cost = new ManaCost(3, ManaSymbol.R);

            InteractionController.BeginInteraction(new PayManaChoice(EmptyPlayer, cost));
            {
                CardViewModel cardViewModel = m_synchronizer.GetCardViewModel(m_card);
                Assert.AreEqual(InteractionType.Play, cardViewModel.InteractionType);
                cardViewModel.Choose();
            }
            Assert.IsTrue(IsCompleted);
            Assert.IsNotNull(Result);
            Assert.IsInstanceOf<PlayAbility>(Result);

            PlayAbility ability = (PlayAbility)Result;
            Assert.AreEqual(m_mockAbility, ability.Ability.Resolve(m_game));
        }

        [Test]
        public void Test_BeginPayMana_returns_PayMana_if_the_player_chooses_a_mana_token()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.G);

            ManaPoolViewModel manaPool = Model.MainPlayer.ManaPool;

            manaPool.Colorless.Amount = 2;
            manaPool.Red.Amount = 2;
            manaPool.Blue.Amount = 2;

            InteractionController.BeginInteraction(new PayManaChoice(EmptyPlayer, cost));
            {
                Assert.IsFalse(manaPool.Colorless.CanPay);
                Assert.IsTrue(manaPool.Red.CanPay);
                Assert.IsFalse(manaPool.Blue.CanPay);
                Assert.IsFalse(manaPool.Green.CanPay);

                manaPool.PayMana(manaPool.Red);
            }
            Assert.IsTrue(IsCompleted);
            Assert.IsNotNull(Result);
            Assert.IsInstanceOf<PayManaAction>(Result);

            PayManaAction action = (PayManaAction)Result;
            Assert.AreEqual(2, action.Payment.Atoms.Length);
            Assert.AreEqual(new ManaPaymentAmount(), action.Payment.Generic);
            Assert.AreEqual(new ManaPaymentAmount { Red = 1 }, action.Payment.Atoms[0]);
            Assert.AreEqual(new ManaPaymentAmount(), action.Payment.Atoms[1]);
        }

        [Test]
        public void Test_all_mana_can_be_played_as_colorless()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.G);

            ManaPoolViewModel manaPool = Model.MainPlayer.ManaPool;

            manaPool.Colorless.Amount = 2;
            manaPool.Red.Amount = 2;
            manaPool.Blue.Amount = 2;

            InteractionController.BeginInteraction(new PayManaChoice(EmptyPlayer, cost));
            {
                Assert.IsTrue(manaPool.Colorless.CanPay);
                Assert.IsTrue(manaPool.Red.CanPay);
                Assert.IsTrue(manaPool.Blue.CanPay);
                Assert.IsFalse(manaPool.Green.CanPay);

                manaPool.PayMana(manaPool.Blue);
            }
            Assert.IsTrue(IsCompleted);
            Assert.IsNotNull(Result);
            Assert.IsInstanceOf<PayManaAction>(Result);

            PayManaAction action = (PayManaAction)Result;
            Assert.AreEqual(2, action.Payment.Atoms.Length);
            Assert.AreEqual(new ManaPaymentAmount { Blue = 1 }, action.Payment.Generic);
            Assert.AreEqual(new ManaPaymentAmount(), action.Payment.Atoms[0]);
            Assert.AreEqual(new ManaPaymentAmount(), action.Payment.Atoms[1]);
        }

        #endregion
    }
}
