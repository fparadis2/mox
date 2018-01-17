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

using Mox.Abilities;

namespace Mox.Flow.Parts
{
#warning todo spell_v2
    /*[TestFixture]
    public class HandleTriggeredAbilitiesTests : PartTestBase
    {
        #region Variables

        private HandleTriggeredAbilities m_part;
        private MockTriggeredAbility m_triggeredAbility;

        private Cost m_cost;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_game.State.ActivePlayer = m_playerA;

            m_triggeredAbility = CreateMockTriggeredAbility(m_card);

            m_part = new HandleTriggeredAbilities(m_playerA);

            m_cost = m_mockery.StrictMock<Cost>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new HandleTriggeredAbilities(null));
        }

        [Test]
        public void Test_Empties_the_TriggeredAbilities_queue()
        {
            m_triggeredAbility.Trigger(null);
            Assert.AreEqual(1, m_game.GlobalData.TriggeredAbilities.Count());

            m_triggeredAbility.Expect_Play();

            m_sequencerTester.Run(m_part);

            Assert.Collections.IsEmpty(m_game.GlobalData.TriggeredAbilities);
        }

        [Test]
        public void Test_If_theres_triggered_abilities_state_based_effects_are_checked_again()
        {
            m_triggeredAbility.Trigger(null);
            Assert.AreEqual(1, m_game.GlobalData.TriggeredAbilities.Count());

            Assert.IsInstanceOf<CheckStateBasedActions>(Execute(m_part));
        }

        [Test]
        public void Test_If_theres_no_triggered_abilities_state_based_effects_dont_need_to_be_checked_again()
        {
            Assert.IsNull(Execute(m_part));
        }

        [Test]
        public void Test_Triggered_abilities_are_handled()
        {
            m_triggeredAbility.Trigger(null);

            m_triggeredAbility.Expect_Play_and_execute_costs(m_playerA, new[] { m_cost });

            m_sequencerTester.Run(m_part);
        }

        [Test]
        public void Test_Triggered_abilities_are_handled_in_APNAP_order()
        {
            Card cardA = CreateCard(m_playerA);
            Card cardB = CreateCard(m_playerB);

            MockTriggeredAbility ability1 = CreateMockTriggeredAbility(cardA);
            MockTriggeredAbility ability2 = CreateMockTriggeredAbility(cardA);
            MockTriggeredAbility ability3 = CreateMockTriggeredAbility(cardB);
            MockTriggeredAbility ability4 = CreateMockTriggeredAbility(cardB);

            ability4.Trigger(null);
            ability2.Trigger(null);
            ability3.Trigger(null);
            ability1.Trigger(null);

            using (m_mockery.Ordered())
            {
                using (m_mockery.Unordered())
                {
                    ability1.Expect_Play();
                    ability2.Expect_Play();
                }

                using (m_mockery.Unordered())
                {
                    ability3.Expect_Play();
                    ability4.Expect_Play();
                }
            }

            m_sequencerTester.Run(m_part);
        }

        #endregion
    }*/
}
