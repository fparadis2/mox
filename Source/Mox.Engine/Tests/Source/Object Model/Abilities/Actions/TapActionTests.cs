using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class TapActionTests : BaseActionTests
    {
        #region Utilities

        private void Run(ObjectResolver cards)
        {
            TapAction action = new TapAction(cards);
            Run(action);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Tap_taps_the_selected_cards()
        {
            var cardA = CreateCard(m_playerA);
            var cardB = CreateCard(m_playerB);
            var cardC = CreateCard(m_playerA);

            cardA.Zone = m_game.Zones.Battlefield;
            cardB.Zone = m_game.Zones.Battlefield;
            cardC.Zone = m_game.Zones.Battlefield;
            cardC.Tap();

            Assert.IsFalse(cardA.Tapped);
            Assert.IsFalse(cardB.Tapped);
            Assert.IsTrue(cardC.Tapped);

            Run(new MultipleObjectResolver(cardA, cardB, cardC));

            Assert.IsTrue(cardA.Tapped);
            Assert.IsTrue(cardB.Tapped);
            Assert.IsTrue(cardC.Tapped);
        }

        #endregion
    }
}
