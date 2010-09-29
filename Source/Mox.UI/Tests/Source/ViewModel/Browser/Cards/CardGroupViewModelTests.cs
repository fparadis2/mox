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
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class CardGroupViewModelTests
    {
        #region Variables

        private CardGroupViewModel m_cardGroupModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_cardGroupModel = CreateGroup(Type.Creature);
        }

        #endregion

        #region Utilities

        private static CardGroupViewModel CreateGroup(Type type)
        {
            var card = new CardDatabase().AddDummyCard("MyCard", type);
            return new CardGroupViewModel(card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Group_returns_the_pluralized_type()
        {
            Assert.AreEqual(CardGroup.Creatures, m_cardGroupModel.Group);
        }

        [Test]
        public void Test_DisplayName_returns_only_the_dominant_type()
        {
            Assert.AreEqual(CardGroup.Creatures, CreateGroup(Type.Artifact | Type.Creature | Type.Enchantment | Type.Instant | Type.Land | Type.Planeswalker | Type.Scheme | Type.Sorcery | Type.Tribal).Group);
            Assert.AreEqual(CardGroup.Spells, CreateGroup(Type.Artifact | Type.Scheme).Group);
            Assert.AreEqual(CardGroup.Lands, CreateGroup(Type.Land | Type.Scheme).Group);
            Assert.AreEqual(CardGroup.Misc, CreateGroup(Type.Scheme).Group);
        }

        [Test]
        public void Test_Equality()
        {
            var group1 = CreateGroup(Type.Creature);
            var group2 = CreateGroup(Type.Planeswalker);
            var group3 = CreateGroup(Type.Artifact);
            var group4 = CreateGroup(Type.Artifact | Type.Creature);

            Assert.AreCompletelyEqual(group1, group1, false);
            Assert.AreCompletelyEqual(group1, group2, false);
            Assert.AreCompletelyEqual(group1, group4, false);

            Assert.AreCompletelyNotEqual(group3, group4, false);
        }

        [Test]
        public void Test_Groups_are_sorted_in_a_specific_order()
        {
            Type[] expectedOrder = new[] { Type.Creature, Type.Enchantment, Type.Land, Type.Scheme };
            
            for (int x = 0; x < expectedOrder.Length; x++)
            {
                for (int y = 0; y < expectedOrder.Length; y++)
                {
                    var groupX = CreateGroup(expectedOrder[x]);
                    var groupY = CreateGroup(expectedOrder[y]);

                    Assert.AreEqual(x.CompareTo(y), groupX.CompareTo(groupY));
                    Assert.AreEqual(y.CompareTo(x), groupY.CompareTo(groupX));
                }
            }

        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("Creatures", m_cardGroupModel.ToString());
        }

        #endregion
    }
}