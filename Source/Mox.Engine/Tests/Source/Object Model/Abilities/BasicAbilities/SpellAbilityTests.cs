using Mox.Flow;
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

        private MockSpellAbility m_spellAbility;
        private SpellDefinition m_spellDefinition;
        private MockCost m_mockCost;
        private MockAction m_mockAction;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_spellAbility = m_game.CreateAbility<MockSpellAbility>(m_card);

            m_mockCost = new MockCost();
            m_mockAction = new MockAction();

            m_spellDefinition = m_spellAbility.CreateSpellDefinition();
            m_spellDefinition.AddCost(m_mockCost);
            m_spellDefinition.AddAction(m_mockAction);
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

        [Test]
        public void Test_Resolve_schedules_the_part_of_the_action()
        {
            m_mockAction.Part = new MockPart();

            var sequencer = new Sequencer(m_game, new MockPart());
            var context = new Part.Context(sequencer);

            m_spellAbility.Resolve(context, m_playerA);

            Assert.Collections.AreEqual(context.ScheduledParts, new[] { m_mockAction.Part });
        }

        [Test]
        public void Test_SpellAbility_uses_the_stack_by_default()
        {
            m_spellAbility.MockedIsManaAbility = false;
            Assert.IsTrue(m_spellAbility.UseStack);
        }

        [Test]
        public void Test_SpellAbility_doesnt_use_the_stack_for_mana_abilities()
        {
            m_spellAbility.MockedIsManaAbility = true;
            Assert.IsFalse(m_spellAbility.UseStack);
        }

        #endregion
    }
}
