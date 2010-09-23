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

namespace Mox.Effects
{
    public enum EffectDependencyLayer
    {
        Copy, // 1
        ControlChanging, // 2
        TextChanging, // 3
        TypeChanging, // 4
        ColorChanging, // 5
        AbilityAdding, // 6

        PT_CharacteristicsDefiningAbility, // 7a
        PT_Set, // 7b
        PT_Modify, // 7c
        PT_Counters, // 7d
        PT_Switch, // 7e
    }

    internal interface IMTGEffect
    {
        EffectDependencyLayer DependendencyLayer { get; }
    }

    [Serializable]
    public abstract class MTGEffect<TValue> : Effect<TValue>, IMTGEffect
    {
        #region Constructor

        protected MTGEffect(Property<TValue> property)
            : base(property)
        {
        }

        #endregion

        #region Properties

        public abstract EffectDependencyLayer DependendencyLayer { get; }

        #endregion

        #region Methods

        public override int CompareTo(EffectBase other)
        {
            int compare = base.CompareTo(other);
            if (compare == 0)
            {
                IMTGEffect otherEffect = other as IMTGEffect;
                if (!ReferenceEquals(otherEffect, null))
                {
                    compare = DependendencyLayer.CompareTo(otherEffect.DependendencyLayer);
                }
            }

            return compare;
        }

        #endregion
    }
}
