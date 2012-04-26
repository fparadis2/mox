using System;
using System.Collections.Generic;

namespace Mox.Lobby
{
    public enum PlayerChange
    {
        Joined,
        Left,
        Changed
    }

    public class PlayerChangedResponse : Message
    {
        #region Variables

        private readonly PlayerChange m_change;
        private readonly List<Player> m_players;

        #endregion

        #region Constructor

        public PlayerChangedResponse(PlayerChange change, IEnumerable<Player> players)
        {
            m_change = change;
            m_players = new List<Player>(players);
        }

        #endregion

        #region Properties

        public IList<Player> Players
        {
            get { return m_players; }
        }

        public PlayerChange Change
        {
            get { return m_change; }
        }

        #endregion
    }
}
