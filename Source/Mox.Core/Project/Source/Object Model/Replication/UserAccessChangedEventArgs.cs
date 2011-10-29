// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace Mox.Replication
{
    public class UserAccessChangedEventArgs<TUser> : EventArgs
    {
        #region Variables

        private readonly Object m_object;
        private readonly TUser m_user;
        private readonly Flags<UserAccess> m_access;

        #endregion

        #region Constructor

        public UserAccessChangedEventArgs(Object obj, TUser user, UserAccess access)
        {
            m_object = obj;
            m_user = user;
            m_access = access;
        }

        #endregion

        #region Properties

        public Object Object
        {
            get { return m_object; }
        }

        public TUser User
        {
            get { return m_user; }
        }

        public Flags<UserAccess> Access
        {
            get { return m_access; }
        }

        #endregion
    }
}
