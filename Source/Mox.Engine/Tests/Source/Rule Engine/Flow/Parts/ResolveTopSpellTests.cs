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

using NUnit.Framework;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class ResolveTopSpellTests : PartTestBase
    {
        #region Variables

        private ResolveTopSpell m_part;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_part = new ResolveTopSpell();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Execute_pops_the_top_spell_and_returns_a_part_to_resolve_it()
        {
            Spell spell = new Spell(m_mockAbility, m_playerA);
            m_game.SpellStack.Push(spell);

            var result = Execute(m_part);

            Assert.Collections.IsEmpty(m_game.SpellStack);

            Assert.IsInstanceOf<ResolveSpell>(result);
            Assert.AreEqual(spell, ((ResolveSpell)result).Spell);
        }

        #endregion
    }
}
