using Mox.Database;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Browser
{
    public class DeckLibraryViewModelTestsBase
    {
        #region Variables

        protected MockRepository m_mockery;
        protected MockMessageService m_messageService;

        protected DeckLibrary m_library;
        protected DeckViewModelEditor m_editor;
        protected DeckLibraryViewModel m_libraryViewModel;

        #endregion

        #region Setup

        [SetUp]
        public virtual void Setup()
        {
            m_mockery = new MockRepository();
            m_messageService = MockMessageService.Use(m_mockery);

            m_library = new DeckLibrary();

            Deck deck1 = new Deck { Name = "Super Deck" };
            Deck deck2 = new Deck { Name = "Ordinary Deck" };

            m_library.Save(deck1);
            m_library.Save(deck2);

            m_editor = new DeckViewModelEditor(new CardDatabase(), null);

            m_libraryViewModel = new DeckLibraryViewModel(m_library, m_editor);
        }

        [TearDown]
        public virtual void Teardown()
        {
            m_messageService.Dispose();
        }

        #endregion
    }
}