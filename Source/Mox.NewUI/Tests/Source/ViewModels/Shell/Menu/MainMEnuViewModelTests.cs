using System;
using NUnit.Framework;

namespace Mox.UI.Shell
{
    [TestFixture]
    public class MainMenuViewModelTests
    {
        #region Variables

        private MainMenuViewModel m_model;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_model = new MainMenuViewModel(null);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Selecting_an_item_makes_it_Selected()
        {
            MainMenuItemViewModel item = new MainMenuItemViewModel(null);
            m_model.Items.Add(item);

            Assert.ThatProperty(m_model, m => m.SelectedItem).SetValue(item).RaisesChangeNotification();
            Assert.IsTrue(item.IsSelected);

            Assert.ThatProperty(m_model, m => m.SelectedItem).SetValue(null).RaisesChangeNotification();
            Assert.IsFalse(item.IsSelected);
        }

        #endregion
    }
}
