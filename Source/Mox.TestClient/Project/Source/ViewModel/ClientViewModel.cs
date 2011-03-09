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
        private readonly Dispatcher m_dispatcher;

        private readonly ObservableCollection<User> m_users = new ObservableCollection<User>();
        private string m_text;
        private string m_input;

        #endregion

        #region Constructor

        public ClientViewModel(ILobby lobby, Dispatcher dispatcher)
        {
            m_lobby = lobby;
            m_dispatcher = dispatcher;
            m_lobby.Chat.MessageReceived += Chat_MessageReceived;
            m_lobby.UserChanged += m_lobby_UserChanged;
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
            OnUserSaid(m_lobby.User, message);
            Input = string.Empty;
        }

        private void OnUserSaid(User user, string message)
        {
            user = user ?? new User("NOT CONNECTED");
            Text += string.Format("{0}: {1}{2}", user.Name, message, Environment.NewLine);
        }

        private void OnUserChanged(UserChangedEventArgs e)
        {
            switch (e.Change)
            {
                case UserChange.Joined:
                    m_users.Add(e.User);
                    break;

                case UserChange.Left:
                    m_users.Remove(e.User);
                    break;
            }
        }

        public void OnDisconnected()
        {
            OnUserSaid(new User("GOD"), "The connection just died!");
        }

        #endregion

        #region Event Handlers

        void Chat_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            m_dispatcher.BeginInvoke(new System.Action(() => OnUserSaid(e.User, e.Message)));
        }

        void m_lobby_UserChanged(object sender, UserChangedEventArgs e)
        {
            m_dispatcher.BeginInvoke(new System.Action(() => OnUserChanged(e)));
        }

        #endregion
    }
}
