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

namespace Mox.Abilities
{
    [TestFixture]
    public class TapSelfCostTests : CostTestsBase
    {
        #region Variables

        private TapSelfCost m_cost;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_card.Type = Type.Creature; // For correctly testing summoning sickness
            m_cost = new TapSelfCost(true);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsTrue(m_cost.DoTap);
        }

        [Test]
        public void Test_Cannot_execute_if_the_card_is_already_in_the_wanted_state()
        {
            AbilityEvaluationContext normalContext = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal);
            AbilityEvaluationContext userContext = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal) { UserMode = true };

            m_card.Tapped = false;
            Assert.IsTrue(m_cost.CanExecute(m_ability, normalContext));
            Assert.IsTrue(m_cost.CanExecute(m_ability, userContext));

            m_card.Tapped = true;
            Assert.IsFalse(m_cost.CanExecute(m_ability, normalContext));
            Assert.IsFalse(m_cost.CanExecute(m_ability, userContext));

            m_cost = new TapSelfCost(false);

            m_card.Tapped = false;
            Assert.IsFalse(m_cost.CanExecute(m_ability, normalContext));
            Assert.IsFalse(m_cost.CanExecute(m_ability, userContext));

            m_card.Tapped = true;
            Assert.IsTrue(m_cost.CanExecute(m_ability, normalContext));
            Assert.IsTrue(m_cost.CanExecute(m_ability, userContext));
        }

        [Test]
        public void Test_Cannot_tap_or_untap_if_the_card_has_summoning_sickness()
        {
            AbilityEvaluationContext normalContext = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal);
            AbilityEvaluationContext userContext = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal) { UserMode = true };

            m_cost = new TapSelfCost(true);
            m_card.Tapped = false;

            m_card.HasSummoningSickness = true;
            Assert.IsFalse(m_cost.CanExecute(m_ability, normalContext));
            Assert.IsFalse(m_cost.CanExecute(m_ability, userContext));

            m_card.HasSummoningSickness = false;
            Assert.IsTrue(m_cost.CanExecute(m_ability, normalContext));
            Assert.IsTrue(m_cost.CanExecute(m_ability, userContext));

            m_cost = new TapSelfCost(false);
            m_card.Tapped = true;

            m_card.HasSummoningSickness = true;
            Assert.IsFalse(m_cost.CanExecute(m_ability, normalContext));
            Assert.IsFalse(m_cost.CanExecute(m_ability, userContext));

            m_card.HasSummoningSickness = false;
            Assert.IsTrue(m_cost.CanExecute(m_ability, normalContext));
            Assert.IsTrue(m_cost.CanExecute(m_ability, userContext));
        }

        [Test]
        public void Test_Execute_sets_the_card_in_the_wanted_state()
        {
            m_card.Tapped = false;
            Execute(m_cost, true);
            Assert.IsTrue(m_card.Tapped);

            m_cost = new TapSelfCost(false);

            m_card.Tapped = true;
            Execute(m_cost, true);
            Assert.IsFalse(m_card.Tapped);
        }

        #endregion
    }
}
