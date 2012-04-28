using System;
using Caliburn.Micro;

namespace Mox.UI.Lobby
{
    public class LobbyServerMessagesViewModel : PropertyChangedBase
    {
        #region Variables

        private string m_text;

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
    }
}
