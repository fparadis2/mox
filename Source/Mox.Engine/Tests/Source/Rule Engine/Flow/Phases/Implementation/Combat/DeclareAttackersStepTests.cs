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

using Mox.Abilities;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class DeclareAttackersStepTests : BaseStepTests<DeclareAttackersStep>
    {
        #region Variables
        
        private readonly List<Card> m_attackingCreatures = new List<Card>();

        private MockCost m_cost;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_cost = new MockCost();

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

        private MockCost CreateAttackAbilityWithCost(Card card)
        {
            MockCost cost = new MockCost();

            SpellDefinition spellDefinition = CreateSpellDefinition(card);
            spellDefinition.AddCost(cost);

            m_game.CreateAbility<MockAttackAbility>(card, spellDefinition);

            return cost;
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

            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Step_doesnt_ask_the_active_player_to_declare_attackers_if_there_are_no_legal_attacker()
        {
            m_attackingCreatures.ForEach(c =>
            {
                c.Type = Type.Enchantment;
            });

            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Attacking_creatures_get_tapped()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

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

            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            RunStep(m_playerA);

            Assert.IsFalse(m_attackingCreatures[0].Tapped);
            Assert.IsFalse(m_attackingCreatures[1].Tapped);
        }

        [Test]
        public void Test_Attack_abilities_are_played_and_costs_are_paid()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0], m_attackingCreatures[1]);

            var cost1 = CreateAttackAbilityWithCost(m_attackingCreatures[0]);
            var cost2 = CreateAttackAbilityWithCost(m_attackingCreatures[1]);

            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            RunStep(m_playerA);

            Assert.That(cost1.Executed);
            Assert.That(cost2.Executed);
        }

        [Test]
        public void Test_The_whole_attack_is_reverted_if_costs_cannot_be_paid()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            var cost = CreateAttackAbilityWithCost(m_attackingCreatures[0]);

            cost.ExecuteCallback = () =>
            {
                m_playerA.Life = 42;
                Assert.AreEqual(42, m_playerA.Life);
                return false;
            };

            Assert.AreNotEqual(42, m_playerA.Life);

            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);
            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, DeclareAttackersResult.Empty);
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            RunStep(m_playerA);

            Assert.That(cost.Executed);
            Assert.IsFalse(m_attackingCreatures[0].Tapped);
            Assert.AreNotEqual(42, m_playerA.Life);
            Assert.That(m_game.CombatData.Attackers.IsEmpty);
        }

        [Test]
        public void Test_Step_will_ask_for_attackers_until_the_result_is_legal()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, new DeclareAttackersResult(m_card));
            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, new DeclareAttackersResult(m_card, m_attackingCreatures[0]));

            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, DeclareAttackersResult.Empty);
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Attacking_creatures_get_added_in_the_combat_data()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);

            // make sure they are already there when giving priority
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null).Callback(() => Assert.IsFalse(m_game.CombatData.Attackers.IsEmpty));

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Attackers_are_not_added_in_combat_data_if_they_are_not_controlled_by_player_anymore()
        {
            DeclareAttackersContext attackInfo = new DeclareAttackersContext(m_attackingCreatures);

            DeclareAttackersResult result = new DeclareAttackersResult(m_attackingCreatures[0]);

            var cost = CreateAttackAbilityWithCost(m_attackingCreatures[0]);

            cost.ExecuteCallback = () =>
            {
                m_attackingCreatures[0].Controller = m_playerB;
                return true;
            };

            m_sequencerTester.Expect_Player_DeclareAttackers(m_playerA, attackInfo, result);
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            RunStep(m_playerA);

            Assert.That(m_game.CombatData.Attackers.IsEmpty);
        }

        #endregion

        #region Mock Types

        private class MockAttackAbility : SpellAbility
        {
            public override AbilityType AbilityType => AbilityType.Attack;
        }

        #endregion
    }
}
