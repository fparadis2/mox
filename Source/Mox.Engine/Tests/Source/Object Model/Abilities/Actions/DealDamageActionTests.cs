using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class DealDamageActionTests : BaseActionTests
    {
        #region Utilities

        private void Run(ObjectResolver objects, int amount)
        {
            m_playerA.Life = 20;
            m_playerB.Life = 20;

            DealDamageAction action = new DealDamageAction(objects, new ConstantAmountResolver(amount));
            Run(action);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Deal_damage_to_player()
        {
            Run(new SingleObjectResolver(m_playerA), 5);
            Assert.AreEqual(15, m_playerA.Life);
            Assert.AreEqual(20, m_playerB.Life);
        }

        [Test]
        public void Test_Deal_damage_to_multiple_players()
        {
            Run(new MultipleObjectResolver(m_playerA, m_playerB), 5);
            Assert.AreEqual(15, m_playerA.Life);
            Assert.AreEqual(15, m_playerB.Life);
        }

        #endregion
    }
}
