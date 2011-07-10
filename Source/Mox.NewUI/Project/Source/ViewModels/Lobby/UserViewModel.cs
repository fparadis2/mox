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

        #endregion

        #region Methods

        private void SyncFromUser(User user)
        {
            Debug.Assert(user.Id == m_identifier);
            Name = user.Name;
        }

        #endregion
    }
}