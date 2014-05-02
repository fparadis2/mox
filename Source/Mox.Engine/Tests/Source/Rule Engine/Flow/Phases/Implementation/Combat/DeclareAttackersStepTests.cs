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
using System.Collections.Generic;
using NUnit.Framework;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class DeclareAttackersStepTests : BaseStepTests<DeclareAttackersStep>
    {
        #region Variables
        
        private readonly List<Card> m_attackingCreatures = new List<Card>();

        private Cost m_cost;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_cost = m_mockery.StrictMock<Cost>();

            m_sequencerTester.MockPlayerChoices(m_playerA);

            m_step = new DeclareAttackersStep();

            m_attackingCreatures.Clear();
            m_attackingCreatures.AddRange(new[] { CreateCard(m_playerA), CreateCard(m_playerA) });
            m_attackingCreatures.ForEach(c =>
            {
                c.Type = Type.Creature;
                c.Zone = m_game.Zones.Battlefield;
                c.Toughness = 1;
                c.HasSummoningSickness = false;
            });
        }

        #endregion

        #region Utilities

        protected static void Expect_Play_Attack_Ability(MockAbility ability, Player player, IEnumerable<Cost> costs)
        {
            ability.Expect_Play_and_execute_costs(player, costs);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(Steps.DeclareAttackers, m_step.Type);
        }

        [Test]
        public void Test_Step_asks_the_active_players_for_attackers()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Step_doesnt_ask_the_active_player_to_declare_attackers_if_there_are_no_legal_attacker()
        {
            m_attackingCreatures.ForEach(c =>
            {
                c.Type = Type.Enchantment;
            });

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Attacking_creatures_get_tapped()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            }

            RunStep(m_playerA);

            Assert.IsTrue(m_attackingCreatures[0].Tapped);
            Assert.IsFalse(m_attackingCreatures[1].Tapped);
        }

        [Test]
        public void Test_Attacking_creatures_with_Vigilance_dont_get_tapped()
        {
            m_game.CreateAbility<VigilanceAbility>(m_attackingCreatures[0]);

            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            }

            RunStep(m_playerA);

            Assert.IsFalse(m_attackingCreatures[0].Tapped);
            Assert.IsFalse(m_attackingCreatures[1].Tapped);
        }

        [Test]
        public void Test_Attack_abilities_are_played_and_costs_are_paid()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0], m_attackingCreatures[1]);

            MockAbility mockAbility1 = CreateMockAbility(m_attackingCreatures[0], AbilityType.Attack);
            MockAbility mockAbility2 = CreateMockAbility(m_attackingCreatures[1], AbilityType.Attack);

            using (m_mockery.Ordered())
            {
                mockAbility1.Expect_CanPlay();
                mockAbility2.Expect_CanPlay();

                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);

                using (m_mockery.Unordered())
                {
                    mockAbility1.Expect_CanPlay();
                    Expect_Play_Attack_Ability(mockAbility1, m_playerA, new[] { m_mockery.StrictMock<Cost>() });

                    mockAbility2.Expect_CanPlay();
                    Expect_Play_Attack_Ability(mockAbility2, m_playerA, new[] { m_mockery.StrictMock<Cost>() });
                }

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_The_whole_attack_is_reverted_if_costs_cannot_be_paid()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            MockAbility mockAbility1 = CreateMockAbility(m_attackingCreatures[0], AbilityType.Attack);

            Assert.AreNotEqual(42, m_playerA.Life);

            using (m_mockery.Ordered())
            {
                mockAbility1.Expect_CanPlay();

                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);

                mockAbility1.Expect_CanPlay();
                mockAbility1.Expect_Play(new[] { m_cost });

                m_cost.Expect_Execute(m_playerA, false, () =>
                {
                    m_playerA.Life = 42;
                    Assert.AreEqual(42, m_playerA.Life);
                });
                
                mockAbility1.Expect_CanPlay();
                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, DeclareAttackersResult.Empty);

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            }

            RunStep(m_playerA);

            Assert.IsFalse(m_attackingCreatures[0].Tapped);
            Assert.AreNotEqual(42, m_playerA.Life);
            Assert.That(m_game.CombatData.Attackers.IsEmpty);
        }

        [Test]
        public void Test_Step_will_ask_for_attackers_until_the_result_is_legal()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, new DeclareAttackersResult(m_card));
                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, new DeclareAttackersResult(m_card, m_attackingCreatures[0]));

                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, DeclareAttackersResult.Empty);
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Attacking_creatures_get_added_in_the_combat_data()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);

                // make sure they are already there when giving priority
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null).Callback(() => Assert.IsFalse(m_game.CombatData.Attackers.IsEmpty));
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Attackers_are_not_added_in_combat_data_if_they_are_not_controlled_by_player_anymore()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            MockAbility mockAbility = CreateMockAbility(m_attackingCreatures[0], AbilityType.Attack);

            using (m_mockery.Ordered())
            {
                mockAbility.Expect_CanPlay();

                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);

                mockAbility.Expect_CanPlay();
                mockAbility.Expect_Play(new[] { m_cost });

                m_cost.Expect_Execute(m_playerA, false, () => m_attackingCreatures[0].Controller = m_playerB);

                mockAbility.Expect_CanPlay();
                m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, DeclareAttackersResult.Empty);

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            }

            RunStep(m_playerA);

            Assert.That(m_game.CombatData.Attackers.IsEmpty);
        }

        #endregion
    }
}
