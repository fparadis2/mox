using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Mox.Lobby;
using Mox.UI;

namespace Mox
{
    public class ClientViewModel : ViewModel
    {
        #region Variables

        private readonly ILobby m_lobby;

        private readonly ObservableCollection<User> m_users = new ObservableCollection<User>();
        private string m_text;
        private string m_input;

        #endregion

        #region Constructor

        public ClientViewModel(ILobby lobby)
        {
            m_lobby = lobby;
            m_lobby.Chat.MessageReceived += Chat_MessageReceived;
            m_lobby.Users.CollectionChanged += Users_CollectionChanged;
            m_lobby.Users.ForEach(m_users.Add);
        }

        #endregion

        #region Properties

        public ObservableCollection<User> Users
        {
            get { return m_users; }
        }

        public string Text
        {
            get { return m_text; }
            set
            {
                m_text = value;
                OnPropertyChanged("Text");
            }
        }

        public string Input
        {
            get { return m_input; }
            set
            {
                m_input = value;
                OnPropertyChanged("Input");
            }
        }

        public ICommand SayCommand
        {
            get { return new RelayCommand(o => !string.IsNullOrEmpty(Input), o => Say(Input)); }
        }

        #endregion

        #region Methods

        private void Say(string message)
        {
            m_lobby.Chat.Say(message);
            Input = string.Empty;
        }

        private void OnUserSaid(ChatMessageReceivedEventArgs e)
        {
            Text += e.ToChatMessage() + Environment.NewLine;
        }

        public void OnDisconnected()
        {
            OnUserSaid(new ChatMessageReceivedEventArgs(null, "The connection just died!"));
        }

        #endregion

        #region Event Handlers

        void Chat_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            OnUserSaid(e);
        }

        void Users_CollectionChanged(object sender, Collections.CollectionChangedEventArgs<User> e)
        {
            e.Synchronize(m_users.Add, u => m_users.Remove(u));
        }

        #endregion
    }
}
