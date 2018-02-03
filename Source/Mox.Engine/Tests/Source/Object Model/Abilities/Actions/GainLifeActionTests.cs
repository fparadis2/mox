using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class GainLifeActionTests : BaseActionTests
    {
        #region Utilities

        private void Run(ObjectResolver players, int amount)
        {
            m_playerA.Life = 20;
            m_playerB.Life = 20;

            var action = new GainLifeAction(players, new ConstantAmountResolver(amount));
            Run(action);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Gain_life()
        {
            Run(new SingleObjectResolver(m_playerA), 5);
            Assert.AreEqual(25, m_playerA.Life);
            Assert.AreEqual(20, m_playerB.Life);
        }

        #endregion
    }
}
