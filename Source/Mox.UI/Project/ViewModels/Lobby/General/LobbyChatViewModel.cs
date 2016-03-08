using System;

using System.Windows.Input;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyChatViewModel : PropertyChangedBase, IDisposable
    {
        #region Variables

        private IChatService m_chatService;

        private string m_text;
        private string m_input;

        #endregion

        #region Constructor

        public void Dispose()
        {
            Bind(null);
        }

        #endregion

        #region Properties

        public string Text
        {
            get
            {
                return m_text;
            }
            set
            {
                if (m_text != value)
                {
                    m_text = value;
                    NotifyOfPropertyChange(() => Text);
                }
            }
        }

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
                    NotifyOfPropertyChange(() => Input);
                }
            }
        }

        public ICommand SayCommand
        {
            get { return new RelayCommand(Say, CanSay); }
        }

        #endregion

        #region Methods

        internal void Bind(IChatService chatService)
        {
            if (m_chatService != null)
            {
                m_chatService.MessageReceived -= WhenMessageReceived;
            }

            m_chatService = chatService;

            if (m_chatService != null)
            {
                m_chatService.MessageReceived += WhenMessageReceived;
            }
        }

        private void WhenMessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            var message = e.ToChatMessage();

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Text += Environment.NewLine + message;
        }

        public bool CanSay()
        {
            return !string.IsNullOrEmpty(Input);
        }

        public void Say()
        {
            if (CanSay())
            {
                m_chatService.Say(Input);
                Input = string.Empty;
            }
        }

        #endregion
    }
}
