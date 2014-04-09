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
using System.Diagnostics;
using NUnit.Framework;

namespace Mox.Collections
{
    [TestFixture]
    public class ImmutableStackTests
    {
        #region Variables

        private ImmutableStack<int> m_stack;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_stack = new ImmutableStack<int>();
        }

        #endregion

        #region Utilities

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsTrue(m_stack.IsEmpty);
            Assert.Collections.AreEqual(new int[0], m_stack);
        }

        [Test, Conditional("DEBUG")]
        public void Test_Cannot_peek_when_the_stack_is_empty()
        {
            Assert.Throws<InvalidOperationException>(() => m_stack.Peek());
        }

        [Test, Conditional("DEBUG")]
        public void Test_Cannot_pop_when_the_stack_is_empty()
        {
            Assert.Throws<InvalidOperationException>(() => m_stack.Pop());
        }

        [Test]
        public void Test_Push_returns_an_independent_stack()
        {
            ImmutableStack<int> newStack = m_stack.Push(3);
            Assert.IsFalse(newStack.IsEmpty);
            Assert.Collections.AreEqual(new[] { 3 }, newStack);

            ImmutableStack<int> newStack2 = m_stack.Push(5);
            Assert.IsFalse(newStack2.IsEmpty);
            Assert.Collections.AreEqual(new[] { 5 }, newStack2);

            Assert.IsTrue(m_stack.IsEmpty); // First stack is still empty
        }

        [Test]
        public void Test_Pop_returns_an_independent_stack()
        {
            ImmutableStack<int> newStack = m_stack.Push(3);
            newStack = newStack.Push(5);
            newStack = newStack.Pop();

            Assert.IsFalse(newStack.IsEmpty);
            Assert.Collections.AreEqual(new[] { 3 }, newStack);
        }

        #endregion
    }
}
