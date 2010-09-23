using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class PageViewModelTests
    {
        #region Variables

        private PageViewModel m_pageModel;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_pageModel = new PageViewModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
        }
        
        #endregion
    }
}
