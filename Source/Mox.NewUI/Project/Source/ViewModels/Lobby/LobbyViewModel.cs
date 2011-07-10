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

        private readonly IDispatcher m_dispatcher;
        private readonly ILobby m_lobby;

        private readonly ObservableCollection<UserViewModel> m_users = new ObservableCollection<UserViewModel>();
        private readonly KeyedUserCollection m_usersById = new KeyedUserCollection();

        #endregion

        #region Constructor

        public LobbyViewModel(ILobby lobby, IDispatcher dispatcher)
        {
            Throw.IfNull(lobby, "lobby");
            Throw.IfNull(dispatcher, "dispatcher");

            m_lobby = lobby;
            m_dispatcher = dispatcher;

            m_lobby.UserChanged += m_lobby_UserChanged;
        }

        #endregion

        #region Properties

        public IList<UserViewModel> Users
        { 
            get { return m_users; }
        }

        #endregion

        #region Methods

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
}
