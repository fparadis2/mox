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
using System.Linq;
using Mox.Transactions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Mox
{
    [TestFixture]
    public class SpellStackTests : BaseGameTests
    {
        #region Variables

        private SpellStack m_stack;
        private Spell m_spell1, m_spell2;

        #endregion

        #region Setup / Teardown

        public override void  Setup()
        {
            base.Setup();

            m_stack = m_game.SpellStack;

            m_mockery.Test(() =>
            {
                m_spell1 = new Spell(m_game, m_mockAbility, m_playerA);
                m_spell2 = new Spell(m_game, m_mockAbility, m_playerA);
            });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new SpellStack(null); });
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsTrue(m_stack.IsEmpty);
            Assert.Collections.IsEmpty(m_stack);
        }

        [Test]
        public void Test_Push_pushes_a_spell_on_the_stack()
        {
            m_stack.Push(m_spell1);

            Assert.Collections.AreEqual(new[] { m_spell1 }, m_stack);
            Assert.IsFalse(m_stack.IsEmpty);
        }

        [Test]
        public void Test_can_push_multiple_spells()
        {
            m_stack.Push(m_spell1);
            m_stack.Push(m_spell2);

            Assert.AreEqual(2, m_stack.Count());
            Assert.AreEqual(m_spell2, m_stack.Peek());
        }

        [Test]
        public void Test_Cannot_push_a_null_spell()
        {
            Assert.Throws<ArgumentNullException>(() => m_stack.Push(null));
        }

        [Test]
        public void Test_Cannot_peek_an_empty_stack()
        {
#if DEBUG
            Assert.Throws<InvalidOperationException>(() => m_stack.Peek());
#endif
        }

        [Test]
        public void Test_Cannot_pop_an_empty_stack()
        {
#if DEBUG
            Assert.Throws<InvalidOperationException>(() => m_stack.Pop());
#endif
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("[Zone: SpellStack]", m_stack.ToString());
        }

        [Test]
        public void Test_Pop_returns_the_top_spell_and_removes_it_from_the_stack()
        {
            m_stack.Push(m_spell1);
            m_stack.Push(m_spell2);

            Assert.AreEqual(m_spell2, m_stack.Pop());
            Assert.Collections.AreEqual(new[] { m_spell1 }, m_stack);

            Assert.AreEqual(m_spell1, m_stack.Pop());
            Assert.Collections.IsEmpty(m_stack);
        }

        [Test]
        public void Test_Pushing_is_undoable()
        {
            Assert.IsUndoRedoable(m_game.Controller, 
                () => Assert.Collections.IsEmpty(m_stack), 
                () => m_stack.Push(m_spell1), 
                () => Assert.Collections.AreEqual(new[] { m_spell1 }, m_stack));
        }

        [Test]
        public void Test_Popping_is_undoable()
        {
            m_stack.Push(m_spell1);
            m_stack.Push(m_spell2);

            Assert.IsUndoRedoable(m_game.Controller,
                () => Assert.Collections.AreEqual(new[] { m_spell2, m_spell1 }, m_stack),
                () => m_stack.Pop(),
                () => Assert.Collections.AreEqual(new[] { m_spell1 }, m_stack));
        }

        #endregion
    }
}
