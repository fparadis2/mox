using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class GainManaActionTests : BaseActionTests
    {
        #region Utilities

        private void Run(params ManaAmount[] amounts)
        {
            m_playerA.ManaPool.Clear();

            GainManaAction action = new GainManaAction(amounts);
            Run(action);
        }

        private void Test_SingleColor(Color color)
        {
            var amount = new ManaAmount();
            amount.Add(color, 1);

            Run(amount);

            Assert.That(m_playerA.ManaPool[color] == 1);

            foreach (Color otherColor in Enum.GetValues(typeof(Color)))
            {
                if (otherColor != color)
                {
                    Assert.That(m_playerA.ManaPool[otherColor] == 0);
                }
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Gains_one_mana_directly_when_there_is_only_one_possible_color()
        {
            Test_SingleColor(Color.None);
            Test_SingleColor(Color.White);
            Test_SingleColor(Color.Blue);
            Test_SingleColor(Color.Black);
            Test_SingleColor(Color.Red);
            Test_SingleColor(Color.Green);
        }

        [Test]
        public void Test_When_more_than_one_color_is_possible_the_player_chooses()
        {
            var amounts = new[] { new ManaAmount { White = 1 }, new ManaAmount { Black = 1 } };

            m_sequencerTester.Expect_Player_GainManaChoice(m_playerA, 0, amounts);
            Run(amounts);

            ManaAmount amount = m_playerA.ManaPool;
            Assert.AreEqual(new ManaAmount { White = 1 }, amount);
        }

        [Test]
        public void Test_Player_has_to_choose_until_a_valid_choice_is_returned()
        {
            var amounts = new[] { new ManaAmount { White = 1 }, new ManaAmount { Black = 1 } };

            m_sequencerTester.Expect_Player_GainManaChoice(m_playerA, 5, amounts);
            m_sequencerTester.Expect_Player_GainManaChoice(m_playerA, 1, amounts);
            Run(amounts);

            ManaAmount amount = m_playerA.ManaPool;
            Assert.AreEqual(new ManaAmount { Black = 1 }, amount);
        }

        [Test]
        public void Test_FillManaOutcome_for_single_amount()
        {
            ManaAmount amount = new ManaAmount { Red = 1 };
            GainManaAction action = new GainManaAction(amount);

            MockManaOutcome outcome = new MockManaOutcome();
            action.FillManaOutcome(outcome);

            Assert.IsFalse(outcome.AnythingCanHappen);
            Assert.AreEqual(1, outcome.Amounts.Count);
            Assert.AreEqual(amount, outcome.Amounts[0]);
        }

        [Test]
        public void Test_FillManaOutcome_for_many_color()
        {
            var amounts = new[]
            {
                new ManaAmount { White = 1 },
                new ManaAmount { Red = 1 }
            };

            GainManaAction action = new GainManaAction(amounts);

            MockManaOutcome outcome = new MockManaOutcome();
            action.FillManaOutcome(outcome);

            Assert.IsFalse(outcome.AnythingCanHappen);
            Assert.AreEqual(2, outcome.Amounts.Count);
            Assert.Collections.Contains(amounts[0], outcome.Amounts);
            Assert.Collections.Contains(amounts[1], outcome.Amounts);
        }

        #endregion
    }
}
