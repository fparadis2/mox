using System;
using Mox.UI.Shell;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class EditDeckCommandPartViewModelTests
    {
        #region Variables

        private MockViewModelServices m_viewModelServices;
        private MockRepository m_mockery;

        private INavigationConductor m_conductor;
        private EditDeckCommandPartViewModel m_command;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_viewModelServices = MockViewModelServices.Use();

            m_mockery = new MockRepository();
            m_conductor = m_mockery.StrictMock<INavigationConductor>();

            m_command = new EditDeckCommandPartViewModel();
        }

        [TearDown]
        public void TearDown()
        {
            DisposableHelper.SafeDispose(m_viewModelServices);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Cancel_pops_the_navigation_conductor()
        {
            m_viewModelServices.Expect_FindParent<INavigationConductor>(m_command, m_conductor);

            m_conductor.Pop();

            using (m_mockery.Test())
            {
                m_command.Cancel();
            }
        }

        [Test]
        public void Test_Save_pops_the_navigation_conductor()
        {
            m_viewModelServices.Expect_FindParent<INavigationConductor>(m_command, m_conductor);

            m_conductor.Pop();

            using (m_mockery.Test())
            {
                m_command.Cancel();
            }
        }

        #endregion
    }
}
