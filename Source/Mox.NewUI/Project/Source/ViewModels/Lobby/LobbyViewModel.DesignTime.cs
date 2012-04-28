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

            var deck1 = new Deck { Name = "Combo Deck" };
            var deck2 = new Deck { Name = "Creature Deck" };

            Mox.Lobby.Player player1 = new Mox.Lobby.Player(user1, new PlayerData { Deck = deck1 });
            Mox.Lobby.Player player2 = new Mox.Lobby.Player(user2);
            Mox.Lobby.Player player3 = new Mox.Lobby.Player(aiUser, new PlayerData { Deck = deck2 });

            PlayerViewModel playerViewModel1 = new PlayerViewModel(player1, userViewModel1);
            PlayerViewModel playerViewModel2 = new PlayerViewModel(player2, userViewModel2);
            PlayerViewModel playerViewModel3 = new PlayerViewModel(player3, aiUserViewModel);

            Players.Add(playerViewModel1);
            Players.Add(playerViewModel2);
            Players.Add(playerViewModel3);

            FillDeckList(deck1, deck2);
        }

        private void FillDeckList(params Deck[] decks)
        {
            foreach (var player in Players)
            {
                foreach (var deck in decks)
                {
                    player.DeckList.DecksSource.Add(new DeckChoiceViewModel(deck));
                }
            }
        }

        #endregion
    }
}
