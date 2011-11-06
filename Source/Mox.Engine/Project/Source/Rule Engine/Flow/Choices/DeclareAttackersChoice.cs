namespace Mox.Flow
{
    public class DeclareAttackersChoice : Choice
    {
        #region Variables

        private readonly DeclareAttackersContext m_context;

        #endregion

        #region Constructor

        public DeclareAttackersChoice(Resolvable<Player> player, DeclareAttackersContext context)
            : base(player)
        {
            m_context = context;
        }

        #endregion

        #region Overrides of Choice

        public override object DefaultValue
        {
            get
            {
                return DeclareAttackersResult.Empty;
            }
        }

        #endregion
    }
}