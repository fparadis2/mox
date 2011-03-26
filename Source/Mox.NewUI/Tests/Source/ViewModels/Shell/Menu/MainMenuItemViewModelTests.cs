using System;
using NUnit.Framework;

namespace Mox.UI.Shell
{
    [TestFixture]
    public class MainMenuItemViewModelTests
    {
        #region Variables

        private MainMenuItemViewModel m_model;
        private bool m_activated;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_activated = false;
            m_model = new MainMenuItemViewModel(() => m_activated = true);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Property_Change_Notification()
        {
            Assert.ThatAllPropertiesOn(m_model).RaiseChangeNotification();
        }

        [Test]
        public void Test_Activate_executes_the_action()
        {
            m_model.Activate();
            Assert.That(m_activated);
        }

        #endregion
    }
}
