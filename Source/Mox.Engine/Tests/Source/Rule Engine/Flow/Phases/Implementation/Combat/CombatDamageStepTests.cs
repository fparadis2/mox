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

using Mox.Abilities;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class CombatDamageStepTests : BaseStepTests<CombatDamageStep>
    {
        #region Variables

        private Card m_blockingCreature1;
        private Card m_blockingCreature2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_sequencerTester.MockAllPlayersChoices();

            m_step = new CombatDamageStep();

            m_card.Type = Type.Creature;
            m_card.Power = 2;
            m_card.Toughness = 3;

            m_blockingCreature1 = CreateCard(m_playerB);
            m_blockingCreature2 = CreateCard(m_playerB);

            foreach (Card card in m_game.Cards)
            {
                card.Type = Type.Creature;
            }

            m_game.CombatData.SetAttackTarget(m_playerB);
        }

        #endregion

        #region  Utilities

        private void Expect_All_Players_pass(Player startingPlayer, System.Action callback = null)
        {
            foreach (Player player in Player.Enumerate(startingPlayer, false))
            {
                if (player == startingPlayer)
                {
                    var expectation = m_sequencerTester.Expect_Player_GivePriority(player, null);
                    expectation.Callback(callback);
                }
                else
                {
                    m_sequencerTester.Expect_Player_GivePriority(player, null);
                }
            }
        }

        #endregion

        #region Tests

        #region General

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(Steps.CombatDamage, m_step.Type);
        }

        [Test]
        public void Test_Phase_is_completely_skipped_if_no_attackers_declared()
        {
            m_game.CombatData.Attackers = null;
            RunStep(m_playerA);
        }

        [Test]
        public void Test_Unblocked_creatures_do_damage_to_the_defending_player()
        {
            m_playerB.Life = 20;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);

            Expect_All_Players_pass(m_playerA);
            RunStep(m_playerA);

            Assert.AreEqual(18, m_playerB.Life);
        }

        [Test]
        public void Test_Creatures_blocked_by_one_creature_assign_all_their_damage_to_that_creature()
        {
            m_blockingCreature1.Damage = 10;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            Expect_All_Players_pass(m_playerA);
            RunStep(m_playerA);

            Assert.AreEqual(12, m_blockingCreature1.Damage);
        }

        [Test]
        public void Test_Creatures_blocked_by_more_than_one_creature_assign_their_damage_to_them_in_order()
        {
            m_card.Power = 10;

            m_blockingCreature1.Damage = 3;
            m_blockingCreature1.Toughness = 10;

            m_blockingCreature2.Toughness = 10;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card),
                new DeclareBlockersResult.BlockingCreature(m_blockingCreature2, m_card));

            Expect_All_Players_pass(m_playerA);
            RunStep(m_playerA);

            Assert.AreEqual(10, m_blockingCreature1.Damage);
            Assert.AreEqual(3, m_blockingCreature2.Damage);
        }

        [Test]
        public void Test_Blockers_do_damage_to_the_attacking_creatures()
        {
            m_card.Power = 0;
            m_blockingCreature1.Power = 1;
            m_blockingCreature2.Power = 1;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card),
                new DeclareBlockersResult.BlockingCreature(m_blockingCreature2, m_card));

            Expect_All_Players_pass(m_playerA);
            RunStep(m_playerA);

            Assert.AreEqual(2, m_card.Damage);
        }

        #endregion

        #region First/Double Strike

        [Test]
        public void Test_When_an_attacker_has_first_strike_he_assigns_damage_in_an_additional_step()
        {
            m_game.CreateAbility<FirstStrikeAbility>(m_card);

            m_blockingCreature1.Power = 1;

            m_card.Damage = 0;
            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(0, m_card.Damage);
                    Assert.AreEqual(2, m_blockingCreature1.Damage);
                });
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(1, m_card.Damage);
                    Assert.AreEqual(2, m_blockingCreature1.Damage);
                });
            }

            RunStep(m_playerA);

            Assert.AreEqual(1, m_card.Damage);
            Assert.AreEqual(2, m_blockingCreature1.Damage);
        }

        [Test]
        public void Test_When_an_attacker_has_double_strike_he_assigns_damage_in_an_additional_step_in_addition_to_the_normal_step()
        {
            m_game.CreateAbility<DoubleStrikeAbility>(m_card);

            m_blockingCreature1.Power = 1;

            m_card.Damage = 0;
            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(0, m_card.Damage);
                    Assert.AreEqual(2, m_blockingCreature1.Damage);
                });
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(1, m_card.Damage);
                    Assert.AreEqual(4, m_blockingCreature1.Damage);
                });
            }

            RunStep(m_playerA);

            Assert.AreEqual(1, m_card.Damage);
            Assert.AreEqual(4, m_blockingCreature1.Damage);
        }

        [Test]
        public void Test_When_a_blocker_has_first_strike_he_assigns_damage_in_an_additional_step()
        {
            m_game.CreateAbility<FirstStrikeAbility>(m_blockingCreature1);

            m_blockingCreature1.Power = 1;

            m_card.Damage = 0;
            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(1, m_card.Damage);
                    Assert.AreEqual(0, m_blockingCreature1.Damage);
                });
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(1, m_card.Damage);
                    Assert.AreEqual(2, m_blockingCreature1.Damage);
                });
            }

            RunStep(m_playerA);

            Assert.AreEqual(1, m_card.Damage);
            Assert.AreEqual(2, m_blockingCreature1.Damage);
        }

        [Test]
        public void Test_When_a_blocker_has_double_strike_he_assigns_damage_in_an_additional_step_in_addition_to_the_normal_step()
        {
            m_game.CreateAbility<DoubleStrikeAbility>(m_blockingCreature1);

            m_blockingCreature1.Power = 1;

            m_card.Damage = 0;
            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(1, m_card.Damage);
                    Assert.AreEqual(0, m_blockingCreature1.Damage);
                });
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(2, m_card.Damage);
                    Assert.AreEqual(2, m_blockingCreature1.Damage);
                });
            }

            RunStep(m_playerA);

            Assert.AreEqual(2, m_card.Damage);
            Assert.AreEqual(2, m_blockingCreature1.Damage);
        }

        [Test]
        public void Test_Removing_double_strike_from_a_creature_during_the_first_combat_damage_step_will_stop_it_from_assigning_combat_damage_in_the_second_combat_damage_step()
        {
            Ability ability = m_game.CreateAbility<DoubleStrikeAbility>(m_card);

            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(2, m_blockingCreature1.Damage);
                    ability.Remove();

                });
                Expect_All_Players_pass(m_playerA, () => Assert.AreEqual(2, m_blockingCreature1.Damage));
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Giving_double_strike_to_a_creature_with_first_strike_after_it_has_already_dealt_combat_damage_in_the_first_combat_damage_step_will_allow_the_creature_to_assign_combat_damage_in_the_second_combat_damage_step()
        {
            m_game.CreateAbility<FirstStrikeAbility>(m_card);

            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(2, m_blockingCreature1.Damage);
                    m_game.CreateAbility<DoubleStrikeAbility>(m_card);

                });
                Expect_All_Players_pass(m_playerA, () => Assert.AreEqual(4, m_blockingCreature1.Damage));
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Giving_first_strike_to_a_creature_without_it_after_combat_damage_has_already_been_dealt_in_the_first_combat_damage_step_wont_prevent_that_creature_from_assigning_combat_damage_in_the_second_combat_damage_step()
        {
            m_game.CreateAbility<FirstStrikeAbility>(m_blockingCreature1);

            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(0, m_blockingCreature1.Damage);
                    m_game.CreateAbility<FirstStrikeAbility>(m_card);

                });
                Expect_All_Players_pass(m_playerA, () => Assert.AreEqual(2, m_blockingCreature1.Damage));
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Removing_first_strike_from_a_creature_after_it_has_already_dealt_combat_damage_in_the_first_combat_damage_step_wont_allow_it_to_also_assign_combat_damage_in_the_second_combat_damage_step()
        {
            Ability ability = m_game.CreateAbility<FirstStrikeAbility>(m_card);

            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(2, m_blockingCreature1.Damage);
                    ability.Remove();

                });
                Expect_All_Players_pass(m_playerA, () => Assert.AreEqual(2, m_blockingCreature1.Damage));
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Removing_first_strike_from_a_creature_after_it_has_already_dealt_combat_damage_in_the_first_combat_damage_step_wont_allow_it_to_also_assign_combat_damage_in_the_second_combat_damage_step_unless_it_has_double_strike()
        {
            m_game.CreateAbility<DoubleStrikeAbility>(m_card);
            Ability ability = m_game.CreateAbility<FirstStrikeAbility>(m_card);

            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(2, m_blockingCreature1.Damage);
                    ability.Remove();

                });
                Expect_All_Players_pass(m_playerA, () => Assert.AreEqual(4, m_blockingCreature1.Damage));
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_If_a_creature_is_killed_in_the_first_combat_damage_step_it_wont_deal_damage()
        {
            m_game.CreateAbility<FirstStrikeAbility>(m_card);

            m_card.Power = 1000;
            m_blockingCreature1.Zone = m_game.Zones.Battlefield;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA);
                Expect_All_Players_pass(m_playerA, () => Assert.AreEqual(m_game.Zones.Graveyard, m_blockingCreature1.Zone));
            }

            RunStep(m_playerA);

            Assert.AreEqual(0, m_card.Damage);
        }

        #endregion

        #region Trample

        [Test]
        public void Test_Attackers_with_Trample_assign_extra_damage_to_the_player()
        {
            m_card.Manager.CreateAbility<TrampleAbility>(m_card);
            m_card.Power = 10;

            m_blockingCreature1.Damage = 3;
            m_blockingCreature1.Toughness = 5;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            m_playerB.Life = 20;

            Expect_All_Players_pass(m_playerA);
            RunStep(m_playerA);

            Assert.AreEqual(5, m_blockingCreature1.Damage);
            Assert.AreEqual(12, m_playerB.Life);
        }

        [Test]
        public void Test_Attackers_with_Trample_assign_extra_damage_to_the_player_even_with_multiple_blockers()
        {
            m_card.Manager.CreateAbility<TrampleAbility>(m_card);
            m_card.Power = 10;

            m_blockingCreature1.Damage = 3;
            m_blockingCreature1.Toughness = 5;

            m_blockingCreature2.Damage = 0;
            m_blockingCreature2.Toughness = 3;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(
                new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card),
                new DeclareBlockersResult.BlockingCreature(m_blockingCreature2, m_card));

            m_playerB.Life = 20;

            Expect_All_Players_pass(m_playerA);
            RunStep(m_playerA);

            Assert.AreEqual(5, m_blockingCreature1.Damage);
            Assert.AreEqual(3, m_blockingCreature2.Damage);
            Assert.AreEqual(15, m_playerB.Life);
        }

        [Test]
        public void Test_Attackers_with_Double_Strike_and_Trample_can_trample_twice()
        {
            m_game.CreateAbility<DoubleStrikeAbility>(m_card);
            m_game.CreateAbility<TrampleAbility>(m_card);

            m_card.Power = 2;

            m_blockingCreature1.Toughness = 1;
            m_blockingCreature1.Damage = 0;

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            m_game.CombatData.Blockers = new DeclareBlockersResult(new DeclareBlockersResult.BlockingCreature(m_blockingCreature1, m_card));

            m_playerB.Life = 20;

            using (m_mockery.Ordered())
            {
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(1, m_blockingCreature1.Damage);
                    Assert.AreEqual(19, m_playerB.Life);
                });
                Expect_All_Players_pass(m_playerA, () =>
                {
                    Assert.AreEqual(1, m_card.Damage);
                    Assert.AreEqual(17, m_playerB.Life);
                });
            }

            RunStep(m_playerA);

            Assert.AreEqual(17, m_playerB.Life);
        }

        #endregion

        #endregion
    }
}
