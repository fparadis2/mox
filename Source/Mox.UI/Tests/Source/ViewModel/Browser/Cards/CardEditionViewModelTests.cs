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