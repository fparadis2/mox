using System;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class DeckCardViewModelTests : DeckViewModelTestsBase
    {
        #region Variables

        private DeckCardViewModel m_model;

        #endregion

        #region Setup

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            DeckViewModel owner = new DeckViewModel(m_editor, m_deck);

            m_model = new DeckCardViewModel(owner, m_card1);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(3, m_model.Quantity);
        }

        [Test]
        public void Test_Can_change_quantity()
        {
            m_editor.IsEnabled = true;
            m_model.Quantity = 7;
            Assert.AreEqual(7, m_model.Quantity);
            Assert.AreEqual(7, m_deck.Cards[m_card1]);
        }

        [Test]
        public void Test_Setting_an_invalid_quantity_does_nothing()
        {
            m_editor.IsEnabled = true;
            m_model.Quantity = 0;
            Assert.AreEqual(3, m_model.Quantity);

            m_model.Quantity = -1;
            Assert.AreEqual(3, m_model.Quantity);
        }

        [Test]
        public void Test_Cannot_change_properties_when_not_enabled()
        {
            m_editor.IsEnabled = false;
            Assert.Throws<InvalidOperationException>(() => m_model.Quantity = 2);
        }

        #endregion
    }
}
