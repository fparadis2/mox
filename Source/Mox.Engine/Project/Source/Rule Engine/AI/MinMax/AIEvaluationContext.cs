using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.AI
{
    public class AIEvaluationContext
    {
        #region Variables

        public readonly IMinimaxTree Tree;
        public readonly IMinMaxAlgorithm Algorithm;
        public readonly IChoiceResolverProvider ChoiceResolverProvider;

        #endregion

        #region Constructor

        public AIEvaluationContext(IMinimaxTree minmaxTree, IMinMaxAlgorithm algorithm, IChoiceResolverProvider choiceResolverProvider)
        {
            Throw.IfNull(minmaxTree, "minmaxTree");
            Throw.IfNull(algorithm, "algorithm");
            Throw.IfNull(choiceResolverProvider, "choiceResolverProvider");

            Tree = minmaxTree;
            Algorithm = algorithm;
            ChoiceResolverProvider = choiceResolverProvider;
        }

        #endregion
    }
}
