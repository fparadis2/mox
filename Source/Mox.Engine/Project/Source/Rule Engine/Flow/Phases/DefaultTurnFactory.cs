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
using System.Linq;
using System.Text;

namespace Mox.Flow.Phases
{
    /// <summary>
    /// Creates a canonical turn.
    /// </summary>
    public class DefaultTurnFactory : ITurnFactory
    {
        #region ITurnFactory Members

        /// <summary>
        /// Creates a canonical turn.
        /// </summary>
        /// <returns></returns>
        public Turn CreateTurn()
        {
            Turn turn = new Turn();

            turn.Phases.Add(CreateBeginningPhase());
            turn.Phases.Add(new Phase(Mox.Phases.PrecombatMain));
            turn.Phases.Add(CreateCombatPhase());
            turn.Phases.Add(new Phase(Mox.Phases.PostcombatMain));
            turn.Phases.Add(CreateEndPhase());

            return turn;
        }

        private Phase CreateBeginningPhase()
        {
            Phase beginningPhase = new Phase(Mox.Phases.Beginning);
            beginningPhase.Steps.Add(new UntapStep());
            beginningPhase.Steps.Add(new UpkeepStep());
            beginningPhase.Steps.Add(new DrawStep());
            return beginningPhase;
        }

        public static Phase CreateCombatPhase()
        {
            Phase combatPhase = new Phase(Mox.Phases.Combat);
            combatPhase.Steps.Add(new BeginningOfCombatStep());
            combatPhase.Steps.Add(new DeclareAttackersStep());
            combatPhase.Steps.Add(new DeclareBlockersStep());
            combatPhase.Steps.Add(new CombatDamageStep());
            combatPhase.Steps.Add(new EndOfCombatStep());
            return combatPhase;
        }

        private Phase CreateEndPhase()
        {
            Phase endPhase = new Phase(Mox.Phases.End);
            endPhase.Steps.Add(new EndOfTurnStep());
            endPhase.Steps.Add(new CleanupStep());
            return endPhase;
        }

        #endregion
    }
}
