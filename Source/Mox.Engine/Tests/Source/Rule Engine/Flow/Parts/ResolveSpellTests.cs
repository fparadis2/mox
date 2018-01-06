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

using Mox.Abilities;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class ResolveSpellTests : PartTestBase
    {
        #region Variables

        private ResolveSpell m_part;

        private Spell m_spell;

        #endregion

        #region Setup / Teardown

        private void Run()
        {
            m_sequencerTester.Run(m_part);
        }

        public override void Setup()
        {
            base.Setup();

            m_spell = new Spell(m_mockAbility, m_playerA);
            m_part = new ResolveSpell(m_spell);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new ResolveSpell(null); });
        }

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.AreEqual(m_spell, m_part.Spell);
        }

        [Test]
        public void Test_Execute_runs_simple_effects()
        {
            var mockEffect = m_mockery.StrictMock<ISpellEffect>();
            m_spell.Effect = s => mockEffect.Do();
            mockEffect.Do();

            Run();
        }

        [Test]
        public void Test_Execute_runs_complex_effects()
        {
            var part = new MockSpellEffectPart();

            m_spell.EffectPart = part;

            Run();

            Assert.AreEqual(m_spell, part.Spell);
        }

        [Test]
        public void Test_Execute_does_nothing_if_the_spell_has_no_effect()
        {
            m_spell.Effect = null;
            Run();
        }

        #endregion

        #region Mock Types

        private class MockSpellEffectPart : Part, ISpellEffectPart
        {
            public Spell Spell
            {
                get;
                set;
            }

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                Spell = this.PopSpell(context);
                return null;
            }

            #endregion
        }

        #endregion
    }
}
