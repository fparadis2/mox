using System;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyViewModel_DesignTime : LobbyViewModel
    {
        #region Constructor

        public LobbyViewModel_DesignTime()
        {
            User user1 = new User("John");
            User user2 = new User("Jack Sparrow");
            User aiUser = User.CreateAIUser();

            UserViewModel userViewModel1 = new UserViewModel(user1);
            UserViewModel userViewModel2 = new UserViewModel(user2);
            UserViewModel aiUserViewModel = new UserViewModel(aiUser);

            Users.Add(userViewModel1);
            Users.Add(userViewModel2);

            Players.Add(new PlayerViewModel(new Mox.Lobby.Player(user1), userViewModel1));
            Players.Add(new PlayerViewModel(new Mox.Lobby.Player(user2), userViewModel2));
            Players.Add(new PlayerViewModel(new Mox.Lobby.Player(aiUser), aiUserViewModel));
        }

        #endregion
    }
}
