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

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class CheckStateBasedActionsTests : PartTestBase<CheckStateBasedActions>
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_part = new CheckStateBasedActions();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Players_with_0_life_lose()
        {
            Assert.IsNull(m_game.State.Winner);
            Assert.IsFalse(m_game.State.HasEnded);

            m_playerA.Life = 10;
            m_playerB.Life = 10;

            Execute(m_part);

            Assert.IsNull(m_game.State.Winner);
            Assert.IsFalse(m_game.State.HasEnded);

            m_playerA.Life = 0;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_playerB, m_game.State.Winner);
            Assert.IsTrue(m_game.State.HasEnded);
        }

        [Test]
        public void Test_Players_with_less_than_0_life_lose()
        {
            Assert.IsNull(m_game.State.Winner);
            Assert.IsFalse(m_game.State.HasEnded);

            m_playerA.Life = -1;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_playerB, m_game.State.Winner);
            Assert.IsTrue(m_game.State.HasEnded);
        }

        [Test]
        public void Test_Creatures_with_less_than_1_toughness_are_sent_to_graveyard()
        {
            Card card0 = CreateCard(m_playerA); card0.Type = Type.Creature;
            Card card1 = CreateCard(m_playerB); card1.Type = Type.Creature;
            Card card2 = CreateCard(m_playerA); card2.Type = Type.Creature;

            card0.Zone = m_game.Zones.Battlefield;
            card1.Zone = m_game.Zones.Battlefield;
            card2.Zone = m_game.Zones.Battlefield;

            card0.Toughness = 10;
            card1.Toughness = 0;
            card2.Toughness = -1;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Battlefield, card0.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, card1.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, card2.Zone);
        }

        [Test]
        public void Test_Creatures_with_more_damage_than_toughness_are_sent_to_graveyard()
        {
            Card card0 = CreateCard(m_playerA); card0.Type = Type.Creature;
            Card card1 = CreateCard(m_playerB); card1.Type = Type.Creature;
            Card card2 = CreateCard(m_playerA); card2.Type = Type.Creature;

            card0.Zone = m_game.Zones.Battlefield;
            card1.Zone = m_game.Zones.Battlefield;
            card2.Zone = m_game.Zones.Battlefield;

            card0.Toughness = 10;
            card1.Toughness = 5;
            card2.Toughness = 2;

            card0.Damage = 5;
            card1.Damage = 5;
            card2.Damage = 5;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Battlefield, card0.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, card1.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, card2.Zone);
        }

        [Test]
        public void Test_If_an_Aura_is_not_attached_to_an_object_or_player_that_Aura_is_put_into_its_owners_graveyard()
        {
            Card card0 = CreateCard(m_playerA); card0.Type = Type.Enchantment; card0.SubTypes |= SubType.Aura;
            Card card1 = CreateCard(m_playerB); card1.Type = Type.Creature;

            card0.Zone = m_game.Zones.Battlefield;
            card1.Zone = m_game.Zones.Battlefield;
            card0.Attach(card1);

            card1.Zone = m_game.Zones.Graveyard;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Graveyard, card0.Zone);
            Assert.IsNull(card0.AttachedTo);
        }

        [Test]
        public void Test_If_an_aura_is_not_attached_to_a_creature_it_gets_put_into_a_graveyard()
        {
            Card card0 = CreateCard(m_playerA); card0.Type = Type.Enchantment; card0.SubTypes |= SubType.Aura;
            Card card1 = CreateCard(m_playerB); card1.Type = Type.Creature;

            card0.Zone = m_game.Zones.Battlefield;
            card1.Zone = m_game.Zones.Battlefield;
            card0.Attach(card1);

            card1.Type = Type.Land;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Graveyard, card0.Zone);
            Assert.IsNull(card0.AttachedTo);
        }

        [Test]
        public void Test_If_an_equipment_is_not_attached_to_an_object_or_player_that_equipment_is_unattached()
        {
            Card card0 = CreateCard(m_playerA); card0.Type = Type.Artifact; card0.SubTypes |= SubType.Equipment;
            Card card1 = CreateCard(m_playerB); card1.Type = Type.Creature;

            card0.Zone = m_game.Zones.Battlefield;
            card1.Zone = m_game.Zones.Battlefield;
            card0.Attach(card1);

            card1.Zone = m_game.Zones.Graveyard;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Battlefield, card0.Zone);
            Assert.IsNull(card0.AttachedTo);
        }

        [Test]
        public void Test_If_a_fortification_is_not_attached_to_a_land_that_fortification_is_unattached()
        {
            Card card0 = CreateCard(m_playerA); card0.Type = Type.Artifact; card0.SubTypes |= SubType.Fortification;
            Card card1 = CreateCard(m_playerB); card1.Type = Type.Land;

            card0.Zone = m_game.Zones.Battlefield;
            card1.Zone = m_game.Zones.Battlefield;
            card0.Attach(card1);

            card1.Type = Type.Creature;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Battlefield, card0.Zone);
            Assert.IsNull(card0.AttachedTo);
        }

        [Test]
        public void Test_If_a_creature_is_attached_to_an_object_or_player_it_becomes_unattached_and_remains_on_the_battlefield()
        {
            Card card0 = CreateCard(m_playerA); card0.Type = Type.Artifact; card0.SubTypes |= SubType.Fortification;
            Card card1 = CreateCard(m_playerB); card1.Type = Type.Land;

            card0.Zone = m_game.Zones.Battlefield;
            card1.Zone = m_game.Zones.Battlefield;
            card0.Attach(card1);

            card0.Toughness = 1;
            card0.Type |= Type.Creature;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Battlefield, card0.Zone);
            Assert.IsNull(card0.AttachedTo);
        }

        [Test]
        public void Test_If_a_permanent_thats_neither_an_Aura_an_Equipment_nor_a_Fortification_is_attached_to_an_object_or_player_it_becomes_unattached_and_remains_on_the_battlefield()
        {
            Card card0 = CreateCard(m_playerA); card0.Type = Type.Artifact; card0.SubTypes |= SubType.Fortification;
            Card card1 = CreateCard(m_playerB); card1.Type = Type.Land;

            card0.Zone = m_game.Zones.Battlefield;
            card1.Zone = m_game.Zones.Battlefield;
            card0.Attach(card1);

            card0.SubTypes = SubTypes.Empty;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Battlefield, card0.Zone);
            Assert.IsNull(card0.AttachedTo);
        }

        #region Legend rule

        private Card CreateLegendary(Player owner, string name)
        {
            Card card = CreateCard(owner, name);
            card.SuperType = SuperType.Legendary;
            card.Zone = m_game.Zones.Battlefield;
            return card;
        }

        [Test]
        public void Test_If_two_or_more_legendary_permanents_with_the_same_name_are_on_the_battlefield_all_are_put_into_their_owners_graveyards()
        {
            Card legendary1 = CreateLegendary(m_playerA, "CreatureA");
            Card legendary2 = CreateLegendary(m_playerB, "CreatureA");
            Card legendary3 = CreateLegendary(m_playerA, "CreatureA");
            Card legendary4 = CreateLegendary(m_playerA, "CreatureB"); // OK
            Card legendary5 = CreateLegendary(m_playerB, "CreatureC"); // OK

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Graveyard, legendary1.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, legendary2.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, legendary3.Zone);
            Assert.AreEqual(m_game.Zones.Battlefield, legendary4.Zone);
            Assert.AreEqual(m_game.Zones.Battlefield, legendary5.Zone);
        }

        [Test]
        public void Test_Legend_rule_doesnt_apply_if_only_one_creature_is_legendary()
        {
            Card legendary1 = CreateLegendary(m_playerA, "CreatureA");
            Card legendary2 = CreateLegendary(m_playerB, "CreatureA"); legendary2.SuperType = SuperType.None;
            Card legendary3 = CreateLegendary(m_playerA, "CreatureA"); legendary3.SuperType = SuperType.None;

            m_sequencerTester.Run(m_part);

            Assert.AreEqual(m_game.Zones.Battlefield, legendary1.Zone);
            Assert.AreEqual(m_game.Zones.Battlefield, legendary2.Zone);
            Assert.AreEqual(m_game.Zones.Battlefield, legendary3.Zone);
        }

        #endregion

        #endregion
    }
}
