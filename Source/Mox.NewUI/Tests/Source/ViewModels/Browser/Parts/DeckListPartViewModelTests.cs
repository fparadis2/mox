using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class DeckListPartViewModelTests : DeckLibraryViewModelTestsBase
    {
        #region Variables

        private MockViewModelServices m_viewModelServices;
        private MockRepository m_mockery;

        private INavigationConductor<INavigationViewModel<MoxWorkspace>> m_conductor;

        private DeckListPartViewModel m_model;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_viewModelServices = MockViewModelServices.Use();
            m_mockery = new MockRepository();
            m_conductor = m_mockery.StrictMock<INavigationConductor<INavigationViewModel<MoxWorkspace>>>();

            m_model = new DeckListPartViewModel(m_libraryViewModel);
        }

        [TearDown]
        public void TearDown()
        {
            DisposableHelper.SafeDispose(m_viewModelServices);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Edit()
        {
            var deckToEdit = m_libraryViewModel.Decks.First();

            m_viewModelServices.Expect_FindParent(m_model, m_conductor);

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
                m_model.Edit(deckToEdit);
            }
        }

        #endregion
    }
}
