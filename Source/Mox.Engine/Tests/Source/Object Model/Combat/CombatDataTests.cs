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

using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class CombatDataTests : BaseGameTests
    {
        #region Variables

        private CombatData m_combatData;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();
            m_combatData = m_game.CombatData;
            Assert.IsNotNull(m_combatData);

            m_card.Zone = m_game.Zones.Battlefield;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_access_the_combat_data_from_the_game()
        {
            Assert.Collections.Contains(m_combatData, m_game.Objects);
        }

        [Test]
        public void Test_Can_get_set_attackers()
        {
            DeclareAttackersResult result = new DeclareAttackersResult(m_card);

            Assert.Collections.IsEmpty(m_combatData.Attackers.AttackerIdentifiers);
            m_combatData.Attackers = result;
            Assert.AreEqual(result, m_combatData.Attackers);
        }

        [Test]
        public void Test_Can_get_set_blockers()
        {
            DeclareBlockersResult result = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_card, m_card));

            Assert.Collections.IsEmpty(m_combatData.Blockers.Blockers);
            m_combatData.Blockers = result;
            Assert.AreEqual(result, m_combatData.Blockers);
        }

        [Test]
        public void Test_IsBlocked_returns_whether_the_card_is_being_blocked()
        {
            Card attackingCard1 = CreateCard(m_playerA);
            Card attackingCard2 = CreateCard(m_playerA);

            Card blockingCard1 = CreateCard(m_playerB);
            Card blockingCard2 = CreateCard(m_playerB);

            m_combatData.Attackers = new DeclareAttackersResult(attackingCard1, attackingCard2);
            m_combatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard1),
                new DeclareBlockersResult.BlockingCreature(blockingCard2, attackingCard1)) ;

            Assert.IsTrue(m_combatData.IsBlocked(attackingCard1));
            Assert.IsFalse(m_combatData.IsBlocked(attackingCard2));
        }

        [Test]
        public void Test_GetBlockers_returns_the_blockers_for_the_given_attacker()
        {
            Card attackingCard1 = CreateCard(m_playerA);
            Card attackingCard2 = CreateCard(m_playerA);

            Card blockingCard1 = CreateCard(m_playerB);
            Card blockingCard2 = CreateCard(m_playerB);

            m_combatData.Attackers = new DeclareAttackersResult(attackingCard1, attackingCard2, m_card);
            m_combatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard2),
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard1),
                new DeclareBlockersResult.BlockingCreature(blockingCard2, attackingCard1));

            Assert.Collections.IsEmpty(m_combatData.GetBlockers(m_card));
            Assert.Collections.AreEqual(new[] { blockingCard1 }, m_combatData.GetBlockers(attackingCard2));
            Assert.Collections.AreEqual(new[] { blockingCard1, blockingCard2 }, m_combatData.GetBlockers(attackingCard1));
        }

        [Test]
        public void Test_GetAttackers_returns_the_attackers_for_the_given_blocker()
        {
            Card attackingCard1 = CreateCard(m_playerA);
            Card attackingCard2 = CreateCard(m_playerA);

            Card blockingCard1 = CreateCard(m_playerB);
            Card blockingCard2 = CreateCard(m_playerB);

            m_combatData.Attackers = new DeclareAttackersResult(attackingCard1, attackingCard2);
            m_combatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard2),
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard1),
                new DeclareBlockersResult.BlockingCreature(blockingCard2, attackingCard1));

            Assert.Collections.AreEqual(new[] { attackingCard1 }, m_combatData.GetAttackers(blockingCard2));
            Assert.Collections.AreEqual(new[] { attackingCard2, attackingCard1 }, m_combatData.GetAttackers(blockingCard1));
        }

        #region Removal from Combat

        [Test]
        public void Test_Removing_an_attacker_from_combat_removes_it_from_the_attackers_list()
        {
            m_combatData.Attackers = new DeclareAttackersResult(m_card);
            m_combatData.RemoveFromCombat(m_card);
            Assert.That(m_combatData.Attackers.IsEmpty);
        }

        [Test]
        public void Test_Removing_an_attacker_from_combat_removes_it_from_the_blockers_list()
        {
            Card attackingCard1 = CreateCard(m_playerA);
            Card attackingCard2 = CreateCard(m_playerA);

            Card blockingCard1 = CreateCard(m_playerB);
            Card blockingCard2 = CreateCard(m_playerB);

            m_combatData.Attackers = new DeclareAttackersResult(attackingCard1, attackingCard2);
            m_combatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard2),
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard1),
                new DeclareBlockersResult.BlockingCreature(blockingCard2, attackingCard1));

            m_combatData.RemoveFromCombat(attackingCard1);
            Assert.Collections.AreEqual(new[] { blockingCard1 }, m_combatData.GetBlockers(attackingCard2));
            Assert.Collections.IsEmpty(m_combatData.GetBlockers(attackingCard1));
            Assert.IsFalse(m_combatData.IsAttacking(attackingCard1));
            Assert.IsFalse(m_combatData.IsBlocked(attackingCard1));
        }

        [Test]
        public void Test_An_attacker_that_leaves_the_battlefield_is_removed_from_combat()
        {
            m_combatData.Attackers = new DeclareAttackersResult(m_card);
            m_card.Zone = m_game.Zones.Graveyard;
            Assert.That(m_combatData.Attackers.IsEmpty);
        }

        [Test]
        public void Test_An_attacker_that_changes_controller_is_removed_from_combat()
        {
            m_combatData.Attackers = new DeclareAttackersResult(m_card);
            m_card.Controller = m_playerB;
            Assert.That(m_combatData.Attackers.IsEmpty);
        }

        [Test]
        public void Test_Removing_a_blocker_from_combat_removes_it_from_the_blockers_list()
        {
            Card attackingCard1 = CreateCard(m_playerA);
            Card attackingCard2 = CreateCard(m_playerA);

            Card blockingCard1 = CreateCard(m_playerB);
            Card blockingCard2 = CreateCard(m_playerB);

            m_combatData.Attackers = new DeclareAttackersResult(attackingCard1, attackingCard2);
            m_combatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard2),
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard1),
                new DeclareBlockersResult.BlockingCreature(blockingCard2, attackingCard1));

            m_combatData.RemoveFromCombat(blockingCard1);
            Assert.Collections.IsEmpty(m_combatData.GetBlockers(attackingCard2));
            Assert.Collections.AreEqual(new[] { blockingCard2 }, m_combatData.GetBlockers(attackingCard1));
        }

        [Test]
        public void Test_A_blocker_that_leaves_the_battlefield_is_removed_from_combat()
        {
            Card attackingCard1 = CreateCard(m_playerA);
            Card attackingCard2 = CreateCard(m_playerA);

            Card blockingCard1 = CreateCard(m_playerB);
            Card blockingCard2 = CreateCard(m_playerB);

            m_combatData.Attackers = new DeclareAttackersResult(attackingCard1, attackingCard2);
            m_combatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard2),
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard1),
                new DeclareBlockersResult.BlockingCreature(blockingCard2, attackingCard1));

            blockingCard1.Zone = m_game.Zones.Graveyard;
            Assert.Collections.IsEmpty(m_combatData.GetBlockers(attackingCard2));
            Assert.Collections.AreEqual(new[] { blockingCard2 }, m_combatData.GetBlockers(attackingCard1));
        }

        [Test]
        public void Test_A_blocker_that_changes_controller_is_removed_from_combat()
        {
            Card attackingCard1 = CreateCard(m_playerA);
            Card attackingCard2 = CreateCard(m_playerA);

            Card blockingCard1 = CreateCard(m_playerB);
            Card blockingCard2 = CreateCard(m_playerB);
            blockingCard2.Zone = m_game.Zones.Battlefield;

            m_combatData.Attackers = new DeclareAttackersResult(attackingCard1, attackingCard2);
            m_combatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard2),
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard1),
                new DeclareBlockersResult.BlockingCreature(blockingCard2, attackingCard1));
            
            blockingCard2.Controller = m_playerA;
            Assert.Collections.AreEqual(new[] { blockingCard1 }, m_combatData.GetBlockers(attackingCard1));
            Assert.Collections.AreEqual(new[] { blockingCard1 }, m_combatData.GetBlockers(attackingCard2));
        }

        [Test]
        public void Test_A_creature_remains_blocked_even_if_all_the_creatures_blocking_it_are_removed_from_combat()
        {
            Card attackingCard1 = CreateCard(m_playerA);
            Card attackingCard2 = CreateCard(m_playerA);

            Card blockingCard1 = CreateCard(m_playerB);
            Card blockingCard2 = CreateCard(m_playerB);

            m_combatData.Attackers = new DeclareAttackersResult(attackingCard1, attackingCard2);
            m_combatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard2),
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard1),
                new DeclareBlockersResult.BlockingCreature(blockingCard2, attackingCard1));

            m_combatData.RemoveFromCombat(blockingCard1);
            m_combatData.RemoveFromCombat(blockingCard2);
            Assert.Collections.IsEmpty(m_combatData.GetBlockers(attackingCard1));
            Assert.Collections.IsEmpty(m_combatData.GetBlockers(attackingCard2));

            Assert.That(m_combatData.IsBlocked(attackingCard1));
            Assert.That(m_combatData.IsBlocked(attackingCard2));
        }

        [Test]
        public void Test_IsAttacking_returns_true_for_an_attacking_card()
        {
            Card attackingCard1 = CreateCard(m_playerA);
            Card attackingCard2 = CreateCard(m_playerA);

            Card blockingCard1 = CreateCard(m_playerB);
            Card blockingCard2 = CreateCard(m_playerB);

            m_combatData.Attackers = new DeclareAttackersResult(attackingCard1, attackingCard2);
            m_combatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard2),
                new DeclareBlockersResult.BlockingCreature(blockingCard1, attackingCard1),
                new DeclareBlockersResult.BlockingCreature(blockingCard2, attackingCard1));

            m_combatData.RemoveFromCombat(blockingCard1);

            Assert.IsTrue(m_combatData.IsAttacking(attackingCard1));
            Assert.IsTrue(m_combatData.IsAttacking(attackingCard2));
            Assert.IsFalse(m_combatData.IsAttacking(blockingCard1));
        }

        #endregion

        #endregion
    }
}
