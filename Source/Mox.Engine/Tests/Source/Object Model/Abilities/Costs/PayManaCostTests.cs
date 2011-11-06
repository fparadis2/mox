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
using Mox.Flow;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

using Is = Rhino.Mocks.Constraints.Is;

namespace Mox
{
    [TestFixture]
    public class PayManaCostTests : BaseGameTests
    {
        #region Variables

        private NewSequencerTester m_sequencer;
        private PayManaCost m_cost;
        private Action m_mockAction;
        private ManaPayment m_payment;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_mockAction = m_mockery.StrictMock<Action>();

            m_sequencer = new NewSequencerTester(m_mockery, m_game);
            m_sequencer.MockPlayerChoices(m_playerA);

            m_cost = new PayManaCost(new ManaCost(2, ManaSymbol.R, ManaSymbol.W));

            m_payment = new ManaPayment();
        }

        #endregion

        #region Utilities

        private class EvaluateCost : PlayerPart
        {
            #region Variables

            private readonly DelayedCost m_cost;

            #endregion

            #region Constructor

            public EvaluateCost(DelayedCost cost, Player player)
                : base(player)
            {
                Throw.IfNull(cost, "cost");

                m_cost = cost;
            }

            #endregion

            #region Overrides of NewPart

            public override NewPart Execute(Context context)
            {
                m_cost.Execute(context, GetPlayer(context));
                return null;
            }

            #endregion
        }

        private void Execute(Player player, bool expectedResult)
        {
            m_sequencer.Run(new EvaluateCost(m_cost, player));
            Assert.AreEqual(expectedResult, m_sequencer.Sequencer.PopArgument<bool>(DelayedCost.ArgumentToken));
        }

        private static IMethodOptions<bool> Expect_CanExecuteAction(Action mockAction, Player player, ExecutionEvaluationContext expectedContext)
        {
            return Expect.Call(mockAction.CanExecute(player, expectedContext));
        }

        private static void Expect_ExecuteAction(Action mockAction, Player player)
        {
            mockAction.Execute(null, player);
            LastCall.IgnoreArguments().Constraints(Is.NotNull(), Is.Equal(player));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(new ManaCost(2, ManaSymbol.R, ManaSymbol.W), m_cost.ManaCost);
        }

        [Test]
        public void Test_Can_always_be_paid()
        {
            ExecutionEvaluationContext context = new ExecutionEvaluationContext();

            context.UserMode = false;
            Assert.IsTrue(m_cost.CanExecute(m_game, context));

            // For now...
            context.UserMode = true;
            Assert.IsTrue(m_cost.CanExecute(m_game, context));
        }

        [Test]
        public void Test_If_the_player_passes_the_cost_is_cancelled()
        {
            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, null);
            Execute(m_playerA, false);
        }

        [Test]
        public void Test_Nothing_to_do_if_the_cost_is_empty()
        {
            m_cost = new PayManaCost(new ManaCost(0));
            Execute(m_playerA, true);
        }

        [Test]
        public void Test_Nothing_to_do_if_the_cost_is_null()
        {
            m_cost = new PayManaCost(null);
            Execute(m_playerA, true);
        }

        [Test]
        public void Test_The_player_can_play_mana_actions_during_mana_payment_cost()
        {
            ExecutionEvaluationContext manaPaymentContext = new ExecutionEvaluationContext { Type = EvaluationContextType.ManaPayment };

            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, m_mockAction);
            Expect_CanExecuteAction(m_mockAction, m_playerA, manaPaymentContext).Return(false);

            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, m_mockAction);
            Expect_CanExecuteAction(m_mockAction, m_playerA, manaPaymentContext).Return(true);
            Expect_ExecuteAction(m_mockAction, m_playerA);

            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, null);

            Execute(m_playerA, false);
        }

        [Test]
        public void Test_The_player_can_pay_for_the_whole_mana()
        {
            m_playerA.ManaPool[Color.Red] = 1;
            m_playerA.ManaPool[Color.Blue] = 1;
            m_playerA.ManaPool[Color.White] = 1;
            m_playerA.ManaPool[Color.None] = 1;

            m_payment.Pay(Color.Red);
            m_payment.Pay(Color.Blue);
            m_payment.Pay(Color.White);
            m_payment.Pay(Color.None);

            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, new PayManaAction(m_payment));

            Execute(m_playerA, true);

            Assert.AreEqual(0, m_playerA.ManaPool[Color.Red]);
            Assert.AreEqual(0, m_playerA.ManaPool[Color.Blue]);
            Assert.AreEqual(0, m_playerA.ManaPool[Color.White]);
            Assert.AreEqual(0, m_playerA.ManaPool[Color.None]);
        }

        [Test]
        public void Test_The_player_can_pay_more_than_needed_the_rest_is_not_consumed()
        {
            m_playerA.ManaPool[Color.Red] = 2;
            m_playerA.ManaPool[Color.Blue] = 1;
            m_playerA.ManaPool[Color.White] = 1;
            m_playerA.ManaPool[Color.None] = 1;

            m_payment.Pay(Color.Red);
            m_payment.Pay(Color.Blue);
            m_payment.Pay(Color.White);
            m_payment.Pay(Color.None);
            m_payment.Pay(Color.Red);

            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, new PayManaAction(m_payment));

            Execute(m_playerA, true);

            Assert.AreEqual(1, m_playerA.ManaPool[Color.Red]);
            Assert.AreEqual(0, m_playerA.ManaPool[Color.Blue]);
            Assert.AreEqual(0, m_playerA.ManaPool[Color.White]);
            Assert.AreEqual(0, m_playerA.ManaPool[Color.None]);
        }

        [Test]
        public void Test_Can_pay_in_multiple_payments()
        {
            // Repeating here for clarity
            m_cost = new PayManaCost(new ManaCost(2, ManaSymbol.R, ManaSymbol.W));

            m_playerA.ManaPool[Color.Red] = 1;
            m_playerA.ManaPool[Color.Blue] = 1;
            m_playerA.ManaPool[Color.White] = 1;
            m_playerA.ManaPool[Color.None] = 1;

            ManaPayment payment1 = new ManaPayment();
            ManaPayment payment2 = new ManaPayment();

            payment1.Pay(Color.Red);
            payment1.Pay(Color.Blue);
            payment2.Pay(Color.White);
            payment2.Pay(Color.None);

            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, new PayManaAction(payment1));
            m_sequencer.Expect_Player_PayMana(m_playerA, new ManaCost(1, ManaSymbol.W), new PayManaAction(payment2));

            Execute(m_playerA, true);

            Assert.AreEqual(0, m_playerA.ManaPool[Color.Red]);
            Assert.AreEqual(0, m_playerA.ManaPool[Color.Blue]);
            Assert.AreEqual(0, m_playerA.ManaPool[Color.White]);
            Assert.AreEqual(0, m_playerA.ManaPool[Color.None]);
        }

        #endregion
    }
}
