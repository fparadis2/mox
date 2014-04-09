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

namespace Mox
{
    /// <summary>
    /// Base class for effects.
    /// </summary>
    [Serializable]
    public abstract class EffectBase : IComparable<EffectBase>
    {
        #region Variables

        private readonly PropertyIdentifier m_property;

        #endregion

        #region Constructor

        internal EffectBase(PropertyBase property)
        {
            Throw.IfNull(property, "property");
            Throw.InvalidArgumentIf(!property.IsModifiable, "Property is not modifiable", "property");
            m_property = new PropertyIdentifier(property);
        }

        #endregion

        #region Properties

        public PropertyBase Property
        {
            get { return m_property.Property; }
        }

        #endregion

        #region Methods

        internal abstract object ModifyInternal(Object owner, object value);

        public virtual int CompareTo(EffectBase other)
        {
            return 0;
        }

        protected internal virtual bool Invalidate(PropertyBase property)
        {
            return false;
        }

        #endregion
    }
}
