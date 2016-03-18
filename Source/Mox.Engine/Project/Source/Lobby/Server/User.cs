using System;

using Mox.Lobby.Network;

namespace Mox.Lobby.Server
{
    public class User
    {
        #region Variables

        private readonly IChannel m_channel;
        private readonly Guid m_id = Guid.NewGuid();
        private readonly string m_name;

        #endregion

        #region Constructor

        public User(IChannel channel, string name)
        {
            Throw.IfNull(channel, "channel");
            Throw.IfEmpty(name, "name");

            m_channel = channel;
            m_name = name;
        }

        #endregion

        #region Properties

        public IChannel Channel
        {
            get { return m_channel; }
        }

        public Guid Id
        {
            get { return m_id; }
        }

        public string Name
        {
            get { return m_name; }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return m_name + "@" + m_channel.EndPointIdentifier;
        }

        public override int GetHashCode()
        {
            return m_id.GetHashCode();
        }

        #endregion
    }
}
