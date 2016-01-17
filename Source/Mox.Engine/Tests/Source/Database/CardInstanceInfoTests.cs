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

namespace Mox.Database
{
    [TestFixture]
    public class CardInstanceInfoTests
    {
        #region Variables

        private CardDatabase m_database;

        private SetInfo m_set;
        private CardInfo m_card;
        private CardInstanceInfo m_instance;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_database = new CardDatabase();

            m_set = m_database.AddSet("THESET", "My Set", "Block", DateTime.Now);
            m_card = m_database.AddCard("My Card", "R", Color.Red, SuperType.None, Type.Artifact, new SubType[0], "0", "0", null);

            m_instance = new CardInstanceInfo(m_card, m_set, 8, Rarity.Rare, 3, "Roger");
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_database, m_instance.Database);
            Assert.AreEqual(m_card, m_instance.Card);
            Assert.AreEqual(m_set, m_instance.Set);
            Assert.AreEqual(8, m_instance.Index);
            Assert.AreEqual(Rarity.Rare, m_instance.Rarity);
            Assert.AreEqual(3, m_instance.MultiverseId);
            Assert.AreEqual("Roger", m_instance.Artist);
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new CardInstanceInfo(null, m_set, 0, Rarity.Common, 0, string.Empty); });
            Assert.Throws<ArgumentNullException>(delegate { new CardInstanceInfo(m_card, null, 0, Rarity.Common, 0, string.Empty); });
        }

        [Test]
        public void Test_Can_convert_to_CardIdentifier()
        {
            CardIdentifier identifier = m_instance;
            Assert.AreEqual("My Card", identifier.Card);
            Assert.AreEqual("THESET", identifier.Set);
            Assert.AreEqual(3, identifier.MultiverseId);
        }

        #endregion
    }
}
