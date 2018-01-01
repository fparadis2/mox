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
using Mox.Events;
using Mox.Replication;
using NUnit.Framework;

namespace Mox.Database.Sets
{
    [TestFixture]
    public class Factory10ECreaturesTests : BaseFactoryTests
    {
        #region Utilities

        private Card CreateVanillaCreature(Player owner, int power, int toughness)
        {
            Card creature = CreateCard(owner);
            creature.Power = power;
            creature.Toughness = toughness;
            creature.Zone = m_game.Zones.Battlefield;
            creature.Type = Type.Creature;
            return creature;
        }

        #endregion

        #region Tests

        #region Vanilla

        [Test]
        public void Test_Vanilla_creature()
        {
            Card creatureCard = InitializeCard("Mass of Ghouls");
            Assert.AreEqual(Type.Creature, creatureCard.Type);

            Assert.AreEqual(5, creatureCard.Power);
            Assert.AreEqual(3, creatureCard.Toughness);

            Assert.AreEqual(1, creatureCard.Abilities.Count());
            Assert.IsInstanceOf<PlayCardAbility>(creatureCard.Abilities.First());

            Assert.IsTrue(CanPlay(m_playerA, creatureCard.Abilities.First()));

            Expect_PayManaCost(m_playerA, "3BB");
            PlayAndResolve(m_playerA, creatureCard.Abilities.First());

            Assert.AreEqual(m_game.Zones.Battlefield, creatureCard.Zone);
        }

        #endregion

        #region White

        #region Angel of Mercy

        [Test]
        public void Test_Angel_of_Mercy_controller_gains_3_life_when_played()
        {
            Card creatureCard = InitializeCard("Angel of Mercy");

            PlayCardAbility playCardAbility = GetPlayCardAbility(creatureCard);

            Assert.IsTrue(CanPlay(m_playerA, playCardAbility));

            Expect_PayManaCost(m_playerA, "4W");
            PlayAndResolve(m_playerA, playCardAbility);

            Assert.AreEqual(m_game.Zones.Battlefield, creatureCard.Zone);

            int initialLife = m_playerA.Life;
            {
                Expect_AllPlayersPass(m_playerA);
                PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            }
            Assert.AreEqual(initialLife + 3, m_playerA.Life);
        }

        [Test]
        public void Test_Angel_of_Mercy_works_with_replication()
        {
            Game replicatedGame = m_game.Replicate();

            Player replicatedPlayerA = replicatedGame.Players.Single(p => p.Name == m_playerA.Name);

            Card creatureCard = InitializeCard("Angel of Mercy");

            PlayCardAbility playCardAbility = GetPlayCardAbility(creatureCard);

            Assert.IsTrue(CanPlay(m_playerA, playCardAbility));

            Expect_PayManaCost(m_playerA, "4W");
            PlayAndResolve(m_playerA, playCardAbility);

            Assert.AreEqual(m_game.Zones.Battlefield, creatureCard.Zone);

            int initialLife = replicatedPlayerA.Life;
            {
                Expect_AllPlayersPass(m_playerA);
                PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            }
            Assert.AreEqual(initialLife + 3, replicatedPlayerA.Life);
        }

        [Test]
        public void Test_Angel_of_Mercy_triggers_when_zone_is_changed()
        {
            Card creatureCard = InitializeCard("Angel of Mercy");
            creatureCard.Zone = m_game.Zones.Graveyard;

            creatureCard.Zone = m_game.Zones.Battlefield;

            int initialLife = m_playerA.Life;
            {
                Expect_AllPlayersPass(m_playerA);
                PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            }
            Assert.AreEqual(initialLife + 3, m_playerA.Life);
        }

        [Test]
        public void Test_Angel_of_Mercy_doesnt_trigger_when_controller_is_changed()
        {
            Card creatureCard = InitializeCard("Angel of Mercy");
            creatureCard.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            creatureCard.Controller = m_playerB;
            creatureCard.Controller = m_playerA;

            int initialLife = m_playerA.Life;
            {
                Expect_AllPlayersPass(m_playerA);
                PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            }
            Assert.AreEqual(initialLife, m_playerA.Life);
        }

        [Test]
        public void Test_Angel_of_Mercy_triggers_even_when_controlled_by_opponent()
        {
            Card creatureCard = InitializeCard("Angel of Mercy");
            creatureCard.Controller = m_playerB;
            creatureCard.Zone = m_game.Zones.Battlefield;

            int initialLife = m_playerB.Life;
            {
                Expect_AllPlayersPass(m_playerA);
                PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            }
            Assert.AreEqual(initialLife, m_playerB.Life);
        }

        #endregion

        #region Ancestor's Chosen

        [Test]
        public void Test_Ancestors_Chosen_controller_gains_life_when_played()
        {
            Card creatureCard = InitializeCard("Ancestor's Chosen");

            const int NumCardsInGraveyard = 5;
            for (int i = 0; i < NumCardsInGraveyard; i++)
            {
                CreateCard(m_playerA).Zone = m_game.Zones.Graveyard;
            }
            Assert.AreEqual(NumCardsInGraveyard, m_playerA.Graveyard.Count, "Sanity check");

            PlayCardAbility playCardAbility = GetPlayCardAbility(creatureCard);

            Assert.IsTrue(CanPlay(m_playerA, playCardAbility));

            Expect_PayManaCost(m_playerA, "5WW");
            PlayAndResolve(m_playerA, playCardAbility);

            Assert.AreEqual(m_game.Zones.Battlefield, creatureCard.Zone);

            int initialLife = m_playerA.Life;
            {
                Expect_AllPlayersPass(m_playerA);
                PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            }
            Assert.AreEqual(initialLife + NumCardsInGraveyard, m_playerA.Life);
        }

        #endregion

        #region Ghost Warden

        [Test]
        public void Test_Ghost_Warden_can_target_itself()
        {
            Card creatureCard = InitializeCard("Ghost Warden");
            creatureCard.Zone = m_game.Zones.Battlefield;

            Ability boostAbility = creatureCard.Abilities.OfType<InPlayAbility>().First();
            Assert.IsTrue(CanPlay(m_playerA, boostAbility));

            Expect_Target(m_playerA, TargetCost.Creature(), creatureCard);
            PlayAndResolve(m_playerA, boostAbility);

            Assert.That(creatureCard.Tapped);
            Assert.AreEqual(2, creatureCard.Toughness);
            Assert.AreEqual(2, creatureCard.Power);
        }

        #endregion

        #region Loxodon Mystic

        [Test]
        public void Test_Loxodon_Mystic_can_tap_itself()
        {
            Card creatureCard = InitializeCard("Loxodon Mystic");

            PlayCardAbility playCardAbility = GetPlayCardAbility(creatureCard);

            Assert.IsTrue(CanPlay(m_playerA, playCardAbility));

            Expect_PayManaCost(m_playerA, "3WW");
            PlayAndResolve(m_playerA, playCardAbility);

            Assert.AreEqual(m_game.Zones.Battlefield, creatureCard.Zone);

            Ability tapAbility = creatureCard.Abilities.OfType<InPlayAbility>().First();
            Assert.IsTrue(CanPlay(m_playerA, tapAbility));

            Expect_Target(m_playerA, TargetCost.Creature(), creatureCard);
            Expect_PayManaCost(m_playerA, "W");
            PlayAndResolve(m_playerA, tapAbility);

            Assert.That(creatureCard.Tapped);
        }

        [Test]
        public void Test_Loxodon_Mystic_wont_do_anything_if_its_target_becomes_tapped()
        {
            Card creatureCard = InitializeCard("Loxodon Mystic");
            Card anotherCreatureCard = InitializeCard("Loxodon Mystic");

            creatureCard.Zone = m_game.Zones.Battlefield;
            anotherCreatureCard.Zone = m_game.Zones.Battlefield;

            Ability tapAbility = creatureCard.Abilities.OfType<InPlayAbility>().First();
            Assert.IsTrue(CanPlay(m_playerA, tapAbility));

            Expect_Target(m_playerA, TargetCost.Creature(), anotherCreatureCard);
            Expect_PayManaCost(m_playerA, "W");

            Play(m_playerA, tapAbility);

            Assert.That(!anotherCreatureCard.Tapped);
            anotherCreatureCard.Tap();
            Assert.That(anotherCreatureCard.Tapped);

            Expect_AllPlayersPass(m_playerA);
            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.That(anotherCreatureCard.Tapped);
        }

        #endregion

        #endregion

        #region Red

        #region Bloodfire Colossus

        [Test]
        public void Test_Bloodfire_Colossus()
        {
            m_playerA.Life = 20;
            m_playerB.Life = 20;

            Card otherCreature1 = CreateVanillaCreature(m_playerA, 3, 3);
            Card otherCreature2 = CreateVanillaCreature(m_playerB, 10, 10);

            Card creatureCard = InitializeCard("Bloodfire Colossus");
            creatureCard.Zone = m_game.Zones.Battlefield;

            Ability sacrificeAbility = creatureCard.Abilities.OfType<InPlayAbility>().First();
            Assert.IsTrue(CanPlay(m_playerA, sacrificeAbility));

            {
                Expect_PayManaCost(m_playerA, "R");
            }
            PlayAndResolve(m_playerA, sacrificeAbility);

            Assert.AreEqual(m_game.Zones.Graveyard, creatureCard.Zone);

            Assert.AreEqual(14, m_playerA.Life);
            Assert.AreEqual(14, m_playerB.Life);

            Assert.AreEqual(6, otherCreature1.Damage);
            Assert.AreEqual(6, otherCreature2.Damage);
        }

        #endregion

        #region Bogordan Firefiend

        [Test]
        public void Test_Bogardan_Firefiend_triggers_when_zone_is_changed()
        {
            Card otherCreature = CreateCard(m_playerA);
            otherCreature.Toughness = 10;
            otherCreature.Zone = m_game.Zones.Battlefield;
            otherCreature.Type = Type.Creature;

            Card creatureCard = InitializeCard("Bogardan Firefiend");
            creatureCard.Zone = m_game.Zones.Battlefield;
            creatureCard.Zone = m_game.Zones.Graveyard;
            
            Expect_Target(m_playerA, TargetCost.Creature(), otherCreature);
            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(2, otherCreature.Damage);
        }

        [Test]
        public void Test_Bogardan_Firefiend_works_with_replication()
        {
            Game replicatedGame = m_game.Replicate();
            
            Card otherCreature = CreateCard(m_playerA);
            otherCreature.Toughness = 10;
            otherCreature.Zone = m_game.Zones.Battlefield;
            otherCreature.Type = Type.Creature;

            Card creatureCard = InitializeCard("Bogardan Firefiend");
            creatureCard.Zone = m_game.Zones.Battlefield;

            Card replicatedCreature = replicatedGame.Cards.Single(c => c.Identifier == otherCreature.Identifier);

            creatureCard.Zone = m_game.Zones.Battlefield;
            creatureCard.Zone = m_game.Zones.Graveyard;

            Expect_Target(m_playerA, TargetCost.Creature(), otherCreature);
            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(2, replicatedCreature.Damage);
        }

        [Test]
        public void Test_Bogardan_Firefiend_doesnt_trigger_when_controller_is_changed()
        {
            Card creatureCard = InitializeCard("Bogardan Firefiend");
            creatureCard.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            creatureCard.Controller = m_playerB;
            creatureCard.Controller = m_playerA;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
        }

        [Test]
        public void Test_Bogardan_Firefiend_doesnt_trigger_when_not_coming_from_play()
        {
            Card creatureCard = InitializeCard("Bogardan Firefiend");
            creatureCard.Zone = m_game.Zones.Graveyard;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
        }

        [Test]
        public void Test_Bogardan_Firefiend_doesnt_trigger_when_staying_in_play_for_some_reason()
        {
            Card creatureCard = InitializeCard("Bogardan Firefiend");
            creatureCard.Zone = m_game.Zones.Battlefield;
            creatureCard.Zone = m_game.Zones.Graveyard;
            creatureCard.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
        }

        [Test]
        public void Test_Bogardan_Firefiend_triggers_even_when_controlled_by_opponent()
        {
            m_sequencerTester.MockPlayerChoices(m_playerB);

            Card otherCreature = CreateCard(m_playerA);
            otherCreature.Toughness = 10;
            otherCreature.Zone = m_game.Zones.Battlefield;
            otherCreature.Type = Type.Creature;

            Card creatureCard = InitializeCard("Bogardan Firefiend");
            creatureCard.Zone = m_game.Zones.Battlefield;
            creatureCard.Controller = m_playerB;

            creatureCard.Zone = m_game.Zones.Graveyard;

            Expect_Target(m_playerB, TargetCost.Creature(), otherCreature);
            Expect_AllPlayersPass();
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.AreEqual(2, otherCreature.Damage);
        }

        #endregion

        #region Rage Weaver

        [Test]
        public void Test_Rage_Weaver()
        {
            List<Card> coloredCards = new List<Card>();
            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                Card coloredCard = CreateCard(m_playerA);
                coloredCard.Color = color;
                coloredCard.Zone = m_game.Zones.Battlefield;
                coloredCard.Type = Type.Creature;
                coloredCards.Add(coloredCard);
            }

            Card creatureCard = InitializeCard("Rage Weaver");
            creatureCard.Zone = m_game.Zones.Battlefield;

            Ability tapAbility = creatureCard.Abilities.OfType<InPlayAbility>().First();
            Assert.IsTrue(CanPlay(m_playerA, tapAbility));

            Card greenCreature = coloredCards.Single(c => c.Color == Color.Green);
            Expect_Target(m_playerA, TargetCost.Creature().OfAnyColor(Color.Green | Color.Black), greenCreature);
            Expect_PayManaCost(m_playerA, "2");
            PlayAndResolve(m_playerA, tapAbility);

            Assert.IsTrue(greenCreature.HasAbility<HasteAbility>());
            m_game.Events.Trigger(new EndOfTurnEvent(m_playerA));
            Assert.IsFalse(greenCreature.HasAbility<HasteAbility>());
        }

        #endregion

        #region Viashino Sandscout

        [Test]
        public void Test_Viashino_Sandscout()
        {
            Card creatureCard = InitializeCard("Viashino Sandscout");
            creatureCard.Zone = m_game.Zones.Battlefield;

            m_game.Events.Trigger(new EndOfTurnEvent(m_playerA));
            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            Assert.AreEqual(m_game.Zones.Hand, creatureCard.Zone);
        }

        [Test]
        public void Test_Viashino_Sandscout_does_nothing_if_card_is_not_in_play()
        {
            Card creatureCard = InitializeCard("Viashino Sandscout");
            creatureCard.Zone = m_game.Zones.Graveyard;

            m_game.Events.Trigger(new EndOfTurnEvent(m_playerA));
            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            Assert.AreEqual(m_game.Zones.Graveyard, creatureCard.Zone);
        }

        #endregion

        #endregion

        #region Black

        [Test]
        public void Test_Basic_Nightmare()
        {
            Card nightmare = InitializeCard("Nightmare");
            nightmare.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(0, nightmare.Power);
            Assert.AreEqual(0, nightmare.Toughness);
        }

        [Test]
        public void Test_Nightmare()
        {
            Card nightmare = InitializeCard("Nightmare");
            nightmare.Zone = m_game.Zones.Battlefield;

            Card swamp1 = InitializeCard("Swamp");
            Card swamp2 = InitializeCard("Swamp");

            Assert.AreEqual(0, nightmare.Power);
            Assert.AreEqual(0, nightmare.Toughness);

            swamp1.Zone = m_game.Zones.Battlefield;
            swamp2.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(2, nightmare.Power);
            Assert.AreEqual(2, nightmare.Toughness);

            nightmare.Zone = m_game.Zones.Exile; // Still works in other zones (CDA)

            Assert.AreEqual(2, nightmare.Power);
            Assert.AreEqual(2, nightmare.Toughness);

            swamp1.Controller = m_playerB;

            Assert.AreEqual(1, nightmare.Power);
            Assert.AreEqual(1, nightmare.Toughness);

            swamp2.SubTypes = new SubTypes(SubType.Mountain);

            Assert.AreEqual(0, nightmare.Power);
            Assert.AreEqual(0, nightmare.Toughness);
        }

        #endregion

        #region Blue

        #region Ambassador Laquatus

        [Test]
        public void Test_Ambassador_Laquatus()
        {
            Card dummyCard = CreateCard(m_playerB); dummyCard.Zone = m_game.Zones.Library;

            Card libraryCard1 = CreateCard(m_playerB, "Library1");
            Card libraryCard2 = CreateCard(m_playerB, "Library2");
            Card libraryCard3 = CreateCard(m_playerB, "Library3");
            var libraryCards = new[] {libraryCard1, libraryCard2, libraryCard3};

            Assert.Collections.AreEqual(new[] { libraryCard3, libraryCard2, libraryCard1 }, m_playerB.Library.Top(3)); // Sanity check

            Card creatureCard = InitializeCard("Ambassador Laquatus");
            creatureCard.Zone = m_game.Zones.Battlefield;

            Ability millAbility = creatureCard.Abilities.OfType<InPlayAbility>().First();
            Assert.IsTrue(CanPlay(m_playerA, millAbility));

            Expect_Target(m_playerA, TargetCost.Player(), m_playerB);
            Expect_PayManaCost(m_playerA, "3");
            PlayAndResolve(m_playerA, millAbility);

            Assert.Collections.AreEqual(libraryCards, m_playerB.Graveyard.Top(3));
        }

        #endregion

        #endregion

        #endregion
    }
}
