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
    /// <summary>
    /// A triggered ability that triggers when the source card comes into play.
    /// </summary>
    /// <remarks>
    /// Prototypes: 
    /// When ABC comes into play, XYZ happens.
    /// Whenever a creature comes into play, XYZ happens.
    /// </remarks>
    public abstract class ComesIntoPlayAbility : ZoneChangeTriggeredAbility
    {
        // Default is already to trigger in battlefield
    }

    /// <summary>
    /// A triggered ability that triggers when any card comes into play under your control.
    /// </summary>
    /// <remarks>
    /// Prototypes: 
    /// Whenever a ABC comes into play under your control, XYZ happens.
    /// </remarks>
    public abstract class AnyComesIntoPlayUnderControlAbility : ComesIntoPlayAbility
    {
        #region Methods

        protected override bool IsValidCard(Card card)
        {
            return card.Controller == Controller;
        }

        #endregion
    }

    /// <summary>
    /// A triggered ability that triggers when any card comes into play under your control.
    /// </summary>
    /// <remarks>
    /// Prototypes: 
    /// Whenever a creature comes into play under your control, XYZ happens.
    /// </remarks>
    public abstract class AnyCreatureComesIntoPlayUnderControlAbility : AnyComesIntoPlayUnderControlAbility
    {
        #region Methods

        protected override bool IsValidCard(Card card)
        {
            return base.IsValidCard(card) && card.Is(Type.Creature);
        }

        #endregion
    }

    /// <summary>
    /// A triggered ability that triggers when the card on which it is comes into play under your control.
    /// </summary>
    /// <remarks>
    /// Prototypes: 
    /// Whenever this creature comes into play under your control, XYZ happens.
    /// </remarks>
    public abstract class ThisCreatureComesIntoPlayUnderControlAbility : ComesIntoPlayAbility
    {
        protected override bool CanTriggerWhenSourceIsNotVisible
        {
            get
            {
                return true;
            }
        }
    }

    /// <summary>
    /// A triggered ability that triggers when any creature comes into play.
    /// </summary>
    /// <remarks>
    /// Prototypes: 
    /// Whenever a creature comes into play, XYZ happens.
    /// </remarks>
    public abstract class AnyCreatureComesIntoPlayAbility : ComesIntoPlayAbility
    {
        #region Methods

        protected override bool IsValidCard(Card card)
        {
            return base.IsValidCard(card) && card.Is(Type.Creature);
        }

        #endregion
    }
}
