using System;

namespace Mox.Lobby
{
    /// <summary>
    /// A user in the lobby (player, spectator)
    /// </summary>
    public struct User
    {
        #region Variables

        public readonly Guid Id;
        public string Name;

        #endregion

        #region Constructor

        public User(Guid id)
        {
            Id = id;
            Name = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            User other = (User)obj;
            return other.Id == Id;
        }

        /// <summary>
        /// ==
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(User a, User b)
        {
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
            return string.Format("{0} [{1}]", Name, Id);
        }

        #endregion
    }
}
