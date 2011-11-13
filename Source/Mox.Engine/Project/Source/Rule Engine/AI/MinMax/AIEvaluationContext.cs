using System;

using Mox.Transactions;

namespace Mox.AI
{
    public class AIEvaluationContext
    {
        #region Variables

        public readonly IMinimaxTree Tree;
        public readonly IMinMaxAlgorithm Algorithm;
        public readonly IChoiceEnumeratorProvider ChoiceEnumeratorProvider;

        #endregion

        #region Constructor

        public AIEvaluationContext(IMinimaxTree minmaxTree, IMinMaxAlgorithm algorithm, IChoiceEnumeratorProvider choiceEnumeratorProvider)
        {
            Throw.IfNull(minmaxTree, "minmaxTree");
            Throw.IfNull(algorithm, "algorithm");
            Throw.IfNull(choiceEnumeratorProvider, "choiceEnumeratorProvider");

            Tree = minmaxTree;
            Algorithm = algorithm;
            ChoiceEnumeratorProvider = choiceEnumeratorProvider;
        }

        #endregion
    }
}
