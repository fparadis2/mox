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
namespace Mox.Abilities
{
    public class MockAbility : Ability
    {
        public AbilityType? MockedAbilityType;
        public override AbilityType AbilityType => MockedAbilityType ?? base.AbilityType;

        public AbilitySpeed? MockedAbilitySpeed;
        public override AbilitySpeed AbilitySpeed => MockedAbilitySpeed ?? base.AbilitySpeed;

        public bool? MockedIsManaAbility;
        public override bool IsManaAbility => MockedIsManaAbility ?? base.IsManaAbility;

        private int m_mockProperty;
        public static readonly Property<int> MockPropertyProperty = Property<int>.RegisterProperty<MockAbility>("MockProperty", a => a.m_mockProperty);

        public int MockProperty
        {
            get { return m_mockProperty; }
            set { SetValue(MockPropertyProperty, value, ref m_mockProperty); }
        }
    }
}
