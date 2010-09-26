using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class EditDeckPageViewModelTests
    {
        #region Variables

        private EditDeckPageViewModel m_pageModel;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            CardDatabase database = new CardDatabase();
            DeckLibrary library = new DeckLibrary();

            m_pageModel = new EditDeckPageViewModel(library, database, new Deck());
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual("Edit Deck", m_pageModel.Title);
            Assert.IsNotNull(m_pageModel.Editor);
            Assert.IsNotNull(m_pageModel.Editor.Database);
        }

        [Test]
        public void Test_Can_get_set_IsDirty()
        {
            Assert.IsFalse(m_pageModel.IsDirty);
            m_pageModel.IsDirty = true;
            Assert.IsTrue(m_pageModel.IsDirty);
        }

        [Test]
        public void Test_Cannot_go_forward_if_not_dirty()
        {
            m_pageModel.IsDirty = false;
            Assert.IsFalse(m_pageModel.CanGoForward);

            m_pageModel.IsDirty = true;
            Assert.IsTrue(m_pageModel.CanGoForward);
        }
        
        #endregion
    }
}
