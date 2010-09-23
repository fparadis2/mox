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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox
{
    /// <summary>
    /// Controller Changed event args.
    /// </summary>
    public class PropertyChangedEventArgs : EventArgs
    {
        #region Variables

        private readonly Object m_object;
        private readonly PropertyBase m_property;
        private readonly object m_oldValue;
        private readonly object m_newValue;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public PropertyChangedEventArgs(Object obj, PropertyBase property, object oldValue, object newValue)
        {
            m_object = obj;
            m_property = property;
            m_oldValue = oldValue;
            m_newValue = newValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The object on which the change occurs.
        /// </summary>
        public Object Object
        {
            get { return m_object; }
        }

        /// <summary>
        /// The property that changed.
        /// </summary>
        public PropertyBase Property
        {
            get { return m_property; }
        }

        /// <summary>
        /// The value of the property before the change.
        /// </summary>
        public object OldValue
        {
            get { return m_oldValue; }
        }

        /// <summary>
        /// The value of the property after the change.
        /// </summary>
        public object NewValue
        {
            get { return m_newValue; }
        }

        #endregion
    }

    public class PropertyChangingEventArgs : PropertyChangedEventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public PropertyChangingEventArgs(Object obj, PropertyBase property, object oldValue, object newValue)
            : base(obj, property, oldValue, newValue)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Whether to cancel the controller change.
        /// </summary>
        public bool Cancel
        {
            get;
            set;
        }

        #endregion
    }
}
