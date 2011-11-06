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

using Mox.Flow.Parts;

namespace Mox
{
    namespace Flow.Phases
    {
        /// <summary>
        /// A single phase.
        /// </summary>
        public class Phase
        {
            #region Variables

            private readonly Mox.Phases m_type;
            private readonly List<Step> m_steps = new List<Step>();

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="type"></param>
            public Phase(Mox.Phases type)
            {
                m_type = type;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Type of the phase.
            /// </summary>
            public Mox.Phases Type
            {
                get { return m_type; }
            }

            /// <summary>
            /// Steps contained in this phase.
            /// </summary>
            public IList<Step> Steps
            {
                get { return m_steps; }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Sequences this phase.
            /// </summary>
            public virtual NewPart Sequence(NewPart.Context context, Player activePlayer)
            {
                context.Game.State.CurrentPhase = Type;

                if (m_steps.Count == 0)
                {
                    context.Schedule(new PlayUntilAllPlayersPassAndTheStackIsEmpty(activePlayer));
                }
                else
                {
                    foreach (Step step in Steps)
                    {
                        context.Schedule(new SequenceStep(activePlayer, step));
                    }
                }

                return null;
            }

            #endregion
        }
    }
}
