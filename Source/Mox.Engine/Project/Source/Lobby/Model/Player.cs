using System;

namespace Mox.Lobby
{
    [Serializable]
    public class Player
    {
        #region Variables

        private readonly Guid m_id = Guid.NewGuid();
        private readonly User m_user;
        private readonly PlayerData m_playerData;

        #endregion

        #region Constructor

        public Player(User user)
        {
            Throw.IfNull(user, "user");
            m_user = user;
            m_playerData = new PlayerData();
        }

        private Player(Player other, User newUser, PlayerData playerData)
        {
            Throw.IfNull(newUser, "newUser");
            m_id = other.Id;
            m_user = newUser;
            m_playerData = playerData;
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

        public PlayerData Data
        {
            get { return m_playerData; }
        }

        #endregion

        #region Methods

        internal Player AssignUser(User newUser)
        {
            return new Player(this, newUser, m_playerData);
        }

        internal Player ChangeData(PlayerData playerData)
        {
            return new Player(this, m_user, playerData);
        }

        #endregion
    }
}
