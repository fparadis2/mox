using System;
using System.Linq;
using Mox.Database;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class BrowseDecksPageViewModelTests
    {
        #region Variables

        private MockViewModelServices m_viewModelServices;
        private MockRepository m_mockery;

        private INavigationConductor<INavigationViewModel<MoxWorkspace>> m_conductor;

        private BrowseDecksPageViewModel m_page;

        private DeckLibraryViewModel m_deckLibrary;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_deckLibrary = CreateLibrary();

            m_viewModelServices = MockViewModelServices.Use();
            m_mockery = new MockRepository();
            m_conductor = m_mockery.StrictMock<INavigationConductor<INavigationViewModel<MoxWorkspace>>>();

            m_page = new BrowseDecksPageViewModel(m_deckLibrary);
        }

        [TearDown]
        public void TearDown()
        {
            DisposableHelper.SafeDispose(m_viewModelServices);
        }

        private static DeckLibraryViewModel CreateLibrary()
        {
            var deckLibrary = new DeckLibrary();

            Deck deck1 = new Deck { Name = "Super Deck" };
            Deck deck2 = new Deck { Name = "Ordinary Deck" };

            deckLibrary.Save(deck1);
            deckLibrary.Save(deck2);

            return new DeckLibraryViewModel(deckLibrary, new DeckViewModelEditor(new CardDatabase(), null));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Fill()
        {
            MoxWorkspace workspace = new MoxWorkspace();
            m_page.Fill(workspace);

            Assert.IsInstanceOf<DeckListPartViewModel>(workspace.LeftView);
            Assert.IsInstanceOf<DeckContentPartViewModel>(workspace.CenterView);
            Assert.IsInstanceOf<InfoPanelPartViewModel>(workspace.RightView);
            Assert.IsInstanceOf<BrowseDecksCommandPartViewModel>(workspace.CommandView);

            Assert.IsNull(workspace.BottomView);
        }

        [Test]
        public void Test_Edit()
        {
            var deckToEdit = m_deckLibrary.Decks.First();

            m_viewModelServices.Expect_FindParent(m_page, m_conductor);

            m_conductor.Push(null);
            LastCall.IgnoreArguments().Callback<INavigationViewModel<MoxWorkspace>>(model =>
            {
                Assert.IsInstanceOf<EditDeckPageViewModel>(model);
                EditDeckPageViewModel page = (EditDeckPageViewModel)model;
                return true;
            });

            using (m_mockery.Test())
            {
                m_page.Edit(deckToEdit);
            }
        }

        #endregion
    }
}
