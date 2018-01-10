using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    [TestFixture]
    public class SpellAbilityTests : BaseGameTests
    {
        #region Variables

        private SpellAbility2 m_spellAbility;
        private SpellDefinition m_spellDefinition;
        private MockCost m_mockCost;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_spellAbility = m_game.CreateAbility<SpellAbility2>(m_card);

            m_mockCost = new MockCost();

            m_spellDefinition = CreateSpellDefinition();
            m_spellDefinition.AddCost(m_mockCost);
            m_spellAbility.SpellDefinition = m_spellDefinition;
        }

        #endregion

        #region Utilities

        private bool CanPlay()
        {
            var context = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal);
            return m_spellAbility.CanPlay(context);
        }

        private SpellDefinition CreateSpellDefinition()
        {
            var identifier = new SpellDefinitionIdentifier();
            return new SpellDefinition(identifier);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_A_spell_ability_can_only_be_played_if_the_costs_can_be_paid()
        {
            m_mockCost.IsValid = true;
            Assert.IsTrue(CanPlay());
        }

        [Test]
        public void Test_A_spell_ability_cannot_be_played_if_costs_cannot_be_paid()
        {
            m_mockCost.IsValid = false;
            Assert.IsFalse(CanPlay());
        }

        #endregion
    }
}
