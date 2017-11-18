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
#warning Messages todo
        //private readonly LobbyChatViewModel m_chat = new LobbyChatViewModel();
        //private readonly LobbyServerMessagesViewModel m_serverMessages = new LobbyServerMessagesViewModel();

        private readonly KeyedUserCollection m_usersById = new KeyedUserCollection();
        private readonly ObservableCollection<LobbyPlayerViewModel> m_players = new ObservableCollection<LobbyPlayerViewModel>();
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
#warning Messages todo
            //m_chat.Dispose();
            //m_serverMessages.Dispose();
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

#warning Messages todo
        /*public LobbyChatViewModel Chat
        {
            get { return m_chat; }
        }

        public LobbyServerMessagesViewModel ServerMessages
        {
            get { return m_serverMessages; }
        }*/

        public IList<LobbyPlayerViewModel> Players
        {
            get { return m_players; }
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

        private LobbyPlayerViewModel m_leader;
        public LobbyPlayerViewModel Leader
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

        public bool TryGetUserViewModel(Guid user, out LobbyUserViewModel userViewModel)
        {
            return m_usersById.TryGetValue(user, out userViewModel);
        }

        public bool TryGetPlayerViewModel(Guid user, out LobbyPlayerViewModel player)
        {
            LobbyUserViewModel userViewModel;
            TryGetUserViewModel(user, out userViewModel);

            player = userViewModel as LobbyPlayerViewModel;
            return player != null;
        }

        #endregion

        #region Bind

        internal void Bind(Client client)
        {
            if (m_lobby != null)
            {
                m_lobby.GameParametersChanged -= GameParameters_Changed;
                m_lobby.LeaderChanged -= Leader_Changed;
                m_lobby.Players.Changed -= Players_Changed;
                m_lobby.Slots.Changed -= Slots_Changed;
            }

            m_lobby = client != null ? client.Lobby : null;

#warning Messages todo
            //m_chat.Bind(m_lobby != null ? m_lobby.Chat : null);
            //m_serverMessages.Bind(m_lobby != null ? m_lobby.ServerMessages : null);

            SyncFromModel();

            if (client != null)
            {
                ServerName = string.Format("{0} - {1}", client.ServerName, client.Lobby.Parameters);
            }

            if (m_lobby != null)
            {
                m_lobby.GameParametersChanged += GameParameters_Changed;
                m_lobby.LeaderChanged += Leader_Changed;
                m_lobby.Players.Changed += Players_Changed;
                m_lobby.Slots.Changed += Slots_Changed;
            }
        }

        private void SyncFromModel()
        {
            using (m_syncingFromModelScope.Begin())
            {
                m_players.Clear();
                m_usersById.Clear();

                m_slots.Clear();
                LobbyUserViewModel localUser = null;
                LobbyPlayerViewModel leader = null;

                if (m_lobby != null)
                {
                    m_gameParameters.Update(m_lobby.GameParameters);

                    foreach (var user in m_lobby.Players)
                    {
                        WhenPlayerJoin(user);
                    }

                    TryGetUserViewModel(m_lobby.LocalUserId, out localUser);
                    TryGetPlayerViewModel(m_lobby.LeaderId, out leader);

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
            LobbyPlayerViewModel leader;
            TryGetPlayerViewModel(m_lobby.LeaderId, out leader);
            Leader = leader;
        }

        private void Players_Changed(object sender, PlayersChangedEventArgs e)
        {
            using (m_syncingFromModelScope.Begin())
            {
                switch (e.Change)
                {
                    case PlayersChangedEventArgs.ChangeType.Joined:
                        e.Players.ForEach(WhenPlayerJoin);
                        break;

                    case PlayersChangedEventArgs.ChangeType.Left:
                        e.Players.ForEach(WhenPlayerLeave);
                        break;

                    case PlayersChangedEventArgs.ChangeType.Changed:
                        e.Players.ForEach(WhenPlayerChange);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void WhenPlayerJoin(PlayerData player)
        {
            var playerViewModel = new LobbyPlayerViewModel(player);
            m_usersById.Add(playerViewModel);
            m_players.Add(playerViewModel);

            FetchPlayerIdentity(m_lobby, playerViewModel);
        }

        private void WhenPlayerLeave(PlayerData player)
        {
            LobbyPlayerViewModel playerViewModel;
            if (TryGetPlayerViewModel(player.Id, out playerViewModel))
            {
                m_usersById.Remove(player.Id);
                m_players.Remove(playerViewModel);
            }
        }

        private void WhenPlayerChange(PlayerData player)
        {
            LobbyPlayerViewModel playerViewModel;
            if (TryGetPlayerViewModel(player.Id, out playerViewModel))
            {
                playerViewModel.SyncFromPlayer(player);
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

        private async void FetchPlayerIdentity(ILobby lobby, LobbyPlayerViewModel playerViewModel)
        {
            var identity = await lobby.GetPlayerIdentity(playerViewModel.Id);
            var image = await LobbyPlayerViewModel.GetImageSource(identity);
            playerViewModel.Image = image;
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
                if (slot.IsReady)
                    continue;

                if (slot.Player == m_localUser && slot.IsValid)
                    continue; // Leader is considered ready if valid

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

            Players.Add(new LobbyPlayerViewModel(new PlayerData { Name = "John" }));

            Slots.Add(new LobbyPlayerSlotViewModel(this, 0));
            Slots.Add(new LobbyPlayerSlotViewModel(this, 1));

            LocalUser = Players[0];
            Slots[0].Player = Players[0];
        }
    }
}
