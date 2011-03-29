using System;

using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class EditDeckPageViewModelTests
    {
        #region Variables

        private EditDeckPageViewModel m_page;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_page = new EditDeckPageViewModel();
        }

        #endregion

        #region Tests

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
