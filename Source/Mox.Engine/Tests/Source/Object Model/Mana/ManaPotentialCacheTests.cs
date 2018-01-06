using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mox.Abilities;

namespace Mox
{
    [TestFixture]
    public class ManaPotentialCacheTests : BaseGameTests
    {
        #region Helpers

        private class MockManaAbility : Ability
        {
            public Color ManaColor;

            public override bool IsManaAbility => true;
            public override void FillManaOutcome(IManaAbilityOutcome outcome)
            {
                Assert.AreNotEqual(Color.None, ManaColor);

                ManaAmount amount = new ManaAmount();
                amount.Add(ManaColor, 1);
                outcome.Add(amount);
            }

            public bool IsPlayable = true;

            public override bool CanPlay(AbilityEvaluationContext evaluationContext)
            {
                if (!IsPlayable)
                    return false;

                return base.CanPlay(evaluationContext);
            }

            public override void Play(Spell spell)
            {
            }
        }

        #endregion

        #region Helpers

        private bool CanPay(ManaCost cost)
        {
            ManaPotentialCache cache = new ManaPotentialCache(m_playerA);
            return cache.CanPay(cost);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Potential_includes_the_current_mana_from_the_pool()
        {
            m_playerA.ManaPool.Red = 2;

            Assert.IsFalse(CanPay(new ManaCost(3)));
            Assert.IsTrue(CanPay(new ManaCost(2)));
        }

        [Test]
        public void Test_Potential_includes_the_mana_abilities()
        {
            var ability1 = m_game.CreateAbility<MockManaAbility>(CreateCard(m_playerA));
            ability1.ManaColor = Color.Red;

            var ability2 = m_game.CreateAbility<MockManaAbility>(CreateCard(m_playerA));
            ability2.ManaColor = Color.Blue;

            Assert.IsFalse(CanPay(new ManaCost(3)));
            Assert.IsTrue(CanPay(new ManaCost(2)));
        }

        [Test]
        public void Test_Potential_doesnt_include_non_playable_mana_abilities()
        {
            var ability1 = m_game.CreateAbility<MockManaAbility>(CreateCard(m_playerB)); // Wrong player
            ability1.ManaColor = Color.Red;

            var ability2 = m_game.CreateAbility<MockManaAbility>(CreateCard(m_playerA)); // Not playable
            ability2.ManaColor = Color.Blue;
            ability2.IsPlayable = false;

            Assert.IsFalse(CanPay(new ManaCost(1)));
        }

        [Test]
        public void Test_Potential_treats_abilities_from_the_same_source_as_exclusive()
        {
            var ability1 = m_game.CreateAbility<MockManaAbility>(m_card);
            ability1.ManaColor = Color.Red;

            var ability2 = m_game.CreateAbility<MockManaAbility>(m_card);
            ability2.ManaColor = Color.Blue;

            Assert.IsFalse(CanPay(new ManaCost(2)));
            Assert.IsTrue(CanPay(new ManaCost(1)));
        }

        #endregion
    }
}
