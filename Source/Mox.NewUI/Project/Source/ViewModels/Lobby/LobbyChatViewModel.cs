using System;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyChatViewModel : PropertyChangedBase
    {
        #region Variables

        private string m_text;
        private string m_input;

        #endregion

        #region Properties

        internal IChatService ChatService
        {
            get;
            set;
        }

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

        #endregion

        #region Methods

        public bool CanSay()
        {
            return !string.IsNullOrEmpty(Input);
        }

        public void Say()
        {
            if (CanSay())
            {
                ChatService.Say(Input);
                Input = string.Empty;
            }
        }

        #endregion
    }
}
