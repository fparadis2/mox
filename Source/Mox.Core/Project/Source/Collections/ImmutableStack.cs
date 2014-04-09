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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mox
{
    /// <summary>
    /// An immutable stack (can be passed around freely because it will never change).
    /// </summary>
    public class ImmutableStack<T> : IEnumerable<T>
    {
        #region Variables

        private readonly T m_head;
        private readonly ImmutableStack<T> m_tail;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an empty stack.
        /// </summary>
        public ImmutableStack()
            : this(default(T), null)
        {
        }

        private ImmutableStack(T head, ImmutableStack<T> tail)
        {
            m_head = head;
            m_tail = tail;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Whether this is an empty stack.
        /// </summary>
        public bool IsEmpty
        {
            get { return m_tail == null; } 
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the top item in the stack.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">If the stack is empty.</exception>
        /// <returns></returns>
        public T Peek()
        {
            ThrowIfEmpty();
            return m_head;
        }

        /// <summary>
        /// Returns a stack where the top item has been popped.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">If the stack is empty.</exception>
        /// <returns></returns>
        public ImmutableStack<T> Pop()
        {
            ThrowIfEmpty();
            return m_tail;
        }

        /// <summary>
        /// Returns a stack where and item has been pushed.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">If the stack is empty.</exception>
        /// <returns></returns>
        public ImmutableStack<T> Push(T value)
        {
            return new ImmutableStack<T>(value, this); 
        }

        [Conditional("DEBUG")]
        private void ThrowIfEmpty()
        {
            Throw.InvalidOperationIf(IsEmpty, "Stack is empty");
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            for (ImmutableStack<T> stack = this; !stack.IsEmpty; stack = stack.Pop())
            {
                yield return stack.Peek();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
