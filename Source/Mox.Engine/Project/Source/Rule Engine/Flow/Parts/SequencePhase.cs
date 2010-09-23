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

using Mox.Flow.Phases;

namespace Mox.Flow.Parts
{
    /// <summary>
    /// Plays a phase
    /// </summary>
    public class SequencePhase : MTGPart
    {
        #region Variables

        private readonly Phase m_phase;

        #endregion

        #region Constructor

        public SequencePhase(Player player, Phase phase)
            : base(player)
        {
            Throw.IfNull(phase, "phase");

            m_phase = phase;
        }

        #endregion

        #region Properties

        public Phase Phase
        {
            get { return m_phase; }
        }

        #endregion

        #region Overrides of Part<IGameController>

        public override Part<IGameController> Execute(Context context)
        {
            return m_phase.Sequence(context, GetPlayer(context));
        }

        #endregion
    }
}
