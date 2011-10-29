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
    /// <summary>
    /// An access control strategy where everything is read/writeable by everyone.
    /// </summary>
    public class OpenAccessControlStrategy<TUser> : IAccessControlStrategy<TUser>
    {
        #region Methods

        public void Dispose()
        {
        }

        public UserAccess GetUserAccess(TUser user, Object @object)
        {
            Throw.IfNull(@object, "object");
            return UserAccess.All;
        }

        #endregion

        #region Events

        public event EventHandler<UserAccessChangedEventArgs<TUser>> UserAccessChanged
        {
            add { }
            remove { }
        }

        #endregion
    }
}
