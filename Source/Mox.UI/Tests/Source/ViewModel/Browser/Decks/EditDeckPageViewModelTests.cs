using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class EditDeckPageViewModelTests
    {
        #region Variables

        private DeckLibrary m_library;
        private Deck m_deck;

        private EditDeckPageViewModel m_pageModel;

        private MockGameFlow m_gameFlow;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_gameFlow = MockGameFlow.Use();

            CardDatabase database = new CardDatabase();
            m_library = new DeckLibrary();
            m_deck = new Deck();

            m_pageModel = new EditDeckPageViewModel(m_library, database, m_deck);

            m_gameFlow.PushPage<bool>();
            m_gameFlow.PushPage<int>();
        }

        [TearDown]
        public void Teardown()
        {
            DisposableHelper.SafeDispose(m_gameFlow);
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

        [Test]
        public void Test_GoForward_goes_back_navigation_wise()
        {
            m_pageModel.GoForward();
            m_gameFlow.Assert_Content_Is<bool>();
        }

        [Test]
        public void Test_GoForward_saves_the_decks()
        {
            Assert.IsFalse(m_library.Decks.Contains(m_deck), "Sanity check");

            m_pageModel.GoForward();

            Assert.That(m_library.Decks.Contains(m_deck));
        }
        
        #endregion
    }
}
