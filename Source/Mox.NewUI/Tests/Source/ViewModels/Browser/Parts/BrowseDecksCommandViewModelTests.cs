using System;

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

        private BrowseDecksCommandPartViewModel m_command;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_viewModelServices = MockViewModelServices.Use(m_mockery);

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
            m_viewModelServices.Expect_PopParent(m_command);

            using (m_mockery.Test())
            {
                m_command.GoBack();
            }
        }

        #endregion
    }
}
