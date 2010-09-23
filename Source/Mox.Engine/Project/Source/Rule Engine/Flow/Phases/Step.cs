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

using Mox.Flow;
using Mox.Flow.Parts;

namespace Mox
{
    /// <summary>
    /// Possible steps.
    /// </summary>
    public enum Steps
    {
        Untap,
        Upkeep,
        Draw,
        BeginningOfCombat,
        DeclareAttackers,
        DeclareBlockers,
        CombatDamage,
        EndOfCombat,
        End,
        Cleanup
    }

    namespace Flow.Phases
    {
        /// <summary>
        /// Base class for steps.
        /// </summary>
        public class Step
        {
            #region Variables

            private readonly Steps m_type;

            #endregion

            #region Constructor

            public Step(Steps type)
            {
                m_type = type;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Type of the step.
            /// </summary>
            public Steps Type
            {
                get { return m_type; }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Sequences the step.
            /// </summary>
            /// <param name="context"></param>
            /// <param name="player"></param>
            public MTGPart Sequence(MTGPart.Context context, Player player)
            {
                context.Game.State.CurrentStep = Type;
                return SequenceImpl(context, player);
            }

            /// <summary>
            /// Sequences the step.
            /// </summary>
            protected virtual MTGPart SequenceImpl(MTGPart.Context context, Player player)
            {
                context.Schedule(new PlayUntilAllPlayersPassAndTheStackIsEmpty(player));
                return null;
            }

            #endregion
        }
    }
}
