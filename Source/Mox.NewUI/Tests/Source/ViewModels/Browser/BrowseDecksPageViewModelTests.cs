using System;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class BrowseDecksPageViewModelTests : DeckLibraryViewModelTestsBase
    {
        #region Variables

        private BrowseDecksPageViewModel m_page;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_page = new BrowseDecksPageViewModel(m_libraryViewModel);
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

        #endregion
    }
}
