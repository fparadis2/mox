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

        private static readonly IRandom ms_random = Random.New();
        private static readonly string[] m_aiNames = new[] { "John Doe", "HAL 9000", "Roboto", "Number Six", "Borg", "C-3PO", "K-9", "T-1000", "Johnny 5", "Marvin" };

        private readonly Guid m_identifier = Guid.NewGuid();
        private readonly string m_name;
        private readonly UserFlags m_flags;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public User(string name)
            : this(name, UserFlags.None)
        {
        }

        private User(string name, UserFlags flags)
        {
            Throw.IfEmpty(name, "name");
            m_name = name;
            m_flags = flags;
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

        public Guid Id
        {
            get { return m_identifier; }
        }

        public bool IsAI
        {
            get { return (m_flags & UserFlags.IsAI) == UserFlags.IsAI; }
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
            return string.Format("{0} [{1}]", Name, m_identifier);
        }

        public static User CreateAIUser()
        {
            // For kicks
            var name = ms_random.Choose(m_aiNames);
            return new User(name, UserFlags.IsAI);
        }

        #endregion

        #region Inner Types

        [Flags]
        private enum UserFlags
        {
            None = 0,
            IsAI = 1
        }

        #endregion
    }
}
