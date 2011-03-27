using System;
using Mox.UI.Shell;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class BrowseDecksCommandPartViewModelTests
    {
        #region Variables

        private MockViewModelServices m_viewModelServices;
        private MockRepository m_mockery;

        private IShellViewModel m_shell;
        private BrowseDecksCommandPartViewModel m_command;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_viewModelServices = MockViewModelServices.Use();

            m_mockery = new MockRepository();
            m_shell = m_mockery.StrictMock<IShellViewModel>();

            m_command = new BrowseDecksCommandPartViewModel();
        }

        [TearDown]
        public void TearDown()
        {
            DisposableHelper.SafeDispose(m_viewModelServices);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_GoBack_pops_the_navigation_conductor()
        {
            m_viewModelServices.Expect_FindParent<IShellViewModel>(m_command, m_shell);

            m_shell.Pop();

            using (m_mockery.Test())
            {
                m_command.GoBack();
            }
        }

        #endregion
    }
}
