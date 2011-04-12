using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class BrowseDecksPageViewModelTests : DeckLibraryViewModelTestsBase
    {
        #region Variables

        private MockViewModelServices m_viewModelServices;
        private MockRepository m_mockery;

        private INavigationConductor<INavigationViewModel<MoxWorkspace>> m_conductor;

        private BrowseDecksPageViewModel m_page;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_viewModelServices = MockViewModelServices.Use();
            m_mockery = new MockRepository();
            m_conductor = m_mockery.StrictMock<INavigationConductor<INavigationViewModel<MoxWorkspace>>>();

            m_page = new BrowseDecksPageViewModel(m_libraryViewModel);
        }

        [TearDown]
        public void TearDown()
        {
            DisposableHelper.SafeDispose(m_viewModelServices);
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
            var deckToEdit = m_libraryViewModel.Decks.First();

            m_viewModelServices.Expect_FindParent(m_page, m_conductor);

            Expect.Call(m_conductor.Push(null)).Return(new MockPageHandle()).IgnoreArguments().Callback<INavigationViewModel<MoxWorkspace>>(model =>
            {
                Assert.IsInstanceOf<EditDeckPageViewModel>(model);
                EditDeckPageViewModel page = (EditDeckPageViewModel)model;
                Assert.AreEqual(m_libraryViewModel, page.DeckLibrary);
                Assert.AreEqual(deckToEdit.Deck, page.EditedDeck.Deck);
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
