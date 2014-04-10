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

namespace Mox.Database
{
    [TestFixture]
    public class CardDatabaseTests
    {
        #region Variables

        private CardDatabase m_database;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_database = new CardDatabase();
        }

        #endregion

        #region Utilities

        #endregion

        #region Tests

        [Test]
        public void Test_Cards_are_readonly()
        {
            ICollection<CardInfo> cards = m_database.Cards;
            Assert.That(cards.IsReadOnly);
        }

        [Test]
        public void Test_Cards_can_be_added()
        {
            CardInfo cardInfo = m_database.AddCard("A", string.Empty, SuperType.None, Type.Creature, new SubType[0], null, null, null);
            Assert.Collections.Contains(cardInfo, m_database.Cards);
            Assert.AreEqual(cardInfo, m_database.Cards["A"]);
        }

        [Test]
        public void Test_Sets_are_readonly()
        {
            ICollection<SetInfo> sets = m_database.Sets;
            Assert.That(sets.IsReadOnly);
        }

        [Test]
        public void Test_Sets_can_be_added()
        {
            SetInfo setInfo = m_database.AddSet("ABC", "My Set", "MyBlock", DateTime.Now);
            Assert.Collections.Contains(setInfo, m_database.Sets);
            Assert.AreEqual(setInfo, m_database.Sets["ABC"]);
        }

        [Test]
        public void Test_Instances_can_be_added()
        {
            CardInfo cardInfo = m_database.AddCard("A", string.Empty, SuperType.None, Type.Creature, new SubType[0], null, null, null);
            SetInfo setInfo = m_database.AddSet("ABC", "My Set", "MyBlock", DateTime.Now);

            CardInstanceInfo instanceInfo = m_database.AddCardInstance(cardInfo, setInfo, Rarity.MythicRare, 4, "Artist");
            Assert.Collections.Contains(instanceInfo, cardInfo.Instances);
            Assert.Collections.Contains(instanceInfo, setInfo.CardInstances);
        }

        [Test]
        public void Test_CardInstance_can_be_retrieved_using_any_valid_identifier()
        {
            CardInfo cardInfo = m_database.AddCard("A", string.Empty, SuperType.None, Type.Creature, new SubType[0], null, null, null);

            SetInfo setInfo1 = m_database.AddSet("ABC", "My Set", "MyBlock", DateTime.Now);
            SetInfo setInfo2 = m_database.AddSet("DEF", "My Set 2", "MyBlock", DateTime.Now);

            CardInstanceInfo instanceInfo1 = m_database.AddCardInstance(cardInfo, setInfo1, Rarity.MythicRare, 4, "Artist");
            CardInstanceInfo instanceInfo2 = m_database.AddCardInstance(cardInfo, setInfo1, Rarity.MythicRare, 5, "Artist 2");
            CardInstanceInfo instanceInfo3 = m_database.AddCardInstance(cardInfo, setInfo2, Rarity.MythicRare, 6, "Artist");

            Assert.Throws<ArgumentException>(() => m_database.GetCardInstance(new CardIdentifier()));
            Assert.IsNull(m_database.GetCardInstance(new CardIdentifier { Card = "Invalid" }));

            Assert.AreEqual(instanceInfo2, m_database.GetCardInstance(new CardIdentifier { MultiverseId = 5 }));
            Assert.Collections.Contains(m_database.GetCardInstance(new CardIdentifier { Card = "A" }), new[] { instanceInfo1, instanceInfo2, instanceInfo3 });
            Assert.Collections.Contains(m_database.GetCardInstance(new CardIdentifier { Card = "A", Set = "ABC" }), new[] { instanceInfo1, instanceInfo2 });
        }

        #endregion
    }

    public static class CardDatabaseTestExtensions
    {
        #region Methods

        public static CardInfo AddDummyCard(this CardDatabase database, string cardName)
        {
            return AddDummyCard(database, cardName, Type.Creature);
        }

        public static CardInfo AddDummyCard(this CardDatabase database, string cardName, Type type)
        {
            return database.AddCard(cardName, "R", SuperType.None, type, new SubType[0], "1", "1", null);
        }

        #endregion
    }
}
