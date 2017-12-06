using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Caliburn.Micro;
using Mox.Lobby;
using Mox.Lobby.Client;

namespace Mox.UI.Lobby
{
    public class LobbyViewModel : Screen, IDisposable
    {
        #region Variables

        private readonly Scope m_syncingFromModelScope = new Scope();

        private readonly LobbyGameParametersViewModel m_gameParameters;
        private readonly LobbyReadinessViewModel m_readiness;
        private readonly LobbyMessagesViewModel m_messages = new LobbyMessagesViewModel();

        private readonly KeyedUserCollection m_usersById = new KeyedUserCollection();
        private readonly ObservableCollection<LobbyUserViewModel> m_users = new ObservableCollection<LobbyUserViewModel>();
        private readonly ObservableCollection<LobbyPlayerSlotViewModel> m_slots = new ObservableCollection<LobbyPlayerSlotViewModel>();

        private ILobby m_lobby;

        #endregion

        #region Constructor

        public LobbyViewModel()
        {
            m_gameParameters = new LobbyGameParametersViewModel(this);
            m_readiness = new LobbyReadinessViewModel(this);
        }

        public void Dispose()
        {
            Bind(null);
            m_messages.Dispose();
        }

        #endregion

        #region Properties

        public ILobby Source
        {
            get { return m_lobby; }
        }

        public LobbyGameParametersViewModel GameParameters
        {
            get { return m_gameParameters; }
        }

        public LobbyReadinessViewModel Readiness
        {
            get { return m_readiness; }
        }
        
        public LobbyMessagesViewModel Messages
        {
            get { return m_messages; }
        }

        public IList<LobbyUserViewModel> Users
        {
            get { return m_users; }
        }

        public IList<LobbyPlayerSlotViewModel> Slots
        {
            get { return m_slots; }
        }

        private LobbyUserViewModel m_localUser;
        public LobbyUserViewModel LocalUser
        {
            get { return m_localUser; }
            set
            {
                if (m_localUser != value)
                {
                    m_localUser = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(() => IsLeader);
                }
            }
        }

        private LobbyUserViewModel m_leader;
        public LobbyUserViewModel Leader
        {
            get { return m_leader; }
            set
            {
                if (m_leader != value)
                {
                    m_leader = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(() => IsLeader);
                }
            }
        }

        public bool IsLeader
        {
            get { return m_leader != null && m_localUser == m_leader; }
        }

        internal ILobby Lobby
        {
            get { return m_lobby; }
        }

        public bool IsSyncingFromModel
        {
            get { return m_syncingFromModelScope.InScope; }
        }

        private string m_serverName;
        public string ServerName
        {
            get { return m_serverName; }
            set
            {
                if (m_serverName != value)
                {
                    m_serverName = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (ConnectedPageViewModel == null)
                    return null;

                return ConnectedPageViewModel.CloseCommand;
            }
        }

        internal ConnectedPageViewModel ConnectedPageViewModel { get; set; }

        #endregion

        #region Methods

        public bool TryGetUser(Guid user, out LobbyUserViewModel userViewModel)
        {
            return m_usersById.TryGetValue(user, out userViewModel);
        }

        #endregion

        #region Bind

        internal void Bind(Client client)
        {
            if (m_lobby != null)
            {
                m_lobby.GameParametersChanged -= GameParameters_Changed;
                m_lobby.LeaderChanged -= Leader_Changed;
                m_lobby.Users.UserJoined -= Users_UserJoined;
                m_lobby.Users.UserLeft -= Users_UserLeft;
                m_lobby.Slots.Changed -= Slots_Changed;
            }

            m_lobby = client != null ? client.Lobby : null;

            m_messages.Bind(this, m_lobby != null ? m_lobby.Messages : null);

            SyncFromModel();

            if (client != null)
            {
                ServerName = string.Format("{0} - {1}", client.ServerName, client.Lobby.Parameters);
            }

            if (m_lobby != null)
            {
                m_lobby.GameParametersChanged += GameParameters_Changed;
                m_lobby.LeaderChanged += Leader_Changed;
                m_lobby.Users.UserJoined += Users_UserJoined;
                m_lobby.Users.UserLeft += Users_UserLeft;
                m_lobby.Slots.Changed += Slots_Changed;
            }
        }

        private void SyncFromModel()
        {
            using (m_syncingFromModelScope.Begin())
            {
                m_users.Clear();
                m_usersById.Clear();

                m_slots.Clear();
                LobbyUserViewModel localUser = null;
                LobbyUserViewModel leader = null;

                if (m_lobby != null)
                {
                    m_gameParameters.Update(m_lobby.GameParameters);

                    foreach (var user in m_lobby.Users)
                    {
                        WhenUserJoined(user);
                    }

                    TryGetUser(m_lobby.LocalUserId, out localUser);
                    TryGetUser(m_lobby.LeaderId, out leader);

                    for (int i = 0; i < m_lobby.Slots.Count; i++)
                    {
                        var slot = new LobbyPlayerSlotViewModel(this, i);
                        slot.SyncFromModel(m_lobby.Slots[i]);
                        m_slots.Add(slot);
                    }
                }

                LocalUser = localUser;
                Leader = leader;
            }
        }

        private void GameParameters_Changed(object sender, EventArgs e)
        {
            using (m_syncingFromModelScope.Begin())
            {
                m_gameParameters.Update(m_lobby.GameParameters);
            }
        }

        private void Leader_Changed(object sender, EventArgs e)
        {
            TryGetUser(m_lobby.LeaderId, out LobbyUserViewModel leader);
            Leader = leader;
        }

        private void Users_UserJoined(object sender, ItemEventArgs<ILobbyUser> e)
        {
            using (m_syncingFromModelScope.Begin())
            {
                WhenUserJoined(e.Item);
            }
        }

        private void WhenUserJoined(ILobbyUser lobbyUser)
        {
            var userViewModel = new LobbyUserViewModel(lobbyUser.Id);
            userViewModel.Update(lobbyUser);

            m_usersById.Add(userViewModel);
            m_users.Add(userViewModel);

            FetchPlayerIdentity(m_lobby, userViewModel);
        }

        private void Users_UserLeft(object sender, ItemEventArgs<ILobbyUser> e)
        {
            using (m_syncingFromModelScope.Begin())
            {
                var lobbyUser = e.Item;

                if (TryGetUser(lobbyUser.Id, out LobbyUserViewModel userViewModel))
                {
                    m_usersById.Remove(userViewModel.Id);
                    m_users.Remove(userViewModel);
                }
            }
        }

        private void Slots_Changed(object sender, ItemEventArgs<int[]> e)
        {
            using (m_syncingFromModelScope.Begin())
            {
                foreach (var index in e.Item)
                {
                    m_slots[index].SyncFromModel(m_lobby.Slots[index]);
                }
            }
        }

        private async void FetchPlayerIdentity(ILobby lobby, LobbyUserViewModel userViewModel)
        {
            var identity = await lobby.GetUserIdentity(userViewModel.Id);
            await userViewModel.UpdateIdentity(identity);
        }

        #endregion

        #region Commands

        public ICommand StartGameCommand
        {
            get
            {
                return new RelayCommand(StartGame, CanStartGame);
            }
        }

        private bool m_canStartGame = true;

        private async void StartGame()
        {
            try
            {
                m_canStartGame = false;
                await m_lobby.GameService.StartGame();
            }
            finally
            {
                m_canStartGame = true;
            }
        }

        private bool CanStartGame()
        {
            if (!IsLeader)
                return false;

            if (!m_canStartGame)
                return false;

            foreach (var slot in m_slots)
            {
                if (!slot.IsReady)
                    return false;
            }

            return true;
        }

        #endregion

        #region User Settings

        public void LoadUserSettings()
        {
            LobbyUserSettings.Load(this);
        }

        public void SaveUserSettings()
        {
            LobbyUserSettings.Save(m_lobby);
        }

        #endregion

        #region Inner Types

        private class KeyedUserCollection : KeyedCollection<Guid, LobbyUserViewModel>
        {
            protected override Guid GetKeyForItem(LobbyUserViewModel item)
            {
                return item.Id;
            }
        }

        #endregion
    }

    public class LobbyViewModel_DesignTime : LobbyViewModel
    {
        public LobbyViewModel_DesignTime()
        {
            ServerName = "My Server";

            Users.Add(new LobbyUserViewModel(Guid.Empty) { Name = "John" });

            Slots.Add(new LobbyPlayerSlotViewModel(this, 0));
            Slots.Add(new LobbyPlayerSlotViewModel(this, 1));

            LocalUser = Users[0];
            Slots[0].Player = Users[0];
        }
    }
}
