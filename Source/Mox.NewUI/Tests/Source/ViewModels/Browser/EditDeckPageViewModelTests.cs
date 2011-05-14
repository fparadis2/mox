using System;
using System.Linq;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class EditDeckPageViewModelTests : DeckLibraryViewModelTestsBase
    {
        #region Variables

        private EditDeckPageViewModel m_page;

        #endregion

        #region Setup / Teardown
        
        public override void Setup()
        {
            base.Setup();

            var deckViewModel = m_libraryViewModel.Decks.First();
            m_page = new EditDeckPageViewModel(m_libraryViewModel, deckViewModel);
            Assert.That(deckViewModel.IsEditing);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.AreEqual(m_libraryViewModel, m_page.DeckLibrary);
            Assert.AreEqual(m_libraryViewModel.Decks.First(), m_page.EditedDeck);
        }

        [Test]
        public void Test_Fill()
        {
            MoxWorkspace workspace = new MoxWorkspace();
            m_page.Fill(workspace);

            Assert.IsInstanceOf<CardListPartViewModel>(workspace.LeftView);
            Assert.IsInstanceOf<EditDeckCommandPartViewModel>(workspace.CommandView);

            Assert.IsNull(workspace.CenterView);
            Assert.IsNull(workspace.RightView);
            Assert.IsNull(workspace.BottomView);
        }

        #endregion
    }
}
