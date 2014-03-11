using System;

namespace Mox.Flow
{
    [Serializable]
    public struct TargetResult : IChoiceResult
    {
        #region Variables

        private static readonly TargetResult ms_invalid = new TargetResult(ObjectManager.InvalidIdentifier);
        private readonly int m_targetIdentifier;

        #endregion

        #region Constructor

        public TargetResult(int targetIdentifier)
        {
            m_targetIdentifier = targetIdentifier;
        }

        #endregion

        #region Properties

        public static TargetResult Invalid
        {
            get { return ms_invalid; }
        }

        public bool IsValid
        {
            get { return m_targetIdentifier != ObjectManager.InvalidIdentifier; }
        }

        internal int Identifier
        {
            get { return m_targetIdentifier; }
        }

        #endregion

        #region Methods

        public TObject Resolve<TObject>(Game game) 
            where TObject : IObject
        {
            Throw.InvalidOperationIf(!IsValid, "Cannot resolve invalid result");
            return game.GetObjectByIdentifier<TObject>(m_targetIdentifier);
        }

        public override string ToString()
        {
            return m_targetIdentifier.ToString();
        }

        public string ToString(Game game)
        {
            if (m_targetIdentifier == ObjectManager.InvalidIdentifier)
                return "[invalid]";

            var result = game.GetObjectByIdentifier<IObject>(m_targetIdentifier);
            if (result == null)
                return "[cannot find target]";

            return result.ToString();
        }

        #endregion
    }
}