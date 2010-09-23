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
    public class CardInfoTests
    {
        #region Variables

        private CardDatabase m_database;
        private CardInfo m_card;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_database = new CardDatabase();
            m_card = new CardInfo(m_database, "My Name", "3RW", SuperType.Basic, Type.Creature, new[] { SubType.Forest, SubType.Island }, "3", "4", new[] { "A", "B" });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_database, m_card.Database);

            Assert.AreEqual("My Name", m_card.Name);
            Assert.AreEqual("3RW", m_card.ManaCost);

            Assert.AreEqual(SuperType.Basic, m_card.SuperType);
            Assert.AreEqual(Type.Creature, m_card.Type);
            Assert.Collections.AreEqual(new[] { SubType.Forest, SubType.Island }, m_card.SubTypes);

            Assert.AreEqual(3, m_card.Power);
            Assert.AreEqual(4, m_card.Toughness);

            Assert.Collections.AreEqual(new[] { "A", "B" }, m_card.Abilities);
            Assert.IsTrue(m_card.Abilities.IsReadOnly);
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new CardInfo(null, "Name", "3RW", SuperType.Basic, Type.Creature, new SubType[0], "3", "4", null); });
            Assert.Throws<ArgumentException>(delegate { new CardInfo(null, "Name", "3RW", SuperType.Basic, Type.None, new SubType[0], "3", "4", null); });
        }

        [Test]
        public void Test_Color_of_a_card_is_color_of_the_mana_cost()
        {
            Assert.AreEqual(Color.Red | Color.White, m_card.Color);
        }

        [Test]
        public void Test_NormalizeCardName()
        {
            Assert.AreEqual(string.Empty, CardInfo.NormalizeCardName(string.Empty));
            Assert.AreEqual(null, CardInfo.NormalizeCardName(null));
            Assert.AreEqual("ABC", CardInfo.NormalizeCardName("ABC"));
            Assert.AreEqual("ABC_DEF", CardInfo.NormalizeCardName("ABC DEF"));
            Assert.AreEqual("ABCDEF", CardInfo.NormalizeCardName("ABC'D,EF"));
        }

        [Test]
        public void Test_ToOracleString()
        {
            Assert.AreEqual(
@"My Name
3RW
Basic Creature - Forest Island
3/4
A
B",
m_card.ToOracleString());
        }

        [Test]
        public void Test_Supports_weird_power_and_toughness()
        {
            m_card = new CardInfo(m_database, "My Name", "R", SuperType.None, Type.Creature, new[] { SubType.Antelope }, "1+*", "1+*", new[] { "A", "B" });

            Assert.AreEqual(-1, m_card.Power);
            Assert.AreEqual(-1, m_card.Toughness);

            Assert.AreEqual(
@"My Name
R
Creature - Antelope
1+*/1+*
A
B",
m_card.ToOracleString());
        }

        [Test]
        public void Test_TypeLine_returns_the_complete_type_line()
        {
            m_card = new CardInfo(m_database, "My Name", "R", SuperType.Basic | SuperType.Legendary, Type.Creature, new[] { SubType.Antelope }, "1+*", "1+*", new[] { "A", "B" });
            Assert.AreEqual("Basic Legendary Creature - Antelope", m_card.TypeLine);

            m_card = new CardInfo(m_database, "My Name", "R", SuperType.None, Type.Land, new[] { SubType.Antelope, SubType.Ape }, "1+*", "1+*", new[] { "A", "B" });
            Assert.AreEqual("Land - Antelope Ape", m_card.TypeLine);

            m_card = new CardInfo(m_database, "My Name", "R", SuperType.None, Type.Land | Type.Creature, new SubType[0], "1+*", "1+*", new[] { "A", "B" });
            Assert.AreEqual("Creature Land", m_card.TypeLine);
        }

        #endregion
    }
}
