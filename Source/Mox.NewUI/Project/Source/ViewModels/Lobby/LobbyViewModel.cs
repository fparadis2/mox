using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyViewModel : PropertyChangedBase
    {
        #region Variables
        
        private readonly UserViewModelCollection m_users;
        private readonly PlayerViewModelCollection m_players;

        #endregion

        #region Constructor

        public LobbyViewModel(ILobby lobby, IDispatcher dispatcher)
        {
            Throw.IfNull(lobby, "lobby");
            Throw.IfNull(dispatcher, "dispatcher");

            m_users = new UserViewModelCollection(lobby, dispatcher);
            m_players = new PlayerViewModelCollection(lobby, dispatcher, m_users);
        }

        #endregion

        #region Properties

        public IList<UserViewModel> Users
        {
            get { return m_users.Users; }
        }

        public IList<PlayerViewModel> Players
        {
            get { return m_players.Players; }
        }

        #endregion

        #region Methods

        #endregion

        #region Inner Types

        private class UserViewModelCollection
        {
            #region Variables

            private readonly IDispatcher m_dispatcher;
            private readonly ObservableCollection<UserViewModel> m_users = new ObservableCollection<UserViewModel>();
            private readonly KeyedUserCollection m_usersById = new KeyedUserCollection();

            #endregion

            #region Constructor

            public UserViewModelCollection(ILobby lobby, IDispatcher dispatcher)
            {
                Throw.IfNull(lobby, "lobby");
                Throw.IfNull(dispatcher, "dispatcher");

                m_dispatcher = dispatcher;

                WeakEvent.Attach<UserViewModelCollection, UserChangedEventArgs>(
                    h => lobby.UserChanged += h,
                    h => lobby.UserChanged -= h,
                    this,
                    (l, e) => l.m_lobby_UserChanged(lobby, e));
            }

            #endregion

            #region Properties

            public IList<UserViewModel> Users
            {
                get { return m_users; }
            }

            #endregion

            #region Methods

            public bool TryGetValue(User user, out UserViewModel userViewModel)
            {
                return m_usersById.TryGetValue(user.Id, out userViewModel);
            }

            private void WhenUserJoin(User user)
            {
                var userViewModel = new UserViewModel(user);
                m_usersById.Add(userViewModel);
                m_users.Add(userViewModel);
            }

            private void WhenUserLeave(User user)
            {
                UserViewModel userViewModel;
                if (m_usersById.TryGetValue(user.Id, out userViewModel))
                {
                    m_usersById.Remove(user.Id);
                    m_users.Remove(userViewModel);
                }
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

            #endregion

            #region Inner Types

            private class KeyedUserCollection : KeyedCollection<Guid, UserViewModel>
            {
                protected override Guid GetKeyForItem(UserViewModel item)
                {
                    return item.Id;
                }
            }

            #endregion
        }

        private class PlayerViewModelCollection
        {
            #region Variables

            private readonly IDispatcher m_dispatcher;
            private readonly UserViewModelCollection m_users;
            private readonly ObservableCollection<PlayerViewModel> m_players = new ObservableCollection<PlayerViewModel>();
            private readonly KeyedPlayerCollection m_playersById = new KeyedPlayerCollection();

            #endregion

            #region Constructor

            public PlayerViewModelCollection(ILobby lobby, IDispatcher dispatcher, UserViewModelCollection users)
            {
                Throw.IfNull(lobby, "lobby");
                Throw.IfNull(dispatcher, "dispatcher");
                Throw.IfNull(users, "users");

                m_dispatcher = dispatcher;
                m_users = users;

                WeakEvent.Attach<PlayerViewModelCollection, PlayerChangedEventArgs>(
                    h => lobby.PlayerChanged += h,
                    h => lobby.PlayerChanged -= h,
                    this,
                    (l, e) => l.m_lobby_PlayerChanged(lobby, e));
            }

            #endregion

            #region Properties

            public IList<PlayerViewModel> Players
            {
                get { return m_players; }
            }

            #endregion

            #region Methods

            private void WhenPlayerJoin(Mox.Lobby.Player player)
            {
                var userViewModel = GetUserViewModel(player.User);
                var playerViewModel = new PlayerViewModel(player, userViewModel);
                m_playersById.Add(playerViewModel);
                m_players.Add(playerViewModel);
            }

            private void WhenPlayerLeave(Mox.Lobby.Player player)
            {
                PlayerViewModel playerViewModel;
                if (m_playersById.TryGetValue(player.Id, out playerViewModel))
                {
                    m_playersById.Remove(player.Id);
                    m_players.Remove(playerViewModel);
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
                if (!m_users.TryGetValue(user, out userViewModel))
                {
                    userViewModel = new UserViewModel(user); // For users representing AIs
                }
                return userViewModel;
            }

            #endregion

            #region Event Handlers

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

            private class KeyedPlayerCollection : KeyedCollection<Guid, PlayerViewModel>
            {
                protected override Guid GetKeyForItem(PlayerViewModel item)
                {
                    return item.Id;
                }
            }

            #endregion
        }

        #endregion
    }
}
