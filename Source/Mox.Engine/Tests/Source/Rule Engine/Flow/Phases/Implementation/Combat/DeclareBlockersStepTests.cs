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
using System.Linq;
using NUnit.Framework;

using Mox.Abilities;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class DeclareBlockersStepTests : BaseStepTests<DeclareBlockersStep>
    {
        #region Variables

        private readonly List<Card> m_attackingCreatures = new List<Card>();
        private readonly List<Card> m_blockingCreatures = new List<Card>();

        private Cost m_cost;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_cost = m_mockery.StrictMock<Cost>();

            m_step = new DeclareBlockersStep();

            m_sequencerTester.MockAllPlayersChoices();

            m_attackingCreatures.Clear();
            m_attackingCreatures.AddRange(new[] { CreateCard(m_playerA), CreateCard(m_playerA) });
            m_attackingCreatures.ForEach(c =>
            {
                c.Type = Type.Creature;
                c.Zone = m_game.Zones.Battlefield;
                c.Toughness = 1;
            });

            m_game.CombatData.SetAttackTarget(m_playerB);
            m_game.CombatData.Attackers = new DeclareAttackersResult(m_attackingCreatures.ToArray());

            m_blockingCreatures.Clear();
            m_blockingCreatures.AddRange(new[] { CreateCard(m_playerB), CreateCard(m_playerB) });
            m_blockingCreatures.ForEach(c =>
            {
                c.Type = Type.Creature;
                c.Zone = m_game.Zones.Battlefield;
                c.Toughness = 1;
            });
        }

        #endregion

        #region Utilities

        private static DeclareBlockersResult CreateResult(Card attackingCreature, params Card[] blockingCreatures)
        {
            return new DeclareBlockersResult(blockingCreatures.Select(c => new DeclareBlockersResult.BlockingCreature(c, attackingCreature)).ToArray());
        }

        private void Expect_All_Players_pass(Player startingPlayer)
        {
            foreach (Player player in Player.Enumerate(startingPlayer, false))
            {
                m_sequencerTester.Expect_Player_GivePriority(player, null);
            }
        }

        private MockCost CreateBlockAbilityWithCost(Card card)
        {
            MockCost cost = new MockCost();

            SpellDefinition spellDefinition = CreateSpellDefinition(card);
            spellDefinition.AddCost(cost);

            m_game.CreateAbility<MockBlockAbility>(card, spellDefinition);

            return cost;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(Steps.DeclareBlockers, m_step.Type);
        }

        [Test]
        public void Test_Phase_is_completely_skipped_if_no_attackers_declared()
        {
            m_game.CombatData.Attackers = null;
            RunStep(m_playerA);
        }

        [Test]
        public void Test_Step_asks_the_defending_player_for_blockers()
        {
            DeclareBlockersContext blockInfo = new DeclareBlockersContext(m_attackingCreatures, m_blockingCreatures);

            DeclareBlockersResult result = CreateResult(m_attackingCreatures[0], m_blockingCreatures[0]);

            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, result);
            Expect_All_Players_pass(m_playerA);

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Step_doesnt_ask_the_active_player_to_declare_blockers_if_there_are_no_legal_blocker()
        {
            m_blockingCreatures.ForEach(c =>
            {
                c.Type = Type.Enchantment;
            });

            Expect_All_Players_pass(m_playerA);

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Blocking_abilities_are_played_and_costs_are_paid()
        {
            DeclareBlockersContext blockInfo = new DeclareBlockersContext(m_attackingCreatures, m_blockingCreatures);

            DeclareBlockersResult result = CreateResult(m_attackingCreatures[0], m_blockingCreatures[0], m_blockingCreatures[1]);

            MockCost cost1 = CreateBlockAbilityWithCost(m_blockingCreatures[0]);
            MockCost cost2 = CreateBlockAbilityWithCost(m_blockingCreatures[1]);

            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, result);
            Expect_All_Players_pass(m_playerA);

            RunStep(m_playerA);

            Assert.That(cost1.Executed);
            Assert.That(cost2.Executed);
        }

        [Test]
        public void Test_The_whole_block_is_reverted_if_costs_cannot_be_paid()
        {
            DeclareBlockersContext blockInfo = new DeclareBlockersContext(m_attackingCreatures, m_blockingCreatures);

            DeclareBlockersResult result = CreateResult(m_attackingCreatures[0], m_blockingCreatures[0]);

            var cost = CreateBlockAbilityWithCost(m_blockingCreatures[0]);

            cost.ExecuteCallback = () =>
            {
                m_playerA.Life = 42;
                Assert.AreEqual(42, m_playerA.Life);
                return false;
            };

            Assert.AreNotEqual(42, m_playerA.Life);

            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, result);
            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, DeclareBlockersResult.Empty);
            Expect_All_Players_pass(m_playerA);

            RunStep(m_playerA);

            Assert.That(cost.Executed);
            Assert.AreNotEqual(42, m_playerA.Life);
        }

        [Test]
        public void Test_Step_will_ask_for_blockers_until_the_result_is_legal()
        {
            DeclareBlockersContext blockInfo = new DeclareBlockersContext(m_attackingCreatures, m_blockingCreatures);

            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, CreateResult(m_card, m_blockingCreatures[0]));
            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, CreateResult(m_attackingCreatures[0], m_card));

            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, DeclareBlockersResult.Empty);

            Expect_All_Players_pass(m_playerA);

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Blocking_creatures_get_added_in_the_combat_data()
        {
            DeclareBlockersContext blockInfo = new DeclareBlockersContext(m_attackingCreatures, m_blockingCreatures);

            DeclareBlockersResult result = CreateResult(m_attackingCreatures[0], m_blockingCreatures[0]);

            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, result);

            Expect_All_Players_pass(m_playerA);

            RunStep(m_playerA);

            Assert.AreEqual(1, m_game.CombatData.Blockers.Blockers.Count);
            Assert.AreEqual(m_blockingCreatures[0].Identifier, m_game.CombatData.Blockers.Blockers[0].BlockingCreatureId);
            Assert.AreEqual(m_attackingCreatures[0].Identifier, m_game.CombatData.Blockers.Blockers[0].BlockedCreatureId);
        }

        [Test]
        public void Test_Blockers_are_not_added_in_combat_data_if_they_are_not_controlled_by_defending_player_anymore()
        {
            DeclareBlockersContext blockInfo = new DeclareBlockersContext(m_attackingCreatures, m_blockingCreatures);

            DeclareBlockersResult result = CreateResult(m_attackingCreatures[0], m_blockingCreatures[0]);

            var cost = CreateBlockAbilityWithCost(m_blockingCreatures[0]);

            cost.ExecuteCallback = () =>
            {
                m_blockingCreatures[0].Controller = m_playerA;
                return true;
            };

            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, result);
            Expect_All_Players_pass(m_playerA);

            RunStep(m_playerA);

            Assert.Collections.IsEmpty(m_game.CombatData.Blockers.Blockers);
        }

        [Test]
        public void Test_Evasion_abilities_are_checked_before_costs_are_paid()
        {
            DeclareBlockersContext blockInfo = new DeclareBlockersContext(m_attackingCreatures, m_blockingCreatures);

            DeclareBlockersResult result = CreateResult(m_attackingCreatures[0], m_blockingCreatures[0]);

            MockEvasionAbility evasionAbility = m_game.CreateAbility<MockEvasionAbility>(m_attackingCreatures[0]);
            evasionAbility.CanBlockResult = true;

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, result);
                Expect_All_Players_pass(m_playerA);
            }

            RunStep(m_playerA);

            Assert.That(evasionAbility.CanBlockCalled);
        }

        [Test]
        public void Test_block_is_not_legal_until_all_restrictions_are_satisfied()
        {
            DeclareBlockersContext blockInfo = new DeclareBlockersContext(m_attackingCreatures, m_blockingCreatures);
            DeclareBlockersResult result = CreateResult(m_attackingCreatures[0], m_blockingCreatures[0]);

            MockEvasionAbility evasionAbility = m_game.CreateAbility<MockEvasionAbility>(m_attackingCreatures[0]);
            evasionAbility.CanBlockResult = false;

            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, result);
            m_sequencerTester.Expect_Player_DeclareBlockers(m_playerB, blockInfo, DeclareBlockersResult.Empty);
            Expect_All_Players_pass(m_playerA);

            RunStep(m_playerA);

            Assert.That(evasionAbility.CanBlockCalled);
        }

        #endregion

        #region Mock Types

        private class MockBlockAbility : SpellAbility
        {
            public override AbilityType AbilityType => AbilityType.Block;
        }

        #endregion
    }
}
