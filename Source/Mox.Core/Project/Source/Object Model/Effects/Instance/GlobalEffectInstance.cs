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

namespace Mox
{
    public abstract class GlobalEffectInstance : EffectInstance
    {
        #region Variables

        private readonly HashSet<Object> m_affectedObjects = new HashSet<Object>(); // Cached list of affected objects

        #endregion

        #region Properties

        protected override IEnumerable<Object> AffectedObjects
        {
            get { return m_affectedObjects; }
        }

        protected int AffectedObjectCount
        {
            get { return m_affectedObjects.Count; }
        }

        #endregion

        #region Methods

        protected internal override void Init()
        {
            base.Init();

            foreach (var obj in InitObjects())
            {
                obj.InvalidateEffects();
            }
        }

        protected internal override void Uninit()
        {
            Clear();

            base.Uninit();
        }

        protected abstract IEnumerable<Object> InitObjects();

        protected bool AddAffectedObject(Object obj)
        {
            if (m_affectedObjects.Add(obj))
            {
                obj.AppliedEffects.Add(this);
                return true;
            }

            return false;
        }

        protected bool RemoveAffectedObject(Object obj)
        {
            if (m_affectedObjects.Remove(obj))
            {
                obj.AppliedEffects.Remove(this);
                return true;
            }

            return false;
        }

        private void Clear()
        {
            foreach (Object obj in m_affectedObjects)
            {
                obj.AppliedEffects.Remove(this);
                obj.InvalidateEffects();
            }

            m_affectedObjects.Clear();
        }

        #endregion
    }
}
