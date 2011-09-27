using System;
using Mox.Lobby;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class UserViewModelTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        #endregion

        #region Tests

        [Test]
        public void Test_Name()
        {
            var userViewModel = new UserViewModel(new User("Joe"));
            Assert.AreEqual("Joe", userViewModel.Name);
        }

        [Test]
        public void Test_IsAI()
        {
            var userViewModel = new UserViewModel(User.CreateAIUser());
            Assert.IsTrue(userViewModel.IsAI);
        }

        [Test]
        public void Test_INotifyPropertyChanged_implementation()
        {
            Assert.ThatAllPropertiesOn(new UserViewModel(new User("John"))).RaiseChangeNotification();
        }

        #endregion
    }
}
