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

namespace Mox
{
    [TestFixture]
    public class TapCostTests : CostTestsBase
    {
        #region Variables

        private TapCost m_cost;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_card.Type = Type.Creature; // For correctly testing summoning sickness
            m_cost = new TapCost(m_card, true);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_card, m_cost.Card.Resolve(m_game));
            Assert.IsTrue(m_cost.DoTap);
        }

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new TapCost(null, true); });
        }

        [Test]
        public void Test_Cannot_execute_if_the_card_is_already_in_the_wanted_state()
        {
            ExecutionEvaluationContext normalContext = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal);
            ExecutionEvaluationContext userContext = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal) { UserMode = true };

            m_card.Tapped = false;
            Assert.IsTrue(m_cost.CanExecute(m_game, normalContext));
            Assert.IsTrue(m_cost.CanExecute(m_game, userContext));

            m_card.Tapped = true;
            Assert.IsFalse(m_cost.CanExecute(m_game, normalContext));
            Assert.IsFalse(m_cost.CanExecute(m_game, userContext));

            m_cost = new TapCost(m_card, false);

            m_card.Tapped = false;
            Assert.IsFalse(m_cost.CanExecute(m_game, normalContext));
            Assert.IsFalse(m_cost.CanExecute(m_game, userContext));

            m_card.Tapped = true;
            Assert.IsTrue(m_cost.CanExecute(m_game, normalContext));
            Assert.IsTrue(m_cost.CanExecute(m_game, userContext));
        }

        [Test]
        public void Test_Cannot_tap_or_untap_if_the_card_has_summoning_sickness()
        {
            ExecutionEvaluationContext normalContext = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal);
            ExecutionEvaluationContext userContext = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal) { UserMode = true };

            m_cost = new TapCost(m_card, true);
            m_card.Tapped = false;

            m_card.HasSummoningSickness = true;
            Assert.IsFalse(m_cost.CanExecute(m_game, normalContext));
            Assert.IsFalse(m_cost.CanExecute(m_game, userContext));

            m_card.HasSummoningSickness = false;
            Assert.IsTrue(m_cost.CanExecute(m_game, normalContext));
            Assert.IsTrue(m_cost.CanExecute(m_game, userContext));

            m_cost = new TapCost(m_card, false);
            m_card.Tapped = true;

            m_card.HasSummoningSickness = true;
            Assert.IsFalse(m_cost.CanExecute(m_game, normalContext));
            Assert.IsFalse(m_cost.CanExecute(m_game, userContext));

            m_card.HasSummoningSickness = false;
            Assert.IsTrue(m_cost.CanExecute(m_game, normalContext));
            Assert.IsTrue(m_cost.CanExecute(m_game, userContext));
        }

        [Test]
        public void Test_Execute_sets_the_card_in_the_wanted_state()
        {
            m_card.Tapped = false;
            Execute(m_cost, m_playerA, true);
            Assert.IsTrue(m_card.Tapped);

            m_cost = new TapCost(m_card, false);

            m_card.Tapped = true;
            Execute(m_cost, m_playerA, true);
            Assert.IsFalse(m_card.Tapped);
        }

        #endregion
    }
}
