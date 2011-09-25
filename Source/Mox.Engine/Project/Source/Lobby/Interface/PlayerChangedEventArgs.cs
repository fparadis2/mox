using System;

namespace Mox.Lobby
{
    public enum PlayerChange
    {
        Joined,
        Left,
        Changed
    }

    public class PlayerChangedEventArgs : EventArgs
    {
        #region Variables

        private readonly PlayerChange m_change;
        private readonly Player m_player;

        #endregion

        #region Constructor

        public PlayerChangedEventArgs(PlayerChange change, Player player)
        {
            m_change = change;
            m_player = player;
        }

        #endregion

        #region Properties

        public Player Player
        {
            get { return m_player; }
        }

        public PlayerChange Change
        {
            get { return m_change; }
        }

        #endregion
    }
}
