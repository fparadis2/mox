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
    /// An effect that modifies a value of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public abstract class Effect<TValue> : EffectBase
    {
        #region Constructor

        protected Effect(Property<TValue> property)
            : base(property)
        {
        }

        #endregion

        #region Methods

        public abstract TValue Modify(Object owner, TValue value);

        internal override sealed object ModifyInternal(Object owner, object value)
        {
            return Modify(owner, (TValue)value);
        }

        #endregion
    }
}
