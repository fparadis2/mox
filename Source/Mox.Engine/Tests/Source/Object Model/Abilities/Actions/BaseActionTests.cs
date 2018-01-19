using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class BaseActionTests : BaseGameTests
    {
        protected NewSequencerTester m_sequencerTester;
        protected MockSpellAbility m_ability;

        public override void Setup()
        {
            base.Setup();

            m_sequencerTester = new NewSequencerTester(null, m_game);
            m_sequencerTester.MockAllPlayersChoices();

            m_ability = m_game.CreateAbility<MockSpellAbility>(m_card);
        }

        public override void Teardown()
        {
            m_sequencerTester.VerifyExpectations();

            base.Teardown();
        }

        public void Run(Action action)
        {
            SpellContext context = new SpellContext(m_ability, m_playerA);
            var part = action.ResolvePart(m_game, context);
            m_sequencerTester.Run(part);
        }
    }
}
