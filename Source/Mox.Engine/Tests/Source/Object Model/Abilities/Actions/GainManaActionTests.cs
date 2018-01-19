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

        private void Run(Color color)
        {
            m_playerA.ManaPool.Clear();

            GainManaAction action = new GainManaAction(color);
            Run(action);
        }

        private void Test_SingleColor(Color color)
        {
            Run(color);

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
            m_sequencerTester.Expect_Player_GainManaChoice(m_playerA, new[] { Color.White, Color.Black }, Color.White);
            Run(Color.White | Color.Black);

            ManaAmount amount = m_playerA.ManaPool;
            Assert.AreEqual(new ManaAmount { White = 1 }, amount);
        }

        [Test]
        public void Test_Player_has_to_choose_until_a_valid_choice_is_returned()
        {
            m_sequencerTester.Expect_Player_GainManaChoice(m_playerA, new[] { Color.White, Color.Black }, Color.Blue);
            m_sequencerTester.Expect_Player_GainManaChoice(m_playerA, new[] { Color.White, Color.Black }, Color.Black);
            Run(Color.White | Color.Black);

            ManaAmount amount = m_playerA.ManaPool;
            Assert.AreEqual(new ManaAmount { Black = 1 }, amount);
        }

        #endregion
    }
}
