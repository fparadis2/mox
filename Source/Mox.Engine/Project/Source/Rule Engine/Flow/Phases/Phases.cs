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

namespace Mox
{
    /// <summary>
    /// Possible Phases.
    /// </summary>
    public enum Phases
    {
        /// <summary>
        /// Beginning (Untap, Upkeep, Draw)
        /// </summary>
        Beginning,
        /// <summary>
        /// Pre-combat Main
        /// </summary>
        PrecombatMain,
        /// <summary>
        /// Combat (Beginning of combat, Declare attackers, Declare blockers, Combat damage, End of combat)
        /// </summary>
        Combat,
        /// <summary>
        /// Post-combat Main
        /// </summary>
        PostcombatMain,
        /// <summary>
        /// End (End, Cleanup)
        /// </summary>
        End
    }

    public static class PhasesExtensions
    {
        public static bool IsMainPhase(this Phases phases)
        {
            switch (phases)
            {
                case Phases.Beginning:
                case Phases.Combat:
                case Phases.End:
                    return false;

                case Phases.PrecombatMain:
                case Phases.PostcombatMain:
                    return true;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}