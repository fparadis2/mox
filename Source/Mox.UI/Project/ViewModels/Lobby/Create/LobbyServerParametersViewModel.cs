using Caliburn.Micro;

namespace Mox.UI.Lobby
{
    public class LobbyServerParametersViewModel : PropertyChangedBase
    {
        private string m_host;

        public string Host
        {
            get { return m_host; }
            set
            {
                if (m_host != value)
                {
                    m_host = value;
                    NotifyOfPropertyChange();
                }
            }
        }
    }
}