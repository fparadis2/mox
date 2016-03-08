using System;

using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyServerMessagesViewModel : PropertyChangedBase, IDisposable
    {
        #region Variables

        private IServerMessages m_serverMessages;

        private string m_text;

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

        #endregion

        #region Methods

        internal void Bind(IServerMessages serverMessages)
        {
            if (m_serverMessages != null)
            {
                m_serverMessages.MessageReceived -= WhenMessageReceived;
            }

            m_serverMessages = serverMessages;

            if (m_serverMessages != null)
            {
                m_serverMessages.MessageReceived += WhenMessageReceived;
            }
        }

        private void WhenMessageReceived(object sender, ServerMessageReceivedEventArgs e)
        {
            var message = e.ToServerMessage();

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Text += Environment.NewLine + message;
        }

        #endregion
    }
}
