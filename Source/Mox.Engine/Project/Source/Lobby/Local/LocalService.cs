using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby
{
    internal abstract class LocalService
    {
        #region Variables

        #endregion

        #region Properties

        protected User User
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
