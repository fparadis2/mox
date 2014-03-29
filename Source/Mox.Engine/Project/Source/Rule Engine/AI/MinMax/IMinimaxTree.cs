namespace Mox.AI
{
    /// <summary>
    /// Interface of a minmax tree.
    /// </summary>
    public interface IMinimaxTree
    {
        #region Properties

        /// <summary>
        /// Current depth of the tree.
        /// </summary>
        int Depth
        {
            get;
        }

        string DebugInfo { get; }
        bool EnableDebugInfo { get; set; }

        Game Game { get; set; }

#if DEBUG
        int NumEvaluations { get; }
#endif

        #endregion

        #region Methods

        /// <summary>
        /// Begins a new node in the minimax tree.
        /// </summary> 
        /// <param name="result">The result to associated with the node.</param>
        /// <param name="debugInfo">Debug info, if wanted.</param>
        void BeginNode(object result, string debugInfo = null);

        /// <summary>
        /// Initializes the current node (must be called before starting child nodes)
        /// </summary>
        /// <param name="isMaximizing">Whether the current node is a maximizing node.</param>
        void InitializeNode(bool isMaximizing);

        /// <summary>
        /// Ends the current node.
        /// </summary>
        /// <returns>False if the search can be beta-cutoff.</returns>
        bool EndNode();

        /// <summary>
        /// Evaluates the current node (must be a leaf node).
        /// </summary>
        /// <param name="value"></param>
        void Evaluate(float value);

        /// <summary>
        /// Discards the current node so it's not taken into account.
        /// </summary>
        void Discard();

        /// <summary>
        /// Checks if the game hash has been seen before.
        /// If so, returns false and this node evaluation can end early.
        /// </summary>
        bool ConsiderTranspositionTable(int hash);

        bool TryGetBestResult(out object result);

        float GetBestValue();

        #endregion
    }
}