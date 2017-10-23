using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    [TestFixture]
    public class PayManaCostEvaluatorTests : BaseGameTests
    {
        #region ManaAbility

        private class MockManaOutcome : ManaAbilityOutcome
        {
            public List<ManaAmount> PotentialAmounts { get; } = new List<ManaAmount>();

            public override bool CanProvide(ManaPayment cost)
            {
                // Not needed here
                return false;
            }

            public override IEnumerable<ManaAmount> GetPossibleAmounts()
            {
                return PotentialAmounts;
            }
        }

        private class ManaAbility : Ability
        {
            public readonly MockManaOutcome m_outcome = new MockManaOutcome();
            public override ManaAbilityOutcome ManaOutcome => m_outcome;

            public override void Play(Spell spell)
            {
            }

            public ManaAbility AddOutcome(ManaAmount amount)
            {
                m_outcome.PotentialAmounts.Add(amount);
                return this;
            }
        }

        #endregion

        #region Setup

        private readonly Dictionary<Card, List<Ability>> m_manaAbilities = new Dictionary<Card, List<Ability>>();

        public override void Setup()
        {
            base.Setup();
            m_manaAbilities.Clear();
        }

        private ManaAbility CreateAbility(Card card = null)
        {
            if (card == null)
            {
                card = CreateCard(m_playerA);
            }

            var ability = m_game.CreateAbility<ManaAbility>(card);

            List<Ability> abilities;
            if (!m_manaAbilities.TryGetValue(card, out abilities))
            {
                abilities = new List<Ability>();
                m_manaAbilities.Add(card, abilities);
            }

            abilities.Add(ability);
            return ability;
        }

        private List<ManaAmount> Evaluate()
        {
            return Evaluate(new ManaAmount());
        }

        private List<ManaAmount> Evaluate(ManaAmount baseAmount)
        {
            ManaAbilityEvaluator evaluator = new ManaAbilityEvaluator(baseAmount);

            foreach (var pair in m_manaAbilities)
            {
                Assert.That(evaluator.Consider(pair.Value));
            }

            return evaluator.Amounts.ToList();
        }

        #endregion

        #region Tests
        
        [Test]
        public void Test_Evaluate_returns_false_with_an_Any_outcome()
        {
            var ability = CreateAbility();

            ManaAbilityEvaluator evaluator = new ManaAbilityEvaluator(new ManaAmount());
            Assert.That(!evaluator.Consider(new[] { ability }));
        }

        [Test]
        public void Test_An_R_ability_gives_1_R()
        {
            var ability = CreateAbility();
            ability.AddOutcome(new ManaAmount { Red = 1 });

            var amounts = Evaluate();
            Assert.AreEqual(1, amounts.Count);
            Assert.AreEqual(new ManaAmount { Red = 1 }, amounts[0]);
        }

        [Test]
        public void Test_A_CC_ability_gives_2_C()
        {
            var ability = CreateAbility();
            ability.AddOutcome(new ManaAmount { Colorless = 2 });

            var amounts = Evaluate();
            Assert.AreEqual(1, amounts.Count);
            Assert.AreEqual(new ManaAmount { Colorless = 2 }, amounts[0]);
        }

        [Test]
        public void Test_Three_single_abilities_combine_their_amount()
        {
            CreateAbility().AddOutcome(new ManaAmount { Red = 1 });
            CreateAbility().AddOutcome(new ManaAmount { Black = 1 });
            CreateAbility().AddOutcome(new ManaAmount { Green = 1 });

            var amounts = Evaluate();
            Assert.AreEqual(1, amounts.Count);
            Assert.AreEqual(new ManaAmount { Red = 1, Black = 1, Green = 1 }, amounts[0]);
        }

        [Test]
        public void Test_Two_abilities_from_the_same_source_are_considered_exclusive()
        {
            CreateAbility(m_card).AddOutcome(new ManaAmount { Red = 1 });
            CreateAbility(m_card).AddOutcome(new ManaAmount { Black = 1 });

            var amounts = Evaluate();
            Assert.AreEqual(2, amounts.Count);
            Assert.AreEqual(new ManaAmount { Red = 1 }, amounts[0]);
            Assert.AreEqual(new ManaAmount { Black = 1 }, amounts[1]);
        }

        [Test]
        public void Test_Two_outcomes_from_the_same_ability_are_exclusive()
        {
            CreateAbility()
                .AddOutcome(new ManaAmount { Red = 1 })
                .AddOutcome(new ManaAmount { Black = 1 });

            var amounts = Evaluate();
            Assert.AreEqual(2, amounts.Count);
            Assert.AreEqual(new ManaAmount { Red = 1 }, amounts[0]);
            Assert.AreEqual(new ManaAmount { Black = 1 }, amounts[1]);
        }

        [Test]
        public void Test_The_base_amount_is_added_to_all_results()
        {
            ManaAmount baseAmount = new ManaAmount { Red = 1 };

            CreateAbility()
                .AddOutcome(new ManaAmount { Red = 1 })
                .AddOutcome(new ManaAmount { Black = 1 });

            var amounts = Evaluate(baseAmount);
            Assert.AreEqual(2, amounts.Count);
            Assert.AreEqual(new ManaAmount { Red = 2 }, amounts[0]);
            Assert.AreEqual(new ManaAmount { Red = 1, Black = 1 }, amounts[1]);
        }

        [Test]
        public void Test_Single_and_dual_outcomes_are_combined()
        {
            CreateAbility().AddOutcome(new ManaAmount { Red = 1 });

            CreateAbility()
                .AddOutcome(new ManaAmount { Red = 1 })
                .AddOutcome(new ManaAmount { Black = 1 });

            var amounts = Evaluate();
            Assert.AreEqual(2, amounts.Count);
            Assert.AreEqual(new ManaAmount { Red = 2 }, amounts[0]);
            Assert.AreEqual(new ManaAmount { Red = 1, Black = 1 }, amounts[1]);
        }

        [Test]
        public void Test_Multiple_similar_dual_outcomes_are_combined()
        {
            CreateAbility()
                .AddOutcome(new ManaAmount { Red = 1 })
                .AddOutcome(new ManaAmount { Black = 1 });

            CreateAbility()
                .AddOutcome(new ManaAmount { Red = 1 })
                .AddOutcome(new ManaAmount { Black = 1 });

            var amounts = Evaluate();
            Assert.AreEqual(3, amounts.Count);
            Assert.AreEqual(new ManaAmount { Red = 2 }, amounts[0]);
            Assert.AreEqual(new ManaAmount { Red = 1, Black = 1 }, amounts[1]);
            Assert.AreEqual(new ManaAmount { Black = 2 }, amounts[2]);
        }

        [Test]
        public void Test_Multiple_different_dual_outcomes_are_combined()
        {
            CreateAbility()
                .AddOutcome(new ManaAmount { Red = 1 })
                .AddOutcome(new ManaAmount { Green = 1 });

            CreateAbility()
                .AddOutcome(new ManaAmount { Red = 1 })
                .AddOutcome(new ManaAmount { Black = 1 });

            var amounts = Evaluate();
            Assert.AreEqual(4, amounts.Count);
            Assert.AreEqual(new ManaAmount { Red = 2 }, amounts[0]);
            Assert.AreEqual(new ManaAmount { Red = 1, Black = 1 }, amounts[1]);
            Assert.AreEqual(new ManaAmount { Red = 1, Green = 1 }, amounts[2]);
            Assert.AreEqual(new ManaAmount { Black = 1, Green = 1 }, amounts[3]);
        }

        #endregion
    }
}
