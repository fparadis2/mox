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

using Mox.Flow.Parts;
using Mox.Flow.Phases;

namespace Mox.AI.Functional
{
    [TestFixture]
    public class CombatFunctionalTests : AIFunctionalTests
    {
        #region Utilities

        private static Phase CreateBlockerCombatPhase()
        {
            Phase combatPhase = new Phase(Phases.Combat);
            combatPhase.Steps.Add(new DeclareBlockersStep());
            combatPhase.Steps.Add(new CombatDamageStep());
            combatPhase.Steps.Add(new EndOfCombatStep());
            return combatPhase;
        }

        private void Declare_Attackers(Player player)
        {
            Phase phase = DefaultTurnFactory.CreateCombatPhase();
            RunUntil<DeclareBlockersStep>(new SequencePhase(player, phase));
        }

        private void Declare_Blockers(Player player)
        {
            RunUntil<CombatDamageStep>(new SequencePhase(player, CreateBlockerCombatPhase()));
        }

        #endregion

        #region Setup / Teardown

        private IDisposable m_disableSummoningSickness;

        public override void Setup()
        {
            base.Setup();

            m_disableSummoningSickness = Rules.SummoningSickness.Bypass();
        }

        public override void Teardown()
        {
            DisposableHelper.SafeDispose(m_disableSummoningSickness);

            base.Teardown();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_The_AI_will_attack_when_able()
        {
            AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            Do_Combat(m_playerA);

            Assert.AreEqual(20, m_playerA.Life);
            Assert.AreEqual(15, m_playerB.Life);
        }

        [Test]
        public void Test_The_AI_will_declare_attackers_when_able()
        {
            Card attackingCreature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            Declare_Attackers(m_playerA);

            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature));
        }

        [Test]
        public void Test_The_AI_will_not_attack_if_losing_creature()
        {
            Card attackingCreature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            blockingCreature.Power = 20;
            blockingCreature.Toughness = 20;

            Declare_Attackers(m_playerA);

            Assert.IsFalse(m_game.CombatData.IsAttacking(attackingCreature));
        }

        [Test]
        public void Test_The_AI_will_only_attack_with_sure_creatures()
        {
            Card attackingCreature1 = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card attackingCreature2 = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            attackingCreature1.Power = 1;
            attackingCreature1.Toughness = 1;

            attackingCreature2.Power = 5;
            attackingCreature2.Toughness = 5;

            blockingCreature.Power = 3;
            blockingCreature.Toughness = 3;

            Declare_Attackers(m_playerA);

            Assert.IsFalse(m_game.CombatData.IsAttacking(attackingCreature1));
            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature2));
        }

        [Test]
        public void Test_The_AI_will_gladly_trade_creatures()
        {
            Card attackingCreature1 = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            attackingCreature1.Power = 3;
            attackingCreature1.Toughness = 3;

            blockingCreature.Power = 3;
            blockingCreature.Toughness = 3;

            Declare_Attackers(m_playerA);

            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature1));
        }

        [Test]
        public void Test_The_AI_will_gladly_trade_creatures_if_its_good_for_him()
        {
            Card attackingCreature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            attackingCreature.Power = 3;
            attackingCreature.Toughness = 3;

            blockingCreature.Power = 10;
            blockingCreature.Toughness = 3;

            Declare_Attackers(m_playerA);

            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature));
        }

        [Test]
        public void Test_The_AI_will_gladly_trade_creatures_in_order_to_land_a_hit_on_a_player()
        {
            Card attackingCreature1 = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card attackingCreature2 = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            attackingCreature1.Power = 3;
            attackingCreature1.Toughness = 3;

            attackingCreature2.Power = 1;
            attackingCreature2.Toughness = 1;

            blockingCreature.Power = 3;
            blockingCreature.Toughness = 3;

            Declare_Attackers(m_playerA);

            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature1));
            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature2));
        }

        [Test]
        public void Test_The_AI_will_sacrifice_a_creature_in_order_to_give_the_final_blow()
        {
            Card attackingCreature1 = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card attackingCreature2 = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            attackingCreature1.Power = 1;
            attackingCreature1.Toughness = 1;

            attackingCreature2.Power = 1;
            attackingCreature2.Toughness = 1;

            blockingCreature.Power = 20;
            blockingCreature.Toughness = 20;

            m_playerB.Life = 1;

            Declare_Attackers(m_playerA);

            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature1));
            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature2));
        }

        [Test]
        public void Test_The_AI_will_attack_with_all_its_creatures_if_able()
        {
            AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            Do_Combat(m_playerA);

            Assert.AreEqual(20, m_playerA.Life);
            Assert.AreEqual(10, m_playerB.Life);
        }

        [Test]
        public void Test_The_AI_will_shock_the_attacking_creature_if_it_kills_it()
        {
            Card invincibleCreature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card creatureToKill = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");

            SetupGame();

            invincibleCreature.Toughness = 10;
            creatureToKill.Toughness = 2;

            m_playerB.ManaPool.Red = 10;

            Do_Combat(m_playerA);

            Assert.AreEqual(20, m_playerA.Life);
            Assert.AreEqual(15, m_playerB.Life);
            Assert.AreEqual(m_game.Zones.Graveyard, creatureToKill.Zone);
        }

        [Test]
        public void Test_The_AI_will_block_when_able()
        {
            Card attackingCreature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            attackingCreature.Power = 1;
            attackingCreature.Toughness = 1;

            blockingCreature.Power = 3;
            blockingCreature.Toughness = 3;

            m_game.CombatData.Attackers = new DeclareAttackersResult(attackingCreature);

            Declare_Blockers(m_playerA);

            Assert.Collections.AreEqual(new[] { blockingCreature }, m_game.CombatData.GetBlockers(attackingCreature));
        }

        [Test]
        public void Test_The_AI_will_not_block_when_not_able()
        {
            Card attackingCreature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            m_game.CreateAbility<FlyingAbility>(attackingCreature);

            SetupGame();

            attackingCreature.Power = 1;
            attackingCreature.Toughness = 1;

            blockingCreature.Power = 3;
            blockingCreature.Toughness = 3;

            m_game.CombatData.Attackers = new DeclareAttackersResult(attackingCreature);

            Declare_Blockers(m_playerA);

            Assert.Collections.IsEmpty(m_game.CombatData.Blockers.Blockers);
        }

        [Test]
        public void Test_The_AI_will_block_a_flying_creature_with_a_reach_creature_when_able()
        {
            Card attackingCreature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            Card dummyCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            m_game.CreateAbility<FlyingAbility>(attackingCreature);
            m_game.CreateAbility<ReachAbility>(blockingCreature);

            SetupGame();

            attackingCreature.Power = 1;
            attackingCreature.Toughness = 5;

            dummyCreature.Power = 10;
            dummyCreature.Toughness = 3;

            blockingCreature.Power = 3;
            blockingCreature.Toughness = 3;

            m_game.CombatData.Attackers = new DeclareAttackersResult(attackingCreature);

            Declare_Blockers(m_playerA);

            Assert.Collections.AreEqual(new[] { blockingCreature }, m_game.CombatData.GetBlockers(attackingCreature));
        }

        [Test]
        public void Test_The_AI_can_handle_double_and_first_Strike()
        {
            Card attackingCreature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            m_game.CreateAbility<DoubleStrikeAbility>(attackingCreature);

            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            m_game.CreateAbility<FirstStrikeAbility>(blockingCreature);

            SetupGame();

            attackingCreature.Power = 3;
            attackingCreature.Toughness = 4;

            blockingCreature.Power = 3;
            blockingCreature.Toughness = 3;

            Declare_Attackers(m_playerA);

            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature));
        }

        [Test]
        public void Test_The_AI_will_attack_with_a_first_strike_creature()
        {
            Card attackingCreature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            m_game.CreateAbility<FirstStrikeAbility>(attackingCreature);

            Card blockingCreature = AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Dross Crocodile");

            SetupGame();

            attackingCreature.Power = 3;
            attackingCreature.Toughness = 3;

            blockingCreature.Power = 3;
            blockingCreature.Toughness = 3;

            Declare_Attackers(m_playerA);

            Assert.IsTrue(m_game.CombatData.IsAttacking(attackingCreature));
        }

        [Test]
        public void Test_The_AI_cannot_attack_when_pacified()
        {
            Card crocodile = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
            Card pacifism = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Pacifism");

            SetupGame();

            pacifism.Attach(crocodile);

            Do_Combat(m_playerA);

            Assert.AreEqual(20, m_playerA.Life);
            Assert.AreEqual(20, m_playerB.Life);
        }

        [Test]
        public void Test_Stress_test_Declare_identical_attackers()
        {
            SetupGame();

            const int NumCreatures = 4;

            foreach (Player player in m_game.Players)
            {
                player.Life = 100;

                for (int i = 0; i < NumCreatures; i++)
                {
                    Card creature = AddCard(player, m_game.Zones.Battlefield, "10E", "Dross Crocodile");
                    creature.Type = Type.Creature;
                    creature.Power = 2;
                    creature.Toughness = 2;
                }
            }

            using (Profile())
            {
                Declare_Attackers(m_playerA);
            }
        }

        [Test]
        public void Test_Stress_test_with_Nightmares()
        {
            SetupGame();

            const int NumCreatures = 4;

            foreach (Player player in m_game.Players)
            {
                player.Life = 100;

                Card swamp = AddCard(player, m_game.Zones.Battlefield, "10E", "Swamp");
                swamp.SubTypes |= SubType.Swamp;

                for (int i = 0; i < NumCreatures; i++)
                {
                    Card creature = AddCard(player, m_game.Zones.Battlefield, "10E", "Nightmare");
                    InitializeCard(creature);
                    Assert.AreNotEqual(0, creature.Toughness, "Sanity check");
                }
            }

            using (Profile())
            {
                Declare_Attackers(m_playerA);
            }
        }

        #endregion
    }
}
