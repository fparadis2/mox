using System;

namespace Mox.UI
{
    public static class PlayerIdentityRepository
    {
        #region Variables

        private static readonly UserIdentity ms_local = new UserIdentity();

        #endregion

        #region Constructor

        static PlayerIdentityRepository()
        {
            ms_local.Name = Environment.UserName;
        }

        #endregion

        #region Properties

        public static IUserIdentity Local
        {
            get { return ms_local; }
        }

        #endregion
    }
}
