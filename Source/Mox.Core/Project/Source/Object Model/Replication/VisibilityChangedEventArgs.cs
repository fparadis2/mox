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
    public class VisibilityChangedEventArgs<TKey> : EventArgs
    {
        #region Variables

        private readonly Object m_object;
        private readonly TKey m_key;
        private readonly bool m_visible;

        #endregion

        #region Constructor

        public VisibilityChangedEventArgs(Object obj, TKey key, bool visible)
        {
            m_object = obj;
            m_key = key;
            m_visible = visible;
        }

        #endregion

        #region Properties

        public Object Object
        {
            get { return m_object; }
        }

        public TKey Key
        {
            get { return m_key; }
        }

        public bool Visible
        {
            get { return m_visible; }
        }

        #endregion
    }
}
