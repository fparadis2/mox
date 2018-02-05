using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class DiscardActionTests : BaseActionTests
    {
        #region Utilities

        private void Discard(ObjectResolver players, int amount)
        {
            var action = new DiscardAction(players, new ConstantAmountResolver(amount));
            Run(action);
        }

        private void DiscardAtRandom(ObjectResolver players, int amount)
        {
            var action = new DiscardAtRandomAction(players, new ConstantAmountResolver(amount));
            Run(action);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Discard()
        {
            var card1 = CreateCard(m_playerA);
            var card2 = CreateCard(m_playerA);
            var card3 = CreateCard(m_playerA);

            card1.Zone = m_game.Zones.Hand;
            card2.Zone = m_game.Zones.Hand;
            card3.Zone = m_game.Zones.Hand;

            Assert.AreEqual(3, m_playerA.Hand.Count);

            m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card2, card3 }, card2, TargetContextType.Discard);
            m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card3 }, card1, TargetContextType.Discard);

            Discard(new SingleObjectResolver(m_playerA), 2);
            Assert.AreEqual(card3, m_playerA.Hand.Single());
        }

        [Test]
        public void Test_Discard_nothing()
        {
            var card1 = CreateCard(m_playerA);
            var card2 = CreateCard(m_playerA);
            var card3 = CreateCard(m_playerA);

            card1.Zone = m_game.Zones.Hand;
            card2.Zone = m_game.Zones.Hand;
            card3.Zone = m_game.Zones.Hand;

            Assert.AreEqual(3, m_playerA.Hand.Count);

            Discard(new SingleObjectResolver(m_playerA), 0);
            Assert.AreEqual(3, m_playerA.Hand.Count);
        }

        [Test]
        public void Test_Discarding_doesnt_need_targeting_when_discarding_the_whole_hand()
        {
            var card1 = CreateCard(m_playerA);
            var card2 = CreateCard(m_playerA);
            var card3 = CreateCard(m_playerA);

            card1.Zone = m_game.Zones.Hand;
            card2.Zone = m_game.Zones.Hand;
            card3.Zone = m_game.Zones.Hand;

            Assert.AreEqual(3, m_playerA.Hand.Count);

            Discard(new SingleObjectResolver(m_playerA), 10);
            Assert.AreEqual(0, m_playerA.Hand.Count);
        }

        [Test]
        public void Test_Discard_with_invalid_target_result()
        {
            var card1 = CreateCard(m_playerA);
            var card2 = CreateCard(m_playerA);
            var card3 = CreateCard(m_playerA);

            card1.Zone = m_game.Zones.Hand;
            card2.Zone = m_game.Zones.Hand;
            card3.Zone = m_game.Zones.Hand;

            Assert.AreEqual(3, m_playerA.Hand.Count);

            m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card2, card3 }, m_card, TargetContextType.Discard);
            m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card2, card3 }, card2, TargetContextType.Discard);
            m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card3 }, card1, TargetContextType.Discard);

            Discard(new SingleObjectResolver(m_playerA), 2);
            Assert.AreEqual(card3, m_playerA.Hand.Single());
        }

        [Test]
        public void Test_Discard_at_random()
        {
            for (int i = 0; i < 3; i++)
            {
                CreateCard(m_playerA).Zone = m_game.Zones.Hand;
                CreateCard(m_playerB).Zone = m_game.Zones.Hand;
            }

            Assert.AreEqual(3, m_playerA.Hand.Count);
            Assert.AreEqual(3, m_playerB.Hand.Count);

            DiscardAtRandom(new SingleObjectResolver(m_playerA), 2);
            Assert.AreEqual(1, m_playerA.Hand.Count);
            Assert.AreEqual(3, m_playerB.Hand.Count);
        }

        #endregion
    }
}
