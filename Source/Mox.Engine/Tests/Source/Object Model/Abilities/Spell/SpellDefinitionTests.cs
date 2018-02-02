using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    [TestFixture]
    public class SpellDefinitionTests
    {
        #region Utilities

        private SpellDefinition CreateSpell()
        {
            SpellDefinitionIdentifier identifier = new SpellDefinitionIdentifier();
            return new SpellDefinition(identifier);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_IsManaAbility_returns_false_by_default()
        {
            var spell = CreateSpell();
            spell.AddAction(new DealDamageAction(ObjectResolver.SpellController, 2));
            Assert.IsFalse(spell.IsManaAbility);
        }

        [Test]
        public void Test_IsManaAbility_returns_true_when_actions_can_produce_mana()
        {
            var spell = CreateSpell();
            spell.AddAction(new GainManaAction(new ManaAmount { White = 1 }));
            Assert.IsTrue(spell.IsManaAbility);
        }

        [Test]
        public void Test_FillManaOutcome_uses_all_actions()
        {
            var spell = CreateSpell();
            spell.AddAction(new GainManaAction(new ManaAmount { White = 1, Red = 1 }));
            spell.AddAction(new GainManaAction(new ManaAmount { Red = 1 }, new ManaAmount{ Green = 1 }));

            MockManaOutcome outcome = new MockManaOutcome();
            spell.FillManaOutcome(outcome);
            Assert.IsFalse(outcome.AnythingCanHappen);
            Assert.AreEqual(2, outcome.Amounts.Count);
            Assert.Collections.Contains(new ManaAmount { White = 1, Red = 2 }, outcome.Amounts);
            Assert.Collections.Contains(new ManaAmount { White = 1, Red = 1, Green = 1 }, outcome.Amounts);
        }

        [Test]
        public void Test_AddCost_adds_the_costs_in_order()
        {
            var cost1 = new TapSelfCost(true);
            var cost2 = new TapSelfCost(false);
            var cost3 = new PayManaCost(new ManaCost(3));
            var cost4 = new TargetCost(TargetContextType.Normal, PermanentFilter.Any);

            var spell = CreateSpell();
            spell.AddCost(cost1);
            spell.AddCost(cost2);
            spell.AddCost(cost3);
            spell.AddCost(cost4);

            Assert.Collections.AreEqual(new Cost[] { cost4, cost3, cost1, cost2 }, spell.Costs);
        }

        #endregion
    }
}
