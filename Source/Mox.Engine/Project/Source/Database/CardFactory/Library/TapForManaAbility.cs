﻿// Copyright (c) François Paradis
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

namespace Mox.Database.Library
{
    /// <summary>
    /// An in-play ability that produces mana.
    /// </summary>
    /// <remarks>
    /// Prototype: 
    /// T: Add X to your mana pool
    /// </remarks>
    public class TapForManaAbility : InPlayAbility
    {
        #region Variables

        private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty("Color", typeof(TapForManaAbility));

        #endregion

        #region Properties

        public Color Color
        {
            get { return GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        #endregion

        #region Overrides of Ability

        public override ManaAbilityOutcome ManaOutcome
        {
            get
            {
                return ManaAbilityOutcome.OfColor(Color);
            }
        }

        /// <summary>
        /// Initializes the given spell and returns the "pre payment" costs associated with the spell (asks players for modal choices, {X} choices, etc...)
        /// </summary>
        /// <param name="spell"></param>
        public override void Play(Spell spell)
        {
            spell.Costs.Add(Tap(spell.Source));

            spell.Effect = (s, c) => OnResolve(s);
        }

        protected virtual void OnResolve(Spell spell)
        {
            spell.Controller.ManaPool[Color] += 1;
        }

        #endregion
    }
}
