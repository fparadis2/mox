namespace Mox.Flow
{
    [AI.ChoiceEnumerator(typeof(AI.ChoiceEnumerators.TargetChoiceEnumerator))]
    public class TargetChoice : Choice
    {
        #region Variables

        private readonly TargetContext m_context;

        #endregion

        #region Constructor

        public TargetChoice(Resolvable<Player> player, TargetContext context)
            : base(player)
        {
            m_context = context;
        }

        #endregion

        #region Properties

        public TargetContext Context
        {
            get { return m_context; }
        }

        public override object DefaultValue
        {
            get
            {
                if (m_context.AllowCancel)
                {
                    return TargetResult.Invalid;
                }

                return m_context.Targets.Count > 0 ? new TargetResult(m_context.Targets[0]) : TargetResult.Invalid;
            }
        }

        #endregion
    }
}