using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class ImportDeckViewModelTests
    {
        #region Variables

        private ImportDeckViewModel m_model;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            CardDatabase database = new CardDatabase();
            database.AddDummyCard("Forest");

            m_model = new ImportDeckViewModel(database);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_Text()
        {
            m_model.Text = "My test";
            Assert.AreEqual("My test", m_model.Text);
        }

        [Test]
        public void Test_Can_import_if_Text_forms_valid_deck()
        {
            Assert.IsFalse(m_model.CanImport);

            m_model.Text = "INVALID";
            Assert.IsFalse(m_model.CanImport);

            m_model.Text = "5 Forest";
            Assert.IsTrue(m_model.CanImport);
        }

        [Test]
        public void Test_Error_returns_the_error_if_cannot_import()
        {
            Assert.IsNullOrEmpty(m_model.Error);

            m_model.Text = "INVALID";
            Assert.IsNotEmpty(m_model.Error);

            m_model.Text = "5 Forest";
            Assert.IsNullOrEmpty(m_model.Error);
        }

        [Test]
        public void Test_Import_imports_the_deck()
        {
            m_model.Text = "5 Forest";
            Deck deck = m_model.Import();

            Assert.IsNotNull(deck);
            Assert.AreEqual(1, deck.Cards.Keys.Count);
        }

        #endregion
    }
}