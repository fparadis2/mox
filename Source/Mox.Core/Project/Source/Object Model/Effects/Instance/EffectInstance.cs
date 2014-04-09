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
    public abstract class EffectInstance : Object, IComparable<EffectInstance>, IComparable
    {
        #region Variables

        private readonly EffectBase m_effect = null;
        public static readonly Property<EffectBase> EffectProperty = Property<EffectBase>.RegisterProperty<EffectInstance>("Effect", instance => instance.m_effect);

        #endregion

        #region Properties

        public EffectBase Effect
        {
            get { return m_effect; }
        }

        protected abstract IEnumerable<Object> AffectedObjects
        {
            get;
        }

        #endregion

        #region Methods

        protected internal virtual IEnumerable<Object> Invalidate(Object sender, PropertyBase property)
        {
            if (Effect.Invalidate(property))
            {
                foreach (Object obj in AffectedObjects)
                {
                    yield return obj;
                }
            }
        }

        public virtual int CompareTo(EffectInstance other)
        {
            return Effect.CompareTo(other.Effect);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((EffectInstance)obj);
        }

        #endregion
    }
}
