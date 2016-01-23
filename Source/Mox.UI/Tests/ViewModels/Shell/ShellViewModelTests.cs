using System;
using NUnit.Framework;

namespace Mox.UI.Shell
{
    [TestFixture]
    public class ShellViewModelTests
    {
        #region Variables

        private ShellViewModel m_model;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_model = new ShellViewModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            //Assert.IsInstanceOf<MainMenuViewModel>(m_model.ActiveItem);
        }

        #endregion
    }
}
