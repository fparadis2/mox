using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Mox.Lobby;
using System.Windows.Input;

namespace Mox.UI.Lobby
{
    public class LobbyMessagesViewModel : PropertyChangedBase, IDisposable 
    {
        #region Variables
        
        private readonly ObservableCollection<LobbyMessageGroupViewModel> m_groups = new ObservableCollection<LobbyMessageGroupViewModel>();

        private LobbyViewModel m_lobby;
        private IMessageService m_messageService;

        private string m_input;

        #endregion

        #region Constructor

        public void Dispose()
        {
            Bind(null, null);
        }

        #endregion

        #region Properties

        public IReadOnlyList<LobbyMessageGroupViewModel> Groups { get { return m_groups; } }

        public string Input
        {
            get
            {
                return m_input;
            }
            set
            {
                if (m_input != value)
                {
                    m_input = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public ICommand SendMessageCommand
        {
            get { return new RelayCommand(SendMessage, CanSendMessage); }
        }

        #endregion

        #region Methods

        internal void Bind(LobbyViewModel lobby, IMessageService messageService)
        {
            if (m_messageService != null)
            {
                m_messageService.ChatMessageReceived -= WhenChatMessageReceived;
            }

            m_lobby = lobby;
            m_messageService = messageService;
            m_groups.Clear();

            if (m_messageService != null)
            {
                m_messageService.ChatMessageReceived += WhenChatMessageReceived;
            }
        }

        private void WhenChatMessageReceived(object sender, ChatMessage e)
        {
            if (m_lobby.TryGetUserViewModel(e.SpeakerUserId, out LobbyUserViewModel user))
            {
                Add(user, e.Text);
            }
        }

        public bool CanSendMessage()
        {
            return !string.IsNullOrEmpty(Input);
        }

        public void SendMessage()
        {
            if (CanSendMessage())
            {
                m_messageService.SendMessage(Input);
                Input = string.Empty;
            }
        }

        public void Add(LobbyUserViewModel user, string text)
        {
            var viewModel = new LobbyMessageViewModel
            {
                Timestamp = DateTime.Now,
                User = user,
                Text = text
            };

            var group = GetOrCreateGroup(viewModel);
            group.Messages.Add(viewModel);
        }

        private LobbyMessageGroupViewModel GetOrCreateGroup(LobbyMessageViewModel message)
        {
            if (m_groups.Count > 0)
            {
                var lastGroup = m_groups[m_groups.Count - 1];
                if (lastGroup.Header.User == message.User)
                    return lastGroup;
            }

            var newGroup = new LobbyMessageGroupViewModel();
            m_groups.Add(newGroup);
            return newGroup;
        }

        #endregion
    }

    public class LobbyMessageGroupViewModel
    {
        private readonly ObservableCollection<LobbyMessageViewModel> m_messages = new ObservableCollection<LobbyMessageViewModel>();

        public IList<LobbyMessageViewModel> Messages { get { return m_messages; } }
        public LobbyMessageViewModel Header { get { return m_messages[0]; } }
    }

    public struct LobbyMessageViewModel
    {
        public DateTime Timestamp;
        public LobbyUserViewModel User;
        public string Text;
    }

    public class LobbyMessagesViewModel_DesignTime : LobbyMessagesViewModel
    {
        public LobbyMessagesViewModel_DesignTime()
        {
            var user1 = new LobbyPlayerViewModel(new Mox.Lobby.PlayerData { Name = "John" });
            var user2 = new LobbyPlayerViewModel(new Mox.Lobby.PlayerData { Name = "Marvin" }, true);

            Add(user1, "Hello World");
            Add(user1, "It's me again");

            Add(user2, "You? What do you want?");

            Add(user1, "I'm thinking of writing some messages");
        }
    }
}
