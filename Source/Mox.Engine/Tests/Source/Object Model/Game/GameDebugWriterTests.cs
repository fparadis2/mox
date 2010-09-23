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
using System.Text;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class GameDebugWriterTests
    {
        #region Variables

        private Game m_game;
        private Player m_playerPatterson;
        private Player m_playerJohnson;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_game = new Game();
            m_playerPatterson = m_game.CreatePlayer();
            m_playerPatterson.Name = "Patterson";
            m_playerPatterson.Life = 18;

            m_playerJohnson = m_game.CreatePlayer();
            m_playerJohnson.Name = "Johnson";
            m_playerJohnson.Life = 9;

            m_game.State.CurrentTurn = 13;
        }

        #endregion

        #region Utilities

        private void Assert_Output_Is(string expectedOutput)
        {
            string actual = new GameDebugWriter(m_game).ToString();

            Assert.IsNotEmpty(actual);
            Assert.That(!actual.EndsWith(Environment.NewLine), "Shouldn't end with a new line");

            Assert.AreEqual(expectedOutput.Trim(), actual.Trim());
        }

        private Card CreateCard(Player owner, string name, Zone zone)
        {
            Card card = m_game.CreateCard(owner, new CardIdentifier { Card = name });
            card.Zone = zone;
            return card;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new GameDebugWriter(null));
        }

        [Test]
        public void Test_Output_with_no_cards()
        {
            Assert_Output_Is(@"
--- Turn 13 ---------------------------------------------------------------
[Patterson     18]=[Library:  0]=[Graveyard:  0]
        [Hand:  0]
        [Play:  0]
[Johnson        9]=[Library:  0]=[Graveyard:  0]
        [Hand:  0]
        [Play:  0]
            ");
        }

        [Test]
        public void Test_Name_is_truncated_if_too_long()
        {
            m_playerPatterson.Name += "ThisIsTooLong";

            Assert_Output_Is(@"
--- Turn 13 ---------------------------------------------------------------
[PattersonThis 18]=[Library:  0]=[Graveyard:  0]
        [Hand:  0]
        [Play:  0]
[Johnson        9]=[Library:  0]=[Graveyard:  0]
        [Hand:  0]
        [Play:  0]
            ");
        }

        [Test]
        public void Test_Output_with_some_cards()
        {
            for (int i = 0; i < 15; i++)
            {
                CreateCard(m_playerPatterson, "Graveyard", m_game.Zones.Graveyard);
                CreateCard(m_playerJohnson, "Library", m_game.Zones.Library);
            }

            CreateCard(m_playerPatterson, "Card 1", m_game.Zones.Hand);
            CreateCard(m_playerPatterson, "Card 2", m_game.Zones.Hand);
            CreateCard(m_playerPatterson, "The Beast", m_game.Zones.Hand);
            CreateCard(m_playerPatterson, "A longer card name", m_game.Zones.Hand);
            CreateCard(m_playerPatterson, "Loxodon Mystic", m_game.Zones.Hand);

            CreateCard(m_playerPatterson, "Plains", m_game.Zones.Battlefield);
            CreateCard(m_playerPatterson, "I'm on a boat", m_game.Zones.Battlefield);

            CreateCard(m_playerJohnson, "Card XYZ", m_game.Zones.Battlefield);

            Assert_Output_Is(@"
--- Turn 13 ---------------------------------------------------------------
[Patterson     18]=[Library:  0]=[Graveyard: 15]
        [Hand:  5] A longer card name             Card 1
                   Card 2                         Loxodon Mystic
                   The Beast
        [Play:  2] I'm on a boat                  Plains
[Johnson        9]=[Library: 15]=[Graveyard:  0]
        [Hand:  0]
        [Play:  1] Card XYZ
            ");
        }

        [Test]
        public void Test_Creature_power_and_toughness_are_written()
        {
            Card creature1 = CreateCard(m_playerPatterson, "Creature 1", m_game.Zones.Hand);
            creature1.Type = Type.Creature;
            creature1.Power = 10;
            creature1.Toughness = 3;

            Card creature2 = CreateCard(m_playerPatterson, "Creature 2", m_game.Zones.Battlefield);
            creature2.Type = Type.Creature;
            creature2.Power = 3;
            creature2.Toughness = 5;

            Card creature3 = CreateCard(m_playerPatterson, "Creature 3", m_game.Zones.Battlefield);
            creature3.Type = Type.Creature;
            creature3.Power = 30;
            creature3.Toughness = 50;

            Card creature4 = CreateCard(m_playerJohnson, "Creature 4", m_game.Zones.Battlefield);
            creature4.Type = Type.Creature;
            creature4.Power = 10;
            creature4.Toughness = 3;

            Assert_Output_Is(@"
--- Turn 13 ---------------------------------------------------------------
[Patterson     18]=[Library:  0]=[Graveyard:  0]
        [Hand:  1] Creature 1
        [Play:  2] Creature 2 (3/5)               Creature 3 (30/50)
[Johnson        9]=[Library:  0]=[Graveyard:  0]
        [Hand:  0]
        [Play:  1] Creature 4 (10/3)
            ");
        }

        #endregion
    }
}
