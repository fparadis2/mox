using System;

using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class BrowseDecksPageViewModelTests
    {
        #region Variables

        private BrowseDecksPageViewModel m_page;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_page = new BrowseDecksPageViewModel();
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
