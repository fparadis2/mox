using System;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class LobbyViewModelTests
    {
        #region Variables

        private LobbyViewModel m_viewModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_viewModel = new LobbyViewModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.IsEmpty(m_viewModel.Users);
            Assert.Collections.IsEmpty(m_viewModel.Players);
        }

        #endregion
    }
}
