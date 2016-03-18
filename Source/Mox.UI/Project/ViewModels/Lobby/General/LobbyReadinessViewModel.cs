using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Mox.UI.Lobby;

namespace Mox.UI
{
    public class LobbyReadinessViewModel : PropertyChangedBase
    {
        private readonly LobbyViewModel m_lobbyViewModel;

        public LobbyReadinessViewModel(LobbyViewModel lobbyViewModel)
        {
            m_lobbyViewModel = lobbyViewModel;
        }

        #region Properties

        private bool m_isReady;

        public bool IsReady
        {
            get { return m_isReady; }
            set
            {
                if (m_isReady != value)
                {
                    m_isReady = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private bool m_canBeReady;

        public bool CanBeReady
        {
            get { return m_canBeReady; }
            set
            {
                if (m_canBeReady != value)
                {
                    m_canBeReady = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion
    }
}
