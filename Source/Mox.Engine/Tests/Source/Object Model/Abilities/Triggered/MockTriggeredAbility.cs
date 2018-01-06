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

namespace Mox.Abilities
{
    public class MockTriggeredAbility : TriggeredAbility, IMockAbility<MockTriggeredAbility.Impl>
    {
        #region Inner Types

        public abstract class Impl : MockAbility.Impl
        {
        }

        #endregion

        #region Properties

        public Impl Implementation
        {
            get;
            internal set;
        }

        MockAbility.Impl IMockAbility.BaseImplementation
        {
            get { return Implementation; }
        }

        #endregion

        #region Overrides of Ability

        /// <summary>
        /// Initializes the given spell and returns the "pre payment" costs associated with the spell (asks players for modal choices, {X} choices, etc...)
        /// </summary>
        /// <param name="spell"></param>
        public override void Play(Spell spell)
        {
            Implementation.Play(spell);
        }

        #endregion

        #region Helpers

        public new void Trigger(object context)
        {
            Manager.GlobalData.TriggerAbility(this, context);
        }

        #endregion
    }
}
