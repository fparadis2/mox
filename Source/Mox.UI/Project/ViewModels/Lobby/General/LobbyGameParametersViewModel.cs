using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyGameParametersViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly LobbyViewModel m_lobby;

        #endregion

        #region Constructor

        public LobbyGameParametersViewModel()
            : this(null)
        {
        }

        public LobbyGameParametersViewModel(LobbyViewModel lobby)
        {
            m_lobby = lobby;
        }

        #endregion

        #region Properties

        private LobbyGameAIType m_aiType;

        public LobbyGameAIType AIType
        {
            get { return m_aiType; }
            set
            {
                if (m_aiType != value)
                {
                    m_aiType = value;
                    NotifyOfPropertyChange();

                    TryPushOnServer();
                }
            }
        }

        private double m_aiTimeOut;

        public double AITimeOut
        {
            get { return m_aiTimeOut; }
            set
            {
                m_aiTimeOut = value;
                NotifyOfPropertyChange();

                TryPushOnServer();
            }
        }

        #endregion

        #region Methods

        public void Update(LobbyGameParameters gameParameters)
        {
            var botParameters = gameParameters.BotParameters;
            AIType = botParameters.AIType;
            AITimeOut = botParameters.TimeOut;
        }

        private LobbyGameParameters ToGameParameters()
        {
            return new LobbyGameParameters
            {
                BotParameters =
                {
                    AIType = AIType,
                    TimeOut = AITimeOut
                }
            };
        }

        private async void TryPushOnServer()
        {
            if (m_lobby == null || m_lobby.IsSyncingFromModel)
                return;

            var lobby = m_lobby.Lobby;
            if (lobby == null)
                return;

            bool result = await lobby.SetGameParameters(ToGameParameters());
            if (!result)
            {
                Update(lobby.GameParameters);
            }
        }

        #endregion
    }
}
