using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
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

        private void OnUserSaid(User user, string message)
        {
            user = user ?? new User("NOT CONNECTED");
            Text += string.Format("{0}: {1}{2}", user.Name, message, Environment.NewLine);
        }

        public void OnDisconnected()
        {
            OnUserSaid(new User("GOD"), "The connection just died!");
        }

        #endregion

        #region Event Handlers

        void Chat_MessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            OnUserSaid(e.User, e.Message);
        }

        void Users_CollectionChanged(object sender, Collections.CollectionChangedEventArgs<User> e)
        {
            e.Synchronize(m_users.Add, u => m_users.Remove(u));
        }

        #endregion
    }
}
