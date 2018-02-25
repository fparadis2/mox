using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class CounterActionTests : BaseActionTests
    {
        #region Utilities

        private void Run(ObjectResolver spells)
        {
            var action = new CounterAction(spells);
            Run(action);
        }

        private PlayCardAbility CreateCard(Player owner, Type type)
        {
            var card = CreateCard(owner);
            card.Type = type;
            card.Zone = m_game.Zones.Hand;

            return m_game.CreateAbility<PlayCardAbility>(card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Counter_counters_the_spells()
        {
            var playAbility1 = CreateCard(m_playerA, Type.Creature);
            var playAbility2 = CreateCard(m_playerA, Type.Instant);
            var playAbility3 = CreateCard(m_playerB, Type.Instant);

            var spell1 = m_sequencerTester.PushSpell(playAbility1);
            var spell2 = m_sequencerTester.PushSpell(playAbility2);
            var spell3 = m_sequencerTester.PushSpell(playAbility3);

            Assert.AreEqual(3, m_game.SpellStack2.Count);

            Run(new MultipleObjectResolver(spell1, spell3));

            Assert.AreEqual(1, m_game.SpellStack2.Count);
            Assert.AreEqual(m_game.Zones.Graveyard, playAbility1.Source.Zone);
            Assert.AreEqual(m_game.Zones.Stack, playAbility2.Source.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, playAbility3.Source.Zone);
        }

        #endregion
    }
}
