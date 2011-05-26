using System;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class DeckContentPartViewModelTests : DeckLibraryViewModelTestsBase
    {
        #region Variables

        private DeckContentPartViewModel m_model;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_model = new DeckContentPartViewModel(m_libraryViewModel);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_libraryViewModel, m_model.Library);
        }

        #endregion
    }
}
