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
using System.Diagnostics;
using System.Linq;

namespace Mox.Abilities
{
    /// <summary>
    /// LandWalk
    /// </summary>
    public abstract class LandWalkAbility : EvasionAbility
    {
        public override sealed bool CanBlock(Card attacker, Card blocker)
        {
            // Cannot block if defender controls at least one affected land
            return !blocker.Controller.Battlefield.Where(c => c.Is(Type.Land)).Any(IsAffectedLand);
        }

        protected abstract bool IsAffectedLand(Card land);
    }

    public class BasicLandWalkAbility : LandWalkAbility
    {
        private SubType m_type;
        private static readonly Property<SubType> LandTypeProperty = Property<SubType>.RegisterProperty<BasicLandWalkAbility>("Type", a => a.m_type, PropertyFlags.Private);

        public SubType Type
        {
            get { return m_type; }
            set
            {
                Validate_Is_Basic_Land(value, "Type should be a basic land type");
                SetValue(LandTypeProperty, value, ref m_type);
            }
        }

        protected override bool IsAffectedLand(Card land)
        {
            Validate_Is_Basic_Land(Type, "BasicLandWalkAbility.Type has not been set to a basic type");
            return land.Is(Type);
        }

        [Conditional("DEBUG")]
        private static void Validate_Is_Basic_Land(SubType type, string msg)
        {
            SubType[] basicLands = new[] {SubType.Plains, SubType.Swamp, SubType.Mountain, SubType.Island, SubType.Forest};

            Debug.Assert(basicLands.Contains(type), msg);
        }
    }
}
