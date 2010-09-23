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

using Mox.Database.Library;

namespace Mox.Database.Sets
{
    [TestFixture]
    public class Factory10EEnchantmentsTests : BaseFactoryTests
    {
        #region Utilities

        private Card CreateCreature(Player owner, int power, int toughness)
        {
            Card creature = CreateCard(owner);
            creature.Type = Type.Creature;
            creature.Power = power;
            creature.Toughness = toughness;
            creature.Zone = m_game.Zones.Battlefield;
            return creature;
        }

        #endregion

        #region Tests

        #region White

        #region Angelic Chorus

        [Test]
        public void Test_Cant_explicitely_play_Angelic_Chorus_ability()
        {
            Card card = CreateCard<AngelicChorusCardFactory>(m_playerA, "10E", "Angelic Chorus");
            Assert.AreEqual(Type.Enchantment, card.Type);

            card.Zone = m_game.Zones.Battlefield;

            var abilities = card.Abilities.ToList();

            Assert.AreEqual(2, abilities.Count);
            Assert.IsInstanceOf<PlayCardAbility>(abilities[0]);
            Assert.IsFalse(CanPlay(m_playerA, abilities[1]));
        }

        [Test]
        public void Test_Angelic_Chorus_does_nothing_when_not_in_play()
        {
            Card creature = CreateCard<BasicLandCardFactory>(m_playerA, "10E", "Forest");
            creature.Zone = m_game.Zones.Hand;
            creature.Type = Type.Creature;
            creature.Toughness = 5;

            Card card = CreateCard<AngelicChorusCardFactory>(m_playerA, "10E", "Angelic Chorus");
            card.Zone = m_game.Zones.Graveyard;

            creature.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(20, m_playerA.Life);
            Assert.AreEqual(20, m_playerB.Life);
        }

        [Test]
        public void Test_When_Angelic_Chorus_is_in_play_it_triggers_when_a_creature_comes_under_the_control_of_the_ability_controller()
        {
            Card creature = CreateCard<BasicLandCardFactory>(m_playerA, "10E", "Forest");
            creature.Zone = m_game.Zones.Hand;
            creature.Type = Type.Creature;
            creature.Toughness = 5;

            Card card = CreateCard<AngelicChorusCardFactory>(m_playerA, "10E", "Angelic Chorus");
            card.Zone = m_game.Zones.Battlefield;

            creature.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(25, m_playerA.Life);
            Assert.AreEqual(20, m_playerB.Life);
        }

        [Test]
        public void Test_Angelic_Chorus_doesnt_trigger_if_card_is_not_under_control()
        {
            Card creature = CreateCard<BasicLandCardFactory>(m_playerB, "10E", "Forest");
            creature.Zone = m_game.Zones.Hand;
            creature.Type = Type.Creature;
            creature.Toughness = 5;

            Card card = CreateCard<AngelicChorusCardFactory>(m_playerA, "10E", "Angelic Chorus");
            card.Zone = m_game.Zones.Battlefield;

            creature.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(20, m_playerA.Life);
            Assert.AreEqual(20, m_playerB.Life);
        }

        [Test]
        public void Test_Angelic_Chorus_only_triggers_for_creatures()
        {
            Card creature = CreateCard<BasicLandCardFactory>(m_playerA, "10E", "Forest");
            creature.Zone = m_game.Zones.Hand;
            creature.Type = Type.Land;
            creature.Toughness = 5;

            Card card = CreateCard<AngelicChorusCardFactory>(m_playerA, "10E", "Angelic Chorus");
            card.Zone = m_game.Zones.Battlefield;

            creature.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(20, m_playerA.Life);
            Assert.AreEqual(20, m_playerB.Life);
        }

        #endregion

        #region Glorious Anthem

        [Test]
        public void Test_Glorious_Anthem()
        {
            Card card = CreateCard<GloriousAnthemCardFactory>(m_playerA, "10E", "Glorious Anthem");
            card.Zone = m_game.Zones.Battlefield;

            Card creature1 = CreateCreature(m_playerA, 1, 1);
            Card creature2 = CreateCreature(m_playerA, 1, 1);

            Assert.AreEqual(2, creature1.Power);
            Assert.AreEqual(2, creature1.Toughness);
            Assert.AreEqual(2, creature2.Power);
            Assert.AreEqual(2, creature2.Toughness);

            creature1.Controller = m_playerB;

            Assert.AreEqual(1, creature1.Power);
            Assert.AreEqual(1, creature1.Toughness);
            Assert.AreEqual(2, creature2.Power);
            Assert.AreEqual(2, creature2.Toughness);

            creature1.Controller = m_playerA;

            Assert.AreEqual(2, creature1.Power);
            Assert.AreEqual(2, creature1.Toughness);
            Assert.AreEqual(2, creature2.Power);
            Assert.AreEqual(2, creature2.Toughness);

            creature1.Type = Type.Artifact;
            creature2.Type = Type.Creature | Type.Land;

            Assert.AreEqual(1, creature1.Power);
            Assert.AreEqual(1, creature1.Toughness);
            Assert.AreEqual(2, creature2.Power);
            Assert.AreEqual(2, creature2.Toughness);
        }

        [Test]
        public void Test_Glorious_Anthem_entering_and_leaving_play()
        {
            Card card = CreateCard<GloriousAnthemCardFactory>(m_playerA, "10E", "Glorious Anthem");
            Card creature1 = CreateCreature(m_playerA, 1, 1);
            Card creature2 = CreateCreature(m_playerA, 1, 1);

            Assert.AreEqual(1, creature1.Power);
            Assert.AreEqual(1, creature1.Toughness);
            Assert.AreEqual(1, creature2.Power);
            Assert.AreEqual(1, creature2.Toughness);
            
            card.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(2, creature1.Power);
            Assert.AreEqual(2, creature1.Toughness);
            Assert.AreEqual(2, creature2.Power);
            Assert.AreEqual(2, creature2.Toughness);

            card.Zone = m_game.Zones.Hand;

            Assert.AreEqual(1, creature1.Power);
            Assert.AreEqual(1, creature1.Toughness);
            Assert.AreEqual(1, creature2.Power);
            Assert.AreEqual(1, creature2.Toughness);
        }

        [Test]
        public void Test_Glorious_Anthem_changing_controller()
        {
            Card card = CreateCard<GloriousAnthemCardFactory>(m_playerA, "10E", "Glorious Anthem");
            card.Zone = m_game.Zones.Battlefield;

            Card creature1 = CreateCreature(m_playerA, 1, 1);
            Card creature2 = CreateCreature(m_playerB, 1, 1);

            Assert.AreEqual(2, creature1.Power);
            Assert.AreEqual(2, creature1.Toughness);
            Assert.AreEqual(1, creature2.Power);
            Assert.AreEqual(1, creature2.Toughness);

            card.Controller = m_playerB;

            Assert.AreEqual(1, creature1.Power);
            Assert.AreEqual(1, creature1.Toughness);
            Assert.AreEqual(2, creature2.Power);
            Assert.AreEqual(2, creature2.Toughness);
        }

        #endregion

        #region Serras Embrace

        [Test]
        public void Test_Serras_Embrace_is_targeted()
        {
            Card creature = CreateCreature(m_playerA, 1, 1);

            Card card = CreateCard<SerrasEmbraceCardFactory>(m_playerA, "10E", "Serra's Embrace");
            card.Zone = m_game.Zones.Hand;

            Assert.AreEqual(2, card.Abilities.Count());
            EnchantAbility enchantAbility = card.Abilities.OfType<EnchantAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, enchantAbility));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Creature(), creature);
                Expect_PayManaCost(m_playerA, "2WW");
            }
            PlayAndResolve(m_playerA, enchantAbility);

            Assert.AreEqual(m_game.Zones.Battlefield, card.Zone);
            Assert.AreEqual(creature, card.AttachedTo);

            Assert.AreEqual(3, creature.Power);
            Assert.AreEqual(3, creature.Toughness);

            Assert.IsTrue(creature.HasAbility<FlyingAbility>());
            Assert.IsTrue(creature.HasAbility<VigilanceAbility>());

            card.Attach(null);

            Assert.AreEqual(1, creature.Power);
            Assert.AreEqual(1, creature.Toughness);

            Assert.IsFalse(creature.HasAbility<FlyingAbility>());
            Assert.IsFalse(creature.HasAbility<VigilanceAbility>());
        }

        #endregion

        #endregion

        #region Black

        #region Megrim

        [Test]
        public void Test_Cant_explicitely_play_Megrims_ability()
        {
            Card megrim = CreateCard<MegrimCardFactory>(m_playerA, "10E", "Megrim");
            Assert.AreEqual(Type.Enchantment, megrim.Type);

            megrim.Zone = m_game.Zones.Battlefield;

            var abilities = megrim.Abilities.ToList();

            Assert.AreEqual(2, abilities.Count);
            Assert.IsInstanceOf<PlayCardAbility>(abilities[0]);
            Assert.IsFalse(CanPlay(m_playerA, abilities[1]));
        }

        [Test]
        public void Test_Megrim_does_nothing_when_not_in_play()
        {
            Card discarded = CreateCard<BasicLandCardFactory>(m_playerB, "10E", "Forest");
            discarded.Zone = m_game.Zones.Hand;

            Card megrim = CreateCard<MegrimCardFactory>(m_playerA, "10E", "Megrim");
            megrim.Zone = m_game.Zones.Graveyard;

            m_playerB.Life = 20;
            m_playerB.Discard(discarded);

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(20, m_playerB.Life);
        }

        [Test]
        public void Test_When_Megrim_is_in_play_it_triggers_when_the_opponent_discards_a_card()
        {
            Card discarded = CreateCard<BasicLandCardFactory>(m_playerB, "10E", "Forest");
            discarded.Zone = m_game.Zones.Hand;

            Card megrim = CreateCard<MegrimCardFactory>(m_playerA, "10E", "Megrim");
            megrim.Zone = m_game.Zones.Battlefield;

            m_playerB.Life = 20;
            m_playerB.Discard(discarded);

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(18, m_playerB.Life);
        }

        [Test]
        public void Test_When_Megrim_is_in_play_it_does_not_trigger_when_the_player_discards_a_card()
        {
            Card discarded = CreateCard<BasicLandCardFactory>(m_playerA, "10E", "Forest");
            discarded.Zone = m_game.Zones.Hand;

            Card megrim = CreateCard<MegrimCardFactory>(m_playerA, "10E", "Megrim");
            megrim.Zone = m_game.Zones.Battlefield;

            m_playerA.Life = 20;
            m_playerA.Discard(discarded);

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(20, m_playerA.Life);
        }

        #endregion

        #endregion

        #region Red

        #region Firebreathing

        [Test]
        public void Test_Firebreathing()
        {
            Card creature = CreateCreature(m_playerB, 1, 1);

            Card card = CreateCard<FirebreathingCardFactory>(m_playerA, "10E", "Firebreathing");
            card.Zone = m_game.Zones.Hand;

            EnchantAbility enchantAbility = card.Abilities.OfType<EnchantAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, enchantAbility));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Creature(), creature);
                Expect_PayManaCost(m_playerA, "R");
            }
            PlayAndResolve(m_playerA, enchantAbility);

            Assert.AreEqual(1, creature.Power);
            Assert.AreEqual(1, creature.Toughness);

            InPlayAbility boostAbility = card.Abilities.OfType<InPlayAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, boostAbility));
            using (m_mockery.Ordered())
            {
                Expect_PayManaCost(m_playerA, "R");
            }
            PlayAndResolve(m_playerA, boostAbility);

            Assert.AreEqual(2, creature.Power);
            Assert.AreEqual(1, creature.Toughness);

            card.Attach(null);

            // Effect is still present if aura is detached
            Assert.AreEqual(2, creature.Power);
            Assert.AreEqual(1, creature.Toughness);
        }

        #endregion

        #endregion

        #region Blue

        #region Persuasion

        [Test]
        public void Test_Persuasion()
        {
            Card creature = CreateCreature(m_playerB, 1, 1);

            Card card = CreateCard<PersuasionCardFactory>(m_playerA, "10E", "Persuasion");
            card.Zone = m_game.Zones.Hand;

            EnchantAbility enchantAbility = card.Abilities.OfType<EnchantAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, enchantAbility));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Creature(), creature);
                Expect_PayManaCost(m_playerA, "3UU");
            }
            PlayAndResolve(m_playerA, enchantAbility);

            Assert.AreEqual(m_game.Zones.Battlefield, card.Zone);
            Assert.AreEqual(creature, card.AttachedTo);
            Assert.AreEqual(m_playerA, creature.Controller);

            card.Attach(null);

            Assert.AreEqual(m_playerB, creature.Controller);
        }

        #endregion

        #endregion

        #endregion
    }
}
