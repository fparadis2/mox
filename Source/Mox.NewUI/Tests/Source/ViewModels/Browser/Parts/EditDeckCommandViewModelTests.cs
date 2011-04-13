﻿using System;
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

            m_editor.IsEnabled = true;
            m_command = new EditDeckCommandPartViewModel(new DeckLibraryViewModel(m_library, m_editor), new DeckViewModel(m_deck, m_editor));
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
            }
        }

        [Test]
        public void Test_Cancel_asks_before_popping_the_navigation_conductor_when_dirty()
        {
            m_editor.IsDirty = true;
            m_mockMessageService.Expect_Show("Cancel", "Caption", MessageBoxButton.OKCancel, MessageBoxResult.OK);
            m_viewModelServices.Expect_PopParent(m_command);

            using (m_mockery.Test())
            {
                m_command.Cancel();
            }
        }

        [Test]
        public void Test_Cancel_does_nothing_if_user_chooses_to_return()
        {
            m_editor.IsDirty = true;
            m_mockMessageService.Expect_Show("Cancel", "Caption", MessageBoxButton.OKCancel, MessageBoxResult.Cancel);

            using (m_mockery.Test())
            {
                m_command.Cancel();
            }
        }

        [Test]
        public void Test_Save_pops_the_navigation_conductor()
        {
            m_viewModelServices.Expect_PopParent(m_command);

            using (m_mockery.Test())
            {
                m_command.Cancel();
            }
        }

        [Test]
        public void Test_GoForward_saves_the_decks()
        {
            Assert.IsFalse(m_library.Decks.Contains(m_deck), "Sanity check");

            m_command.Save();

            Assert.That(m_library.Decks.Contains(m_deck));
        }

        #endregion
    }
}
