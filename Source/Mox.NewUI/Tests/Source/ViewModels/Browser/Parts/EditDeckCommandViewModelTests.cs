using System;
using System.Windows;
using Mox.Database;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class EditDeckCommandPartViewModelTests : DeckViewModelTestsBase
    {
        #region Variables

        private MockViewModelServices m_viewModelServices;
        private MockRepository m_mockery;
        private MockMessageService m_mockMessageService;

        private EditDeckCommandPartViewModel m_command;

        private DeckLibrary m_library;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_mockery = new MockRepository();

            m_mockMessageService = MockMessageService.Use(m_mockery);
            m_viewModelServices = MockViewModelServices.Use(m_mockery);

            m_library = new DeckLibrary();

            m_command = new EditDeckCommandPartViewModel(new DeckLibraryViewModel(m_library, m_editor), m_deckViewModel);
            Assert.IsTrue(m_deckViewModel.IsEditing);
        }

        [TearDown]
        public void TearDown()
        {
            DisposableHelper.SafeDispose(m_viewModelServices);
            DisposableHelper.SafeDispose(m_mockMessageService);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Cancel_pops_the_navigation_conductor_if_not_dirty()
        {
            m_editor.IsDirty = false;
            m_viewModelServices.Expect_PopParent(m_command);

            using (m_mockery.Test())
            {
                m_command.Cancel();
                Assert.IsFalse(m_deckViewModel.IsEditing);
            }
        }

        [Test]
        public void Test_Cancel_asks_before_popping_the_navigation_conductor_when_dirty()
        {
            m_editor.IsDirty = true;
            m_mockMessageService.Expect_Show("Are you sure you want to discard the changes made to this deck?", "Discard changes?", MessageBoxButton.OKCancel, MessageBoxResult.OK);
            m_viewModelServices.Expect_PopParent(m_command);

            using (m_mockery.Test())
            {
                m_command.Cancel();
                Assert.IsFalse(m_deckViewModel.IsEditing);
            }
        }

        [Test]
        public void Test_Cancel_does_nothing_if_user_chooses_to_return()
        {
            m_editor.IsDirty = true;
            m_mockMessageService.Expect_Show("Are you sure you want to discard the changes made to this deck?", "Discard changes?", MessageBoxButton.OKCancel, MessageBoxResult.Cancel);

            using (m_mockery.Test())
            {
                m_command.Cancel();
                Assert.IsTrue(m_deckViewModel.IsEditing);
            }
        }

        [Test]
        public void Test_Save_pops_the_navigation_conductor()
        {
            m_viewModelServices.Expect_PopParent(m_command);

            using (m_mockery.Test())
            {
                m_command.Cancel();
                Assert.IsFalse(m_deckViewModel.IsEditing);
            }
        }

        [Test]
        public void Test_Save_saves_the_decks()
        {
            m_viewModelServices.Expect_PopParent(m_command);

            Assert.IsFalse(m_library.Decks.Contains(Deck), "Sanity check");

            using (m_mockery.Test())
            {
                m_command.Save();
                Assert.IsFalse(m_deckViewModel.IsEditing);
            }

            Assert.That(m_library.Decks.Contains(Deck));
        }

        #endregion
    }
}
