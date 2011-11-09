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
using System.Diagnostics;
using System.Linq;

namespace Mox.Flow
{
    /// <summary>
    /// Describes how a part accesses the controller.
    /// </summary>
    /// <remarks>
    /// This mainly exists as an optimisation since Sequencer can skip some operations when access is lighter.
    /// </remarks>
    public enum ControllerAccess
    {
        /// <summary>
        /// Part doesn't need the controller.
        /// </summary>
        None,
        /// <summary>
        /// Part uses the controller once to make a single choice.
        /// </summary>
        Single,
        /// <summary>
        /// Part uses the controller more than once to make multiple choices during the same part.
        /// </summary>
        Multiple
    }

    partial class Part<TController>
    {
        /// <summary>
        /// A sequencing context.
        /// </summary>
        public class Context
        {
            #region Variables

            private readonly ControllerAccess m_controllerAccess;

#if DEBUG
            private int m_numControllerAccess;
#endif

            private readonly Sequencer<TController> m_sequencer;
            private readonly TController m_controller;
            private readonly List<Part<TController>> m_scheduledParts = new List<Part<TController>>();

            #endregion

            #region Constructor

            public Context(Sequencer<TController> sequencer, TController controller, ControllerAccess controllerAccess)
            {
                Throw.IfNull(sequencer, "sequencer");
                
                m_sequencer = sequencer;
                m_controller = controller;
                m_controllerAccess = controllerAccess;
            }

            #endregion

            #region Properties

            public Sequencer<TController> Sequencer
            {
                get { return m_sequencer; }
            }

            public Game Game
            {
                get { return Sequencer.Game; }
            }

            public TController Controller
            {
                get
                {
#if DEBUG
                    ValidateControllerAccess();
#endif
                    return m_controller;
                }
            }

            public ControllerAccess ControllerAccess
            {
                get { return m_controllerAccess; }
            }

            public IEnumerable<Part<TController>> ScheduledParts
            {
                get
                {
                    return m_scheduledParts.AsEnumerable().Reverse();
                }
            }

            /// <summary>
            /// Whether to stop the sequencer after the current part. Useful for AI.
            /// </summary>
            public bool Stop { get; set; }

            #endregion

            #region Methods

            /// <summary>
            /// Schedules a part to be executed in the current sequence.
            /// </summary>
            /// <param name="part"></param>
            public void Schedule(Part<TController> part)
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

#if DEBUG
            private void ValidateControllerAccess()
            {
                Debug.Assert(m_controllerAccess != ControllerAccess.None, "Invalid access to controller. Part.ControllerAccess returns None, did you forget to override the property?");
                Debug.Assert(m_controllerAccess != ControllerAccess.Single || m_numControllerAccess == 0, "Invalid access to controller. Part.ControllerAccess returns Single and it is the second time you access this property");
                m_numControllerAccess++;
            }
#endif

            #endregion
        }
    }

    partial class NewPart
    {
        /// <summary>
        /// A sequencing context.
        /// </summary>
        public class Context
        {
            #region Variables

            private readonly NewSequencer m_sequencer;
            private readonly List<NewPart> m_scheduledParts = new List<NewPart>();

            #endregion

            #region Constructor

            public Context(NewSequencer sequencer)
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

            public IEnumerable<NewPart> ScheduledParts
            {
                get
                {
                    return m_scheduledParts.AsEnumerable().Reverse();
                }
            }

#warning still needed?
            /// <summary>
            /// Whether to stop the sequencer after the current part. Useful for AI.
            /// </summary>
            public bool Stop { get; set; }

            #endregion

            #region Methods

            /// <summary>
            /// Schedules a part to be executed in the current sequence.
            /// </summary>
            /// <param name="part"></param>
            public void Schedule(NewPart part)
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
