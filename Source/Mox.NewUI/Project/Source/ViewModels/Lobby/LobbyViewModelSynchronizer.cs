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
        private readonly IDispatcher m_dispatcher;

        private readonly KeyedUserCollection m_usersById = new KeyedUserCollection();
        private readonly KeyedPlayerCollection m_playersById = new KeyedPlayerCollection();

        #endregion

        #region Constructor

        public LobbyViewModelSynchronizer(LobbyViewModel lobbyViewModel, ILobby lobby, IDispatcher dispatcher)
        {
            Throw.IfNull(lobbyViewModel, "lobbyViewModel");
            Throw.IfNull(lobby, "lobby");
            Throw.IfNull(dispatcher, "dispatcher");

            m_lobbyViewModel = lobbyViewModel;
            m_lobby = lobby;
            m_dispatcher = dispatcher;

            m_lobby.UserChanged += m_lobby_UserChanged;
            m_lobby.PlayerChanged += m_lobby_PlayerChanged;
        }

        public void Dispose()
        {
            m_lobby.PlayerChanged -= m_lobby_PlayerChanged;
            m_lobby.UserChanged -= m_lobby_UserChanged;
        }

        #endregion

        #region Methods

        private bool TryGetUserViewModel(User user, out UserViewModel userViewModel)
        {
            return m_usersById.TryGetValue(user.Id, out userViewModel);
        }

        private void WhenUserJoin(User user)
        {
            var userViewModel = new UserViewModel(user);
            m_usersById.Add(userViewModel);
            m_lobbyViewModel.Users.Add(userViewModel);
        }

        private void WhenUserLeave(User user)
        {
            UserViewModel userViewModel;
            if (m_usersById.TryGetValue(user.Id, out userViewModel))
            {
                m_usersById.Remove(user.Id);
                m_lobbyViewModel.Users.Remove(userViewModel);
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

        void m_lobby_UserChanged(object sender, UserChangedEventArgs e)
        {
            m_dispatcher.BeginInvokeIfNeeded(() =>
            {
                switch (e.Change)
                {
                    case UserChange.Joined:
                        WhenUserJoin(e.User);
                        break;

                    case UserChange.Left:
                        WhenUserLeave(e.User);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            });
        }

        void m_lobby_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            m_dispatcher.BeginInvokeIfNeeded(() =>
            {
                switch (e.Change)
                {
                    case PlayerChange.Joined:
                        WhenPlayerJoin(e.Player);
                        break;

                    case PlayerChange.Left:
                        WhenPlayerLeave(e.Player);
                        break;

                    case PlayerChange.Changed:
                        WhenPlayerChange(e.Player);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            });
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
