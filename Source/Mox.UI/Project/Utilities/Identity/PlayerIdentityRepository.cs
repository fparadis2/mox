using System;

namespace Mox.UI
{
    public static class PlayerIdentityRepository
    {
        #region Variables

        private static readonly PlayerIdentity ms_local = new PlayerIdentity();

        #endregion

        #region Constructor

        static PlayerIdentityRepository()
        {
            ms_local.Name = Environment.UserName;
        }

        #endregion

        #region Properties

        public static IPlayerIdentity Local
        {
            get { return ms_local; }
        }

        #endregion
    }
}
