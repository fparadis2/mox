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
    public class CardEditionViewModelTests
    {
        #region Variables

        private readonly DateTime m_now = DateTime.Now;
        private CardInstanceInfo m_cardPrint;
        private CardEditionViewModel m_cardPrintModel;

        #endregion

        #region Setup / Teardown

        [Test]
        public void Setup()
        {
            var database = new CardDatabase();
            CardInfo card = database.AddCard("My card", "R", SuperType.None, Type.Creature, new SubType[0], "0", "0", new string[0]);
            SetInfo set = database.AddSet("SET", "My Set", "My Block", m_now);
            m_cardPrint = database.AddCardInstance(card, set, Rarity.MythicRare, 3, "Joe");
            
            m_cardPrintModel = new CardEditionViewModel(m_cardPrint);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(Rarity.MythicRare, m_cardPrintModel.Rarity);
            Assert.AreEqual("My Set", m_cardPrintModel.SetName);
            Assert.AreEqual("My Block", m_cardPrintModel.BlockName);
            Assert.AreEqual(m_now.Year, m_cardPrintModel.ReleaseYear);
            Assert.AreEqual("My Set (Mythic Rare)", m_cardPrintModel.ToolTip);
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("My card (My Set)", m_cardPrintModel.ToString());
        }

        #endregion
    }
}