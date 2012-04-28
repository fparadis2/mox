using System;
using System.Collections.ObjectModel;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyViewModelSynchronizer : IDisposable
    {
        #region Variables

        private readonly LobbyViewModel m_lobbyViewModel;
        private readonly ILobby m_lobby;

        private readonly KeyedUserCollection m_usersById = new KeyedUserCollection();
        private readonly KeyedPlayerCollection m_playersById = new KeyedPlayerCollection();

        #endregion

        #region Constructor

        public LobbyViewModelSynchronizer(LobbyViewModel lobbyViewModel, ILobby lobby)
        {
            Throw.IfNull(lobbyViewModel, "lobbyViewModel");
            Throw.IfNull(lobby, "lobby");

            m_lobbyViewModel = lobbyViewModel;
            m_lobby = lobby;

            m_lobbyViewModel.Chat.ChatService = m_lobby.Chat;

            m_lobby.Users.CollectionChanged += Users_CollectionChanged;
            m_lobby.Users.ForEach(u => WhenUserJoin(u, true));

            m_lobby.Players.CollectionChanged += Players_CollectionChanged;
            m_lobby.Players.ItemChanged += Players_ItemChanged;
            m_lobby.Players.ForEach(WhenPlayerJoin);

            m_lobby.Chat.MessageReceived += Chat_MessageReceived;
        }

        public void Dispose()
        {
            m_lobby.Chat.MessageReceived -= Chat_MessageReceived;
            m_lobby.Players.CollectionChanged -= Players_CollectionChanged;
            m_lobby.Players.ItemChanged -= Players_ItemChanged;
            m_lobby.Users.CollectionChanged -= Users_CollectionChanged;
        }

        #endregion

        #region Methods

        private bool TryGetUserViewModel(User user, out UserViewModel userViewModel)
        {
            return m_usersById.TryGetValue(user.Id, out userViewModel);
        }

        private void WhenUserJoin(User user, bool initial)
        {
            var userViewModel = new UserViewModel(user);
            m_usersById.Add(userViewModel);
            m_lobbyViewModel.Users.Add(userViewModel);

            if (!initial)
            {
                AppendChatText(string.Format("[User {0} joined]", user.Name));
            }
        }

        private void WhenUserLeave(User user)
        {
            UserViewModel userViewModel;
            if (m_usersById.TryGetValue(user.Id, out userViewModel))
            {
                m_usersById.Remove(user.Id);
                m_lobbyViewModel.Users.Remove(userViewModel);
                AppendChatText(string.Format("[User {0} left]", user.Name));
            }
        }

        private void WhenPlayerJoin(Mox.Lobby.Player player)
        {
            var userViewModel = GetUserViewModel(player.User);
            var playerViewModel = new PlayerViewModel(player, userViewModel);
            m_playersById.Add(playerViewModel);
            m_lobbyViewModel.Players.Add(playerViewModel);
        }

        private void WhenPlayerLeave(Mox.Lobby.Player player)
        {
            PlayerViewModel playerViewModel;
            if (m_playersById.TryGetValue(player.Id, out playerViewModel))
            {
                m_playersById.Remove(player.Id);
                m_lobbyViewModel.Players.Remove(playerViewModel);
            }
        }

        private void WhenPlayerChange(Mox.Lobby.Player player)
        {
            PlayerViewModel playerViewModel;
            if (m_playersById.TryGetValue(player.Id, out playerViewModel))
            {
                var userViewModel = GetUserViewModel(player.User);
                playerViewModel.SyncFromPlayer(player, userViewModel);
            }
        }

        private UserViewModel GetUserViewModel(User user)
        {
            UserViewModel userViewModel;
            if (!TryGetUserViewModel(user, out userViewModel))
            {
                userViewModel = new UserViewModel(user); // For users representing AIs
            }
            return userViewModel;
        }

        #endregion

        #region Event Handlers

        void Users_CollectionChanged(object sender, Collections.CollectionChangedEventArgs<User> e)
        {
            e.Synchronize(u => WhenUserJoin(u, false), WhenUserLeave);
        }

        void Players_CollectionChanged(object sender, Collections.CollectionChangedEventArgs<Mox.Lobby.Player> e)
        {
            e.Synchronize(WhenPlayerJoin, WhenPlayerLeave);
        }

        void Players_ItemChanged(object sender, ItemEventArgs<Mox.Lobby.Player> e)
        {
            WhenPlayerChange(e.Item);
        }

        void Chat_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            string message = string.Format("{0}: {1}", e.User.Name, e.Message);
            AppendChatText(message);
        }

        private void AppendChatText(string message)
        {
            if (string.IsNullOrEmpty(m_lobbyViewModel.Chat.Text))
            {
                m_lobbyViewModel.Chat.Text = message;
            }
            else
            {
                m_lobbyViewModel.Chat.Text += Environment.NewLine + message;
            }
        }

        #endregion

        #region Inner Types

        private class KeyedUserCollection : KeyedCollection<Guid, UserViewModel>
        {
            protected override Guid GetKeyForItem(UserViewModel item)
            {
                return item.Id;
            }
        }

        private class KeyedPlayerCollection : KeyedCollection<Guid, PlayerViewModel>
        {
            protected override Guid GetKeyForItem(PlayerViewModel item)
            {
                return item.Id;
            }
        }

        #endregion
    }
}
