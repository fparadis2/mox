using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class DrawCardsActionTests : BaseActionTests
    {
        #region Utilities

        private void Run(ObjectResolver players, int amount)
        {
            var action = new DrawCardsAction(players, new ConstantAmountResolver(amount));
            Run(action);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Draw_Cards()
        {
            for (int i = 0; i < 3; i++)
            {
                CreateCard(m_playerA).Zone = m_game.Zones.Library;
                CreateCard(m_playerB).Zone = m_game.Zones.Library;
            }

            Run(new SingleObjectResolver(m_playerA), 2);
            Assert.AreEqual(2, m_playerA.Hand.Count);
            Assert.AreEqual(0, m_playerB.Hand.Count);
        }

        #endregion
    }
}
