using Mox.Flow;

namespace Mox
{
    public class CostTestsBase : BaseGameTests
    {
        #region Variables

        protected NewSequencerTester m_sequencer;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_sequencer = new NewSequencerTester(m_mockery, m_game);
            m_sequencer.MockPlayerChoices(m_playerA);
        }

        public override void Teardown()
        {
            m_sequencer.VerifyExpectations();

            base.Teardown();
        }

        #endregion

        #region Utilities

        private class EvaluateCost : PlayerPart
        {
            #region Variables

            private readonly Cost m_cost;

            #endregion

            #region Constructor

            public EvaluateCost(Cost cost, Player player)
                : base(player)
            {
                Throw.IfNull(cost, "cost");

                m_cost = cost;
            }

            #endregion

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                m_cost.Execute(context, GetPlayer(context));
                return null;
            }

            #endregion
        }

        protected void Execute(Cost cost, Player player, bool expectedResult)
        {
            m_sequencer.Run(new EvaluateCost(cost, player));
            Assert.AreEqual(expectedResult, m_sequencer.Sequencer.PopArgument<bool>(Cost.ArgumentToken));
        }

        #endregion
    }
}