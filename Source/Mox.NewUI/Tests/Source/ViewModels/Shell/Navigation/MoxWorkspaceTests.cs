using System;

using NUnit.Framework;

namespace Mox.UI.Shell
{
    [TestFixture]
    public class MoxWorkspaceTests
    {
        #region Variables

        private MoxWorkspace m_workspace;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_workspace = new MoxWorkspace();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_All_properties_trigger_change_notification()
        {
            Assert.ThatAllPropertiesOn(m_workspace).RaiseChangeNotification();
        }

        #endregion
    }
}
