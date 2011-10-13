using System;
using Mox.Database;
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

            DeckListViewModel deckList = new DeckListViewModel();

            PlayerViewModel player1 = new PlayerViewModel(deckList, new Mox.Lobby.Player(user1), userViewModel1);
            PlayerViewModel player2 = new PlayerViewModel(deckList, new Mox.Lobby.Player(user2), userViewModel2);
            PlayerViewModel player3 = new PlayerViewModel(deckList, new Mox.Lobby.Player(aiUser), aiUserViewModel);

            player1.SelectedDeck = new DeckViewModel(new Deck { Name = "Combo Deck" });
            player2.UseRandomDeck = true;
            player3.SelectedDeck = new DeckViewModel(new Deck { Name = "Creature Deck" });

            Players.Add(player1);
            Players.Add(player2);
            Players.Add(player3);
        }

        #endregion
    }
}
