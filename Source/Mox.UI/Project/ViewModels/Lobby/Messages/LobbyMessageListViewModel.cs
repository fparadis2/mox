using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mox.UI.Lobby
{
    public class LobbyMessageListViewModel
    {
        #region Variables

        private readonly ObservableCollection<LobbyMessageGroupViewModel> m_groups = new ObservableCollection<LobbyMessageGroupViewModel>();

        #endregion

        #region Properties

        public IReadOnlyList<LobbyMessageGroupViewModel> Groups { get { return m_groups; } }

        #endregion

        #region Methods

        public void Clear()
        {
            m_groups.Clear();
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
        public DateTime Timestamp { get; set; }
        public LobbyUserViewModel User { get; set; }
        public string Text { get; set; }
    }

    public class LobbyMessageListViewModel_DesignTime : LobbyMessageListViewModel
    {
        public LobbyMessageListViewModel_DesignTime()
        {
            AddMessages(this);
        }

        public static void AddMessages(LobbyMessageListViewModel messageList)
        {
            var user1 = new LobbyPlayerViewModel(new Mox.Lobby.PlayerData { Name = "John" });
            var user2 = new LobbyPlayerViewModel(new Mox.Lobby.PlayerData { Name = "Marvin" }, true);

            messageList.Add(user1, "Hello World");
            messageList.Add(user1, "It's me again");
            messageList.Add(user1, "I want to write many lines");
            messageList.Add(user1, "So we can check the behavior with a lot of lines and also very very very long lines. I wonder what will happen?");

            messageList.Add(user2, "You? What do you want?");

            messageList.Add(user1, "I'm thinking of writing some messages");
        }
    }
}
