using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Caliburn.Micro;
using Mox.Collections;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyViewModel : PropertyChangedBase, IDisposable
    {
        #region Variables

        private readonly LobbyChatViewModel m_chat = new LobbyChatViewModel();
        private readonly LobbyServerMessagesViewModel m_serverMessages = new LobbyServerMessagesViewModel();

        private readonly KeyedUserCollection m_usersById = new KeyedUserCollection();
        private readonly System.Collections.ObjectModel.ObservableCollection<LobbyUserViewModel> m_users = new System.Collections.ObjectModel.ObservableCollection<LobbyUserViewModel>();
        private readonly System.Collections.ObjectModel.ObservableCollection<LobbySlotViewModel> m_slots = new System.Collections.ObjectModel.ObservableCollection<LobbySlotViewModel>();

        private ILobby m_lobby;

        #endregion

        #region Constructor

        public void Dispose()
        {
            Bind(null);
            m_chat.Dispose();
            m_serverMessages.Dispose();
        }

        #endregion

        #region Properties

        public LobbyChatViewModel Chat
        {
            get { return m_chat; }
        }

        public LobbyServerMessagesViewModel ServerMessages
        {
            get { return m_serverMessages; }
        }

        public IList<LobbyUserViewModel> Users
        {
            get { return m_users; }
        }

        public IReadOnlyList<LobbySlotViewModel> Slots
        {
            get { return m_slots; }
        }

        #endregion

        #region Methods

        public bool TryGetUserViewModel(User user, out LobbyUserViewModel userViewModel)
        {
            return m_usersById.TryGetValue(user.Id, out userViewModel);
        }

        #endregion

        #region Bind

        internal void Bind(ILobby lobby)
        {
            if (m_lobby != null)
            {
                m_lobby.Users.CollectionChanged -= Users_CollectionChanged;
                m_lobby.Slots.ItemChanged -= Slots_ItemChanged;
            }

            m_lobby = lobby;
            m_chat.Bind(m_lobby != null ? m_lobby.Chat : null);
            m_serverMessages.Bind(m_lobby != null ? m_lobby.ServerMessages : null);

            SyncFromModel();

            if (m_lobby != null)
            {
                m_lobby.Users.CollectionChanged += Users_CollectionChanged;
                m_lobby.Slots.ItemChanged += Slots_ItemChanged;
            }
        }

        private void SyncFromModel()
        {
            m_users.Clear();
            m_usersById.Clear();

            m_slots.Clear();

            if (m_lobby != null)
            {
                foreach (var user in m_lobby.Users)
                {
                    WhenUserJoin(user);
                }

                for (int i = 0; i < m_lobby.Slots.Count; i++)
                {
                    var slot = new LobbySlotViewModel(this, i);
                    slot.SyncFromModel(m_lobby.Slots[i]);
                    m_slots.Add(slot);
                }
            }
        }

        private void Users_CollectionChanged(object sender, CollectionChangedEventArgs<User> e)
        {
            e.Synchronize(WhenUserJoin, WhenUserLeave);
        }

        private void WhenUserJoin(User user)
        {
            var userViewModel = new LobbyUserViewModel(user);
            m_usersById.Add(userViewModel);
            m_users.Add(userViewModel);
        }

        private void WhenUserLeave(User user)
        {
            LobbyUserViewModel userViewModel;
            if (m_usersById.TryGetValue(user.Id, out userViewModel))
            {
                m_usersById.Remove(user.Id);
                m_users.Remove(userViewModel);
            }
        }

        private void Slots_ItemChanged(object sender, ItemEventArgs<int> e)
        {
            m_slots[e.Item].SyncFromModel(m_lobby.Slots[e.Item]);
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
}
