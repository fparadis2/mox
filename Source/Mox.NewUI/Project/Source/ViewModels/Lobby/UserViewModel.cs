using System;
using System.Diagnostics;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class UserViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly Guid m_identifier;

        private string m_name;
        private bool m_isAi;

        #endregion

        #region Constructor

        public UserViewModel(User user)
        {
            m_identifier = user.Id;
            SyncFromUser(user);
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
            private set
            {
                if (m_name != value)
                {
                    m_name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        public bool IsAI
        {
            get 
            {
                return m_isAi;
            }
            private set
            {
                if (m_isAi != value)
                {
                    m_isAi = value;
                    NotifyOfPropertyChange(() => IsAI);
                }
            }
        }

        #endregion

        #region Methods

        private void SyncFromUser(User user)
        {
            Debug.Assert(user.Id == m_identifier);
            Name = user.Name;
            IsAI = user.IsAI;
        }

        #endregion
    }
}