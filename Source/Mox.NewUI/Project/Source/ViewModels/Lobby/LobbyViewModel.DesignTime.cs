using System;
using Mox.Database;
using Mox.Lobby;
using Mox.UI.Browser;

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

            DeckViewModelEditor deckEditor = new DeckViewModelEditor(new CardDatabase(), null);
            
            PlayerViewModel player1 = new PlayerViewModel(new Mox.Lobby.Player(user1), userViewModel1) { Deck = CreateDeckViewModel("Combo Deck", deckEditor) };
            PlayerViewModel player2 = new PlayerViewModel(new Mox.Lobby.Player(user2), userViewModel2) { Deck = CreateDeckViewModel("Aggro Deck", deckEditor) }; ;
            PlayerViewModel player3 = new PlayerViewModel(new Mox.Lobby.Player(aiUser), aiUserViewModel) { Deck = CreateDeckViewModel("Creature Deck", deckEditor) }; ;

            Players.Add(player1);
            Players.Add(player2);
            Players.Add(player3);
        }

        private static DeckViewModel CreateDeckViewModel(string deckName, IDeckViewModelEditor deckEditor)
        {
            Deck deck = new Deck { Name = deckName };
            return new DeckViewModel(deck, deckEditor);
        }

        #endregion
    }
}
