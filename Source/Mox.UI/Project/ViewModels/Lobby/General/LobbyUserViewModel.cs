using System;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyUserViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly Guid m_identifier;
        private string m_name;

        #endregion

        #region Constructor

        public LobbyUserViewModel(Guid id)
        {
            m_identifier = id;
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_identifier; }
        }

        public string Name
        {
            get { return m_name; }
            protected set
            {
                if (m_name != value)
                {
                    m_name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        #endregion
    }
}