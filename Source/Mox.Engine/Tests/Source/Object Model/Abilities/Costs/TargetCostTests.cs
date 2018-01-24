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
using Mox.Flow;
using NUnit.Framework;

namespace Mox.Abilities
{
    #warning todo spell_v2
    /*[TestFixture]
    public class TargetCostTests : CostTestsBase
    {
        #region Variables

        private TargetCost m_cost;

        private Predicate<GameObject> m_predicate;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_predicate = (target => true);

            m_cost = new TargetCost(Predicate);

            m_card.Zone = m_game.Zones.Battlefield;
        }

        #endregion

        #region Utilities

        private void Expect_Target(Player player, IEnumerable<GameObject> possibleTargets, GameObject result)
        {
            m_sequencer.Expect_Player_Target(player, true, possibleTargets, result, TargetContextType.Normal);
        }

        private bool Predicate(GameObject targetable)
        {
            return m_predicate(targetable);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new TargetCost(null); });
        }

        [Test]
        public void Test_Can_only_be_paid_if_there_exists_a_valid_target()
        {
            AbilityEvaluationContext context = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal);

            // Always allow to play.. can always cancel after that.
            Assert.IsTrue(m_cost.CanExecute(context, m_spellContext));

            context.UserMode = true;

            // But in user mode, don't allow if no legal target
            m_cost = TargetCost.Creature();
            m_card.Type = Type.Creature;
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsTrue(m_cost.CanExecute(context, m_spellContext));

            m_card.Type = Type.Artifact;
            Assert.IsFalse(m_cost.CanExecute(context, m_spellContext));
        }

        [Test]
        public void Test_Execute()
        {
            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), m_card);
            Execute(m_cost, true);

            Assert.AreEqual(m_card, m_cost.Resolve(m_game));
        }

        [Test]
        public void Test_Execute_will_ask_the_player_until_he_provides_a_valid_object()
        {
            m_predicate = (target => target is Player);

            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), m_card);
            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), m_playerA);
            Execute(m_cost, true);

            Assert.AreEqual(m_playerA, m_cost.Resolve(m_game));
        }

        [Test]
        public void Test_Execute_will_cancel_if_the_player_returns_nothing()
        {
            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), null);
            Execute(m_cost, false);

            Assert.IsNull(m_cost.Resolve(m_game));
        }

        [Test]
        public void Test_Execute_will_cancel_if_there_are_no_valid_targets()
        {
            m_predicate = (target => false);
            Execute(m_cost, false);
        }

        #region Common targets

        [Test]
        public void Test_TargetPlayer_can_only_target_players()
        {
            TargetCost cost = TargetCost.Player();
            Assert.IsTrue(cost.Filter(m_playerA));
            Assert.IsFalse(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_Opponent_can_only_target_opponents()
        {
            TargetCost cost = TargetCost.Player().Opponent(m_playerA);

            Assert.IsFalse(cost.Filter(m_card));
            Assert.IsFalse(cost.Filter(m_playerA));
            Assert.IsTrue(cost.Filter(m_playerB));
        }

        [Test]
        public void Test_TargetCreature_can_only_target_creatures()
        {
            TargetCost cost = TargetCost.Creature();
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Type = Type.Creature;
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsTrue(cost.Filter(m_card));

            m_card.Zone = m_game.Zones.Hand;
            Assert.IsFalse(cost.Filter(m_card));
        }

        [Test]
        public void Test_TargetPermanent_can_only_target_permanents()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            TargetCost cost = TargetCost.Permanent();
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Type = Type.Creature;
            Assert.IsTrue(cost.Filter(m_card));

            m_card.Type = Type.Land;
            Assert.IsTrue(cost.Filter(m_card));

            m_card.Type = Type.Instant;
            Assert.IsFalse(cost.Filter(m_card));
        }

        [Test]
        public void Test_TargetCard_can_only_target_cards()
        {
            TargetCost cost = TargetCost.Card();
            m_card.Zone = m_game.Zones.Library;
            Assert.IsFalse(cost.Filter(m_card));
            Assert.IsFalse(cost.Filter(m_playerA));

            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsTrue(cost.Filter(m_card));
        }

        [Test]
        public void Test_TargetPlayerOrCreature_can_target_players_or_creatures()
        {
            TargetCost cost = TargetCost.Player() | TargetCost.Creature();
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Type = Type.Creature;
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsTrue(cost.Filter(m_card));
            Assert.IsTrue(cost.Filter(m_playerA));

            m_card.Zone = m_game.Zones.Hand;
            Assert.IsFalse(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_Attacking_can_only_target_attacking_creatures()
        {
            TargetCost cost = TargetCost.Creature().Attacking();
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Type = Type.Creature;
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsFalse(cost.Filter(m_card));

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);
            Assert.IsTrue(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_Blocking_can_only_target_blocking_creatures()
        {
            Card blockedCreature = CreateCard(m_playerB);

            TargetCost cost = TargetCost.Creature().Blocking();
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Type = Type.Creature;
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsFalse(cost.Filter(m_card));

            m_game.CombatData.Blockers = new DeclareBlockersResult(new[] { new DeclareBlockersResult.BlockingCreature(m_card, blockedCreature) });
            Assert.IsTrue(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_CardsOfAnyType_can_target_cards_of_the_given_type()
        {
            TargetCost cost = TargetCost.Card().OfAnyType(Type.Creature | Type.Planeswalker);

            m_card.Zone = m_game.Zones.Exile;
            m_card.Type = Type.Creature;
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Zone = m_game.Zones.Battlefield;
            m_card.Type = Type.Enchantment;
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Type = Type.Creature;
            Assert.IsTrue(cost.Filter(m_card));

            m_card.Type = Type.Planeswalker;
            Assert.IsTrue(cost.Filter(m_card));

            m_card.Type = Type.Creature | Type.Enchantment;
            Assert.IsTrue(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_CardsOfAnyColor_can_target_cards_of_the_given_colors()
        {
            TargetCost cost = TargetCost.Card().OfAnyColor(Color.Blue | Color.Red);

            m_card.Zone = m_game.Zones.Exile;
            m_card.Color = Color.Red;
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Zone = m_game.Zones.Battlefield;
            m_card.Color = Color.Green;
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Color = Color.Blue;
            Assert.IsTrue(cost.Filter(m_card));

            m_card.Color = Color.Red;
            Assert.IsTrue(cost.Filter(m_card));

            m_card.Color = Color.Red | Color.Blue;
            Assert.IsTrue(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_Tapped_can_only_target_tapped_cards()
        {
            TargetCost cost = TargetCost.Card().Tapped();

            m_card.Tapped = false;
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Tapped = true;
            Assert.IsTrue(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_Untapped_can_only_target_tapped_cards()
        {
            TargetCost cost = TargetCost.Card().Untapped();

            m_card.Tapped = false;
            Assert.IsTrue(cost.Filter(m_card));

            m_card.Tapped = true;
            Assert.IsFalse(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_With_restricts_to_cards_with_given_ability()
        {
            TargetCost cost = TargetCost.Card().With<FlyingAbility>();

            Assert.IsFalse(cost.Filter(m_card));

            FlyingAbility ability = m_game.CreateAbility<FlyingAbility>(m_card);
            Assert.IsTrue(cost.Filter(m_card));

            ability.Remove();
            Assert.IsFalse(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_Without_restricts_to_cards_without_given_ability()
        {
            TargetCost cost = TargetCost.Card().Without<FlyingAbility>();

            Assert.IsTrue(cost.Filter(m_card));

            FlyingAbility ability = m_game.CreateAbility<FlyingAbility>(m_card);
            Assert.IsFalse(cost.Filter(m_card));

            ability.Remove();
            Assert.IsTrue(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_UnderControl_restricts_to_cards_with_the_given_controller()
        {
            TargetCost cost = TargetCost.Card().UnderControl(m_playerB);

            m_card.Controller = m_playerA;
            Assert.IsFalse(cost.Filter(m_card));

            m_card.Controller = m_playerB;
            Assert.IsTrue(cost.Filter(m_card));

            m_card.Controller = m_playerA;
            Assert.IsFalse(cost.Filter(m_card));
        }

        [Test]
        public void Test_Target_Except_filters_out_the_result_of_another_target()
        {
            Card creature1 = CreateCard(m_playerA);
            creature1.Zone = m_game.Zones.Battlefield;

            Card creature2 = CreateCard(m_playerA);
            creature2.Zone = m_game.Zones.Battlefield;

            m_cost = TargetCost.Card();

            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), creature1);
            Execute(m_cost, true);

            Assert.AreEqual(creature1, m_cost.Resolve(m_game));

            TargetCost filteredCost = TargetCost.Card().Except(m_cost);

            Assert.IsFalse(filteredCost.Filter(creature1));
            Assert.IsTrue(filteredCost.Filter(creature2));
        }

        [Test]
        public void Test_Target_ExceptThisResult_filters_out_the_result_of_a_target()
        {
            Card creature1 = CreateCard(m_playerA);
            creature1.Zone = m_game.Zones.Battlefield;

            Card creature2 = CreateCard(m_playerA);
            creature2.Zone = m_game.Zones.Battlefield;

            m_cost = TargetCost.Card();

            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), creature1);
            Execute(m_cost, true);

            Assert.AreEqual(creature1, m_cost.Resolve(m_game));

            TargetCost filteredCost = m_cost.ExceptThisResult();

            Assert.IsFalse(filteredCost.Filter(creature1));
            Assert.IsTrue(filteredCost.Filter(creature2));
        }

        [Test]
        public void Test_Sacrifice_transforms_a_target_cost_into_a_sacrifice_cost()
        {
            var cost = TargetCost.Card().OfAnyColor(Color.Blue | Color.Red);
            var sacrifice = cost.Sacrifice();

            Assert.IsNotNull(sacrifice);
            Assert.AreEqual(cost.Filter, sacrifice.Filter);
        }

        #endregion

        #endregion
    }*/
}
