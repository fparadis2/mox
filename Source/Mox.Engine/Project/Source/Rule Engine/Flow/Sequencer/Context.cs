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

namespace Mox.Flow
{
    partial class Part
    {
        /// <summary>
        /// A sequencing context.
        /// </summary>
        public class Context
        {
            #region Variables

            private readonly Sequencer m_sequencer;
            private readonly List<Part> m_scheduledParts = new List<Part>();

            #endregion

            #region Constructor

            public Context(Sequencer sequencer)
            {
                Throw.IfNull(sequencer, "sequencer");

                m_sequencer = sequencer;
            }

            #endregion

            #region Properties

            public Game Game
            {
                get { return m_sequencer.Game; }
            }

            public IEnumerable<Part> ScheduledParts
            {
                get
                {
                    return m_scheduledParts.AsEnumerable().Reverse();
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Schedules a part to be executed in the current sequence.
            /// </summary>
            /// <param name="part"></param>
            public void Schedule(Part part)
            {
                m_scheduledParts.Add(part);
            }

            public void PushArgument(object arg, object debugToken)
            {
                m_sequencer.PushArgument(arg, debugToken);
            }

            public T PopArgument<T>(object debugToken)
            {
                return m_sequencer.PopArgument<T>(debugToken);
            }

            public T PeekArgument<T>(object debugToken)
            {
                return m_sequencer.PeekArgument<T>(debugToken);
            }

            #endregion
        }
    }
}
