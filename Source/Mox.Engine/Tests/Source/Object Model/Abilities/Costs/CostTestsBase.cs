using Mox.Flow;

namespace Mox.Abilities
{
    public class CostTestsBase : BaseGameTests
    {
        #region Variables

        protected NewSequencerTester m_sequencer;
        private MockSpellAbility m_ability;
        protected SpellContext m_spellContext;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_sequencer = new NewSequencerTester(m_mockery, m_game);
            m_sequencer.MockPlayerChoices(m_playerA);

            m_ability = m_game.CreateAbility<MockSpellAbility>(m_card);
            m_spellContext = new SpellContext(m_ability, m_playerA);
        }

        public override void Teardown()
        {
            m_sequencer.VerifyExpectations();

            base.Teardown();
        }

        #endregion

        #region Utilities

        private class EvaluateCost : Part
        {
            #region Variables

            private readonly Cost m_cost;
            private readonly SpellContext m_spellContext;

            #endregion

            #region Constructor

            public EvaluateCost(Cost cost, SpellContext spellContext)
            {
                Throw.IfNull(cost, "cost");
                Throw.IfNull(spellContext, "spellContext");

                m_cost = cost;
                m_spellContext = spellContext;
            }

            #endregion

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                m_cost.Execute(context, m_spellContext);
                return null;
            }

            #endregion
        }

        protected void Execute(Cost cost, bool expectedResult)
        {
            m_sequencer.Run(new EvaluateCost(cost, m_spellContext));
            Assert.AreEqual(expectedResult, m_sequencer.Sequencer.PopArgument<bool>(Cost.ArgumentToken));
        }

        #endregion
    }
}