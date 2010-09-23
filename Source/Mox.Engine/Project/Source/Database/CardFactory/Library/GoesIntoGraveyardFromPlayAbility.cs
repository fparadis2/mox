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

namespace Mox.Database.Library
{
    /// <summary>
    /// A triggered ability that triggers when the source card goes into the graveyard from play.
    /// </summary>
    /// <remarks>
    /// Prototypes: 
    /// When ABC is put into a graveyard from play, XYZ happens.
    /// </remarks>
    public abstract class GoesIntoGraveyardFromPlayAbility : ZoneChangeTriggeredAbility
    {
        #region Properties

        protected override bool CanTriggerWhenSourceIsNotVisible
        {
            get
            {
                return true;
            }
        }

        protected override Zone.Id TriggerZone
        {
            get
            {
                return Zone.Id.Graveyard;
            }
        }

        #endregion

        #region Methods

        protected override bool IsTriggeringSourceZone(Zone zone)
        {
            return zone.ZoneId == Zone.Id.Battlefield;
        }

        #endregion
    }
}
