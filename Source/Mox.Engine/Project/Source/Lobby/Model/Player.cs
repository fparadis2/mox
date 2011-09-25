using System;

namespace Mox.Lobby
{
    [Serializable]
    public class Player
    {
        #region Variables

        private readonly Guid m_id = Guid.NewGuid();
        private readonly User m_user;

        #endregion

        #region Constructor

        public Player(User user)
        {
            Throw.IfNull(user, "user");
            m_user = user;
        }

        private Player(Player other, User newUser)
        {
            m_id = other.Id;
            m_user = newUser;
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_id; }
        }

        public User User
        {
            get { return m_user; }
        }

        #endregion

        #region Methods

        internal Player AssignUser(User newUser)
        {
            return new Player(this, newUser);
        }

        #endregion
    }
}
