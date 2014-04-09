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
    public class LocalEffectInstance : EffectInstance
    {
        #region Variables

        private readonly Object m_affectedObject = null;
        public static readonly Property<Object> AffectedObjectProperty = Property<Object>.RegisterProperty<LocalEffectInstance>("AffectedObject", instance => instance.m_affectedObject);

        #endregion

        #region Properties

        public Object AffectedObject
        {
            get { return m_affectedObject; }
        }

        protected override IEnumerable<Object> AffectedObjects
        {
            get 
            {
                yield return AffectedObject;
            }
        }

        #endregion

        #region Methods

        protected internal override void Init()
        {
            base.Init();

            AffectedObject.AppliedEffects.Add(this);
            AffectedObject.InvalidateEffects();
        }

        protected internal override void Uninit()
        {
            AffectedObject.AppliedEffects.Remove(this);
            AffectedObject.InvalidateEffects();

            base.Uninit();
        }

        #endregion
    }
}
