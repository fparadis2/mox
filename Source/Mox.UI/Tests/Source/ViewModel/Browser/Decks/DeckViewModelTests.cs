using System;
using System.Linq;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class DeckViewModelTests : DeckViewModelTestsBase
    {
        #region Variables

        private DeckViewModel m_deckModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_deckModel = new DeckViewModel(m_editor, m_deck);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual("My Super Deck", m_deckModel.Name);
            Assert.AreEqual("Frank", m_deckModel.Author);
        }

        [Test]
        public void Test_Can_access_cards()
        {
            Assert.AreEqual(2, m_deckModel.Cards.Count);
            var card1 = m_deckModel.Cards.Single(c => c.Name == m_card1.Card);
            Assert.AreEqual(3, card1.Quantity);

            var card2 = m_deckModel.Cards.Single(c => c.Name == m_card2.Card);
            Assert.AreEqual(1, card2.Quantity);
        }

        [Test]
        public void Test_Can_get_set_IsSelected()
        {
            Assert.IsFalse(m_deckModel.IsSelected);
            m_deckModel.IsSelected = true;
            Assert.IsTrue(m_deckModel.IsSelected);
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("My Super Deck", m_deckModel.ToString());
        }

        [Test]
        public void Test_Can_change_name()
        {
            m_editor.IsEnabled = true;
            m_deckModel.Name = "My new name";
            Assert.AreEqual("My new name", m_deckModel.Name);
            Assert.AreEqual("My new name", m_deck.Name);
        }

        [Test]
        public void Test_Can_change_author()
        {
            m_editor.IsEnabled = true;
            m_deckModel.Author = "My new author";
            Assert.AreEqual("My new author", m_deckModel.Author);
            Assert.AreEqual("My new author", m_deck.Author);
        }

        [Test]
        public void Test_Can_change_description()
        {
            m_editor.IsEnabled = true;
            m_deckModel.Description = "My new description";
            Assert.AreEqual("My new description", m_deckModel.Description);
            Assert.AreEqual("My new description", m_deck.Description);
        }

        [Test]
        public void Test_Cannot_change_properties_when_not_enabled()
        {
            m_editor.IsEnabled = false;
            Assert.Throws<InvalidOperationException>(() => m_deckModel.Name = "My new name");
            Assert.Throws<InvalidOperationException>(() => m_deckModel.Author = "My new name");
            Assert.Throws<InvalidOperationException>(() => m_deckModel.Description = "My new name");
        }

        #endregion
    }
}