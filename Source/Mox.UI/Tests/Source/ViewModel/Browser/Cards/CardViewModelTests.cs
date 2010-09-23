using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class CardViewModelTests
    {
        #region Variables

        private CardInfo m_card;
        private CardViewModel m_cardModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_card = new CardDatabase().AddCard("MyCard", "RBG", SuperType.Basic, Type.Artifact, new [] { SubType.Advisor, SubType.Antelope }, "*", "2", new[] { "Hello world!", "How you doin?" });
            var set1 = m_card.Database.AddSet("SET", "My Set", "A block", DateTime.Now);
            var older = m_card.Database.AddSet("OLDER", "Older Set", "A block", DateTime.Now.Subtract(TimeSpan.FromDays(2)));
            m_card.Database.AddCardInstance(m_card, older, Rarity.Common, 3, "Hello");
            m_card.Database.AddCardInstance(m_card, set1, Rarity.Rare, 3, "Hello");
            m_cardModel = new CardViewModel(m_card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual("MyCard", m_cardModel.Name);
            Assert.AreEqual("Basic Artifact - Advisor Antelope", m_cardModel.TypeLine);
            Assert.AreEqual("RBG", m_cardModel.ManaCost);
            Assert.AreEqual("Hello world!" + Environment.NewLine + "How you doin?", m_cardModel.Rules);

            Assert.AreEqual(2, m_cardModel.Editions.Count);
            Assert.IsTrue(m_cardModel.HasMoreThanOneEdition);
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("MyCard", m_cardModel.ToString());
        }

        [Test]
        public void Test_CurrentEdition_returns_the_latest_edition_by_default()
        {
            var latestEdition = m_cardModel.CurrentEdition;
            Assert.AreEqual("My Set", latestEdition.SetName);
        }

        [Test]
        public void Test_can_set_CurrentEdition()
        {
            m_cardModel.CurrentEdition = m_cardModel.Editions[1];
            Assert.AreEqual(m_cardModel.Editions[1], m_cardModel.CurrentEdition);
        }

        #endregion
    }
}