using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Mox.UI.Lobby
{
    public class LobbyViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly LobbyChatViewModel m_chat = new LobbyChatViewModel();
        private readonly LobbyServerMessagesViewModel m_serverMessages = new LobbyServerMessagesViewModel();
        private readonly ObservableCollection<UserViewModel> m_users = new ObservableCollection<UserViewModel>();
        private readonly ObservableCollection<PlayerViewModel> m_players = new ObservableCollection<PlayerViewModel>();

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

        public IList<UserViewModel> Users
        {
            get { return m_users; }
        }

        public IList<PlayerViewModel> Players
        {
            get { return m_players; }
        }

        #endregion
    }
}
