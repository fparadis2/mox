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
using Mox.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Abilities
{
    public class TrackingEffectInstance : GlobalEffectInstance
    {
        #region Properties

        private readonly ContinuousAbility m_ability = null;
        public static readonly Property<ContinuousAbility> AbilityProperty = Property<ContinuousAbility>.RegisterProperty<TrackingEffectInstance>("Ability", instance => instance.m_ability);

        private readonly ObjectResolver m_targets = null;
        public static readonly Property<ObjectResolver> TargetsProperty = Property<ObjectResolver>.RegisterProperty<TrackingEffectInstance>("Targets", instance => instance.m_targets);

        #endregion

        #region Properties

        public ContinuousAbility Ability => m_ability;
        public ObjectResolver Targets => m_targets;

        private Game Game => (Game)Manager;

        #endregion

        #region Methods

        protected override IEnumerable<Object> InitObjects()
        {
            return Update();
        }

        private IEnumerable<Object> Update()
        {
            int totalObjects = 0;

            foreach (var target in m_targets.Resolve(m_ability))
            {
                totalObjects++;

                if (AddAffectedObject(target))
                {
                    yield return target;
                }
            }

            if (totalObjects != AffectedObjectCount)
            {
                var targets = m_targets.Resolve(m_ability).ToList();

                foreach (var affectedObject in AffectedObjects.ToList())
                {
                    if (!targets.Contains(affectedObject))
                    {
                        if (RemoveAffectedObject(affectedObject))
                        {
                            yield return affectedObject;
                        }
                    }
                }
            }
        }

        protected override IEnumerable<Object> Invalidate(Object sender, PropertyBase property)
        {            
            if (m_targets.Invalidate(property))
            {
                foreach (var obj in Update())
                {
                    yield return obj;
                }
            }

            foreach (var obj in BaseInvalidate(sender, property))
            {
                yield return obj;
            }
        }

        private IEnumerable<Object> BaseInvalidate(Object sender, PropertyBase property)
        {
            return base.Invalidate(sender, property);
        }

        #endregion
    }
}
