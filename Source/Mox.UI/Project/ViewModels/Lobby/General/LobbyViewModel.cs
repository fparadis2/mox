using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Caliburn.Micro;
using Mox.Collections;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyViewModel : Screen, IDisposable
    {
        #region Variables

        private readonly Scope m_syncingFromModelScope = new Scope();

        private readonly LobbyReadinessViewModel m_readiness;
        private readonly LobbyChatViewModel m_chat = new LobbyChatViewModel();
        private readonly LobbyServerMessagesViewModel m_serverMessages = new LobbyServerMessagesViewModel();

        private readonly KeyedUserCollection m_usersById = new KeyedUserCollection();
        private readonly System.Collections.ObjectModel.ObservableCollection<LobbyPlayerViewModel> m_players = new System.Collections.ObjectModel.ObservableCollection<LobbyPlayerViewModel>();
        private readonly System.Collections.ObjectModel.ObservableCollection<LobbyPlayerSlotViewModel> m_slots = new System.Collections.ObjectModel.ObservableCollection<LobbyPlayerSlotViewModel>();

        private ILobby m_lobby;

        #endregion

        #region Constructor

        public LobbyViewModel()
        {
            m_readiness = new LobbyReadinessViewModel(this);
        }

        public void Dispose()
        {
            Bind(null);
            m_chat.Dispose();
            m_serverMessages.Dispose();
        }

        #endregion

        #region Properties

        public LobbyReadinessViewModel Readiness
        {
            get { return m_readiness; }
        }

        public LobbyChatViewModel Chat
        {
            get { return m_chat; }
        }

        public LobbyServerMessagesViewModel ServerMessages
        {
            get { return m_serverMessages; }
        }

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
                }
            }
        }

        internal ILobby Lobby
        {
            get { return m_lobby; }
        }

        public bool IsSyncingFromModel
        {
            get { return m_syncingFromModelScope.InScope; }
        }

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

        internal void Bind(ILobby lobby)
        {
            if (m_lobby != null)
            {
                m_lobby.Players.Changed -= Players_Changed;
                m_lobby.Slots.Changed -= Slots_Changed;
            }

            m_lobby = lobby;
            m_chat.Bind(m_lobby != null ? m_lobby.Chat : null);
            m_serverMessages.Bind(m_lobby != null ? m_lobby.ServerMessages : null);

            SyncFromModel();

            if (m_lobby != null)
            {
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
                LocalUser = null;

                if (m_lobby != null)
                {
                    foreach (var user in m_lobby.Players)
                    {
                        WhenPlayerJoin(user);
                    }

                    LobbyUserViewModel localUser;
                    TryGetUserViewModel(m_lobby.LocalUserId, out localUser);
                    LocalUser = localUser;

                    for (int i = 0; i < m_lobby.Slots.Count; i++)
                    {
                        var slot = new LobbyPlayerSlotViewModel(this, i);
                        slot.SyncFromModel(m_lobby.Slots[i]);
                        m_slots.Add(slot);
                    }
                }
            }
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
            Players.Add(new LobbyPlayerViewModel(new PlayerData { Name = "John" }));

            Slots.Add(new LobbyPlayerSlotViewModel(this, 0));
            Slots.Add(new LobbyPlayerSlotViewModel(this, 1));

            LocalUser = Players[0];
            Slots[0].Player = Players[0];
        }
    }
}
