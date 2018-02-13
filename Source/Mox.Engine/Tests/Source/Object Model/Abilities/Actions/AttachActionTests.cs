using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class AttachActionTests : BaseActionTests
    {
        #region Utilities

        private void Run(Card target)
        {
            var action = new AttachAction(new SingleObjectResolver(target));
            Run(action);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Attach()
        {
            m_card.Type = Type.Enchantment;
            m_card.SubTypes = new SubTypes(SubType.Aura);

            var creature = CreateCard(m_playerA);
            creature.Type = Type.Creature;

            m_card.Zone = m_game.Zones.Battlefield;
            creature.Zone = m_game.Zones.Battlefield;

            Run(creature);

            Assert.That(m_card.AttachedTo == creature);
        }

        #endregion
    }
}
