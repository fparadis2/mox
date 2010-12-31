using System;

namespace Mox.Lobby
{
    /// <summary>
    /// A user in the lobby (player, spectator)
    /// </summary>
    [Serializable]
    public class User
    {
        #region Variables

        private readonly Guid m_identifier = Guid.NewGuid();
        private readonly string m_name;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public User(string name)
        {
            Throw.IfEmpty(name, "name");
            m_name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the user.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_identifier.GetHashCode();
        }

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            User other = obj as User;

            return !ReferenceEquals(other, null) && other.m_identifier == m_identifier;
        }

        /// <summary>
        /// ==
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(User a, User b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        /// <summary>
        /// !=
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(User a, User b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[User: {0}]", Name);
        }

        #endregion
    }
}
