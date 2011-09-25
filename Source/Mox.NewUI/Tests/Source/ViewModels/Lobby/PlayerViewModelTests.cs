using System;
using Mox.Lobby;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class PlayerViewModelTests
    {
        #region Variables

        private Mox.Lobby.Player m_player;
        private PlayerViewModel m_model;
        private UserViewModel m_user;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            User user = new User("John");
            m_player = new Mox.Lobby.Player(user);
            m_user = new UserViewModel(user);

            m_model = new PlayerViewModel(m_player, m_user);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_player.Id, m_model.Id);
            Assert.AreEqual(m_user, m_model.User);
        }

        [Test]
        public void Test_Can_set_User()
        {
            var newUser = new UserViewModel(new User("Jack"));
            Assert.ThatProperty(m_model, p => p.User).SetValue(newUser).RaisesChangeNotification();
            Assert.AreEqual(newUser, m_model.User);
        }

        #endregion
    }
}
