// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Windows;
using Mox.Database;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class EditDeckPageViewModelTests
    {
        #region Variables

        private DeckLibrary m_library;
        private Deck m_deck;

        private EditDeckPageViewModel m_pageModel;

        private MockRepository m_mockery;

        private MockGameFlow m_gameFlow;
        private MockMessageService m_messageService;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_gameFlow = MockGameFlow.Use();
            m_messageService = MockMessageService.Use(m_mockery);

            CardDatabase database = new CardDatabase();
            m_library = new DeckLibrary();
            m_deck = new Deck();

            var editor = new MockDeckViewModelEditor(DesignTimeCardDatabase.Instance);
            m_pageModel = new EditDeckPageViewModel(new DeckLibraryViewModel(m_library, editor), database, m_deck);

            m_gameFlow.PushPage<bool>();
            m_gameFlow.PushPage<int>();
        }

        [TearDown]
        public void Teardown()
        {
            DisposableHelper.SafeDispose(m_messageService);
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
            Assert.IsNotNull(m_pageModel.DeckViewModel);
            Assert.AreEqual(m_deck, m_pageModel.DeckViewModel.Deck);
            Assert.IsNotNull(m_pageModel.CardLibraryViewModel);
        }

        [Test]
        public void Test_Cannot_go_forward_if_not_dirty()
        {
            m_pageModel.Editor.IsDirty = false;
            Assert.IsFalse(m_pageModel.CanGoForward);

            m_pageModel.Editor.IsDirty = true;
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

        [Test]
        public void Test_GoBack_goes_back_navigation_wise()
        {
            m_mockery.Test(() => m_pageModel.GoBack());
            m_gameFlow.Assert_Content_Is<bool>();
        }

        [Test]
        public void Test_GoBack_asks_for_confirmation_if_dirty()
        {
            m_pageModel.Editor.IsDirty = true;

            m_messageService.Expect_Show("Are you sure you want to discard the changes made to this deck?", "Discard changes?", MessageBoxButton.OKCancel, MessageBoxResult.Cancel);
            m_mockery.Test(() => m_pageModel.GoBack());
            m_gameFlow.Assert_Content_Is<int>();

            m_messageService.Expect_Show("Are you sure you want to discard the changes made to this deck?", "Discard changes?", MessageBoxButton.OKCancel, MessageBoxResult.OK);
            m_mockery.Test(() => m_pageModel.GoBack());
            m_gameFlow.Assert_Content_Is<bool>();
        }
        
        #endregion
    }
}
