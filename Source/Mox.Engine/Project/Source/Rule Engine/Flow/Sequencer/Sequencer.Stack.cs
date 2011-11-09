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

namespace Mox.Flow
{
    partial class NewSequencer
    {
        #region Variables

#if DEBUG
        private ImmutableStack<Argument> m_argumentStack = new ImmutableStack<Argument>();
#else
        private ImmutableStack<object> m_argumentStack = new ImmutableStack<object>();
#endif

        #endregion

        #region Properties

        internal bool IsArgumentStackEmpty
        {
            get { return m_argumentStack.IsEmpty; }
        }

        #endregion

        #region Methods

        internal void PushArgument(object value, object debugToken)
        {
#if DEBUG
            m_argumentStack = m_argumentStack.Push(new Argument(value, debugToken));
#else
            m_argumentStack = m_argumentStack.Push(value);
#endif
        }

        internal T PeekArgument<T>(object debugToken)
        {
#if DEBUG
            Argument arg = m_argumentStack.Peek();
            Throw.InvalidOperationIf(debugToken != arg.DebugToken, string.Format("Expected to pop {0} but popped {1}", debugToken, arg.DebugToken));
            return (T)arg.Value;
#else
            return (T)m_argumentStack.Peek();
#endif
        }

        internal T PopArgument<T>(object debugToken)
        {
            T value = PeekArgument<T>(debugToken);
            m_argumentStack = m_argumentStack.Pop();
            return value;
        }

        #endregion

        #region Inner Types

#if DEBUG

        private class Argument
        {
            public readonly object DebugToken;
            public readonly object Value;

            public Argument(object value, object debugToken)
            {
                Throw.IfNull(debugToken, "debugToken");

                Value = value;
                DebugToken = debugToken;
            }
        }

#endif

        #endregion
    }

    partial class Sequencer<TController>
    {
        #region Variables

#if DEBUG
        private ImmutableStack<Argument> m_argumentStack = new ImmutableStack<Argument>();
#else
        private ImmutableStack<object> m_argumentStack = new ImmutableStack<object>();
#endif

        #endregion

        #region Properties

        internal bool IsArgumentStackEmpty
        {
            get { return m_argumentStack.IsEmpty; }
        }

        #endregion

        #region Methods

        internal void PushArgument(object value, object debugToken)
        {
#if DEBUG
            m_argumentStack = m_argumentStack.Push(new Argument(value, debugToken));
#else
            m_argumentStack = m_argumentStack.Push(value);
#endif
        }

        internal T PopArgument<T>(object debugToken)
        {
#if DEBUG
            Argument arg = m_argumentStack.Peek();
            Throw.InvalidOperationIf(debugToken != arg.DebugToken, string.Format("Expected to pop {0} but popped {1}", debugToken, arg.DebugToken));
            T value = (T)arg.Value;
#else
            T value = (T)m_argumentStack.Peek();
#endif

            m_argumentStack = m_argumentStack.Pop();
            return value;
        }

        #endregion

        #region Inner Types

#if DEBUG

        private class Argument
        {
            public readonly object DebugToken;
            public readonly object Value;

            public Argument(object value, object debugToken)
            {
                Throw.IfNull(debugToken, "debugToken");

                Value = value;
                DebugToken = debugToken;
            }
        }

#endif

        #endregion
    }
}
