﻿using System;
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

        private DeckListPartViewModel m_model;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_viewModelServices = MockViewModelServices.Use(m_mockery);
            
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

            m_viewModelServices.Expect_Push<INavigationViewModel<MoxWorkspace>>(m_model, model =>
            {
                Assert.IsInstanceOf<EditDeckPageViewModel>(model);
                EditDeckPageViewModel page = (EditDeckPageViewModel)model;
                Assert.AreEqual(m_libraryViewModel, page.DeckLibrary);
                Assert.AreEqual(deckToEdit, page.EditedDeck);
                Assert.That(deckToEdit.IsEditing);
            });

            using (m_mockery.Test())
            {
                m_model.Edit(deckToEdit);
            }
        }

        [Test]
        public void Test_CreateDeck()
        {
            DeckViewModel deck = null;

            m_viewModelServices.Expect_Push<INavigationViewModel<MoxWorkspace>>(m_model, model =>
            {
                Assert.IsInstanceOf<EditDeckPageViewModel>(model);
                EditDeckPageViewModel page = (EditDeckPageViewModel)model;
                Assert.AreEqual(m_libraryViewModel, page.DeckLibrary);
                deck = page.EditedDeck;
            });

            using (m_mockery.Test())
            {
                DeckViewModel newDeckViewModel = m_model.CreateDeck();
                Assert.IsNotNull(newDeckViewModel);
                Assert.AreEqual(newDeckViewModel, deck);
            }
        }

        #endregion
    }
}
