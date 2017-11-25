using System;

using Mox.Lobby.Network;

namespace Mox.Lobby.Server
{
    public struct User
    {
        #region Variables

        public static readonly User Invalid = new User();

        private readonly IChannel m_channel;
        private readonly Guid m_id;
        private readonly string m_name;

        #endregion

        #region Constructor

        public User(IChannel channel, string name)
        {
            Throw.IfEmpty(name, "name");

            m_channel = channel;
            m_id = Guid.NewGuid();
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

        public bool IsValid
        {
            get { return m_id != Guid.Empty; }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return m_name + "@" + m_channel?.EndPointIdentifier;
        }

        public override int GetHashCode()
        {
            return m_id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals((User)obj);
        }

        public bool Equals(User other)
        {
            return m_id == other.m_id;
        }

        public static bool operator ==(User a, User b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(User a, User b)
        {
            return !a.Equals(b);
        }

        #endregion
    }
}
