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
    public class DeckTests
    {
        #region Variables

        private Deck m_deck;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_deck = new Deck("My deck");
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.IsEmpty(m_deck.Cards);
            Assert.AreEqual("My deck", m_deck.Name);
        }

        [Test]
        public void Test_Can_get_set_Description()
        {
            m_deck.Description = "My Description";
            Assert.AreEqual("My Description", m_deck.Description);
        }

        #endregion
    }
}
