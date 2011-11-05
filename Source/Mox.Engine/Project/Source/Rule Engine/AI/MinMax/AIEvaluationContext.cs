using System;

using Mox.Transactions;

namespace Mox.AI
{
    public class AIEvaluationContext
    {
        #region Variables

        private readonly Game m_game;
        private readonly IObjectController m_originalObjectController;

        public readonly IMinimaxTree Tree;
        public readonly IMinMaxAlgorithm Algorithm;
        public readonly IChoiceResolverProvider ChoiceResolverProvider;

        #endregion

        #region Constructor

        public AIEvaluationContext(Game game, IMinimaxTree minmaxTree, IMinMaxAlgorithm algorithm, IChoiceResolverProvider choiceResolverProvider)
        {
            Throw.IfNull(game, "game");
            Throw.InvalidArgumentIf(game.Controller is ObjectController == false, "Invalid object controller", "game");
            Throw.IfNull(minmaxTree, "minmaxTree");
            Throw.IfNull(algorithm, "algorithm");
            Throw.IfNull(choiceResolverProvider, "choiceResolverProvider");

            m_game = game;
            m_originalObjectController = game.Controller;

            Tree = minmaxTree;
            Algorithm = algorithm;
            ChoiceResolverProvider = choiceResolverProvider;
        }

        #endregion

        #region Properties

        public IObjectController OriginalController
        {
            get { return m_originalObjectController; }
        }

        #endregion

        #region Methods

        public AIObjectController AcquireAIObjectController()
        {
            AIObjectController controller = new AIObjectController(this);
            var handle = m_game.UpgradeController(controller);
            controller.SetHandle(handle);
            return controller;
        }

        #endregion
    }
}
