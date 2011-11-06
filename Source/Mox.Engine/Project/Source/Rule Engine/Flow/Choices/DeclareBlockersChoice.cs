namespace Mox.Flow
{
    public class DeclareBlockersChoice : Choice
    {
        #region Variables

        private readonly DeclareBlockersContext m_context;

        #endregion

        #region Constructor

        public DeclareBlockersChoice(Resolvable<Player> player, DeclareBlockersContext context)
            : base(player)
        {
            m_context = context;
        }

        #endregion

        #region Properties

        public DeclareBlockersContext BlockContext
        {
            get { return m_context; }
        }

        public override object DefaultValue
        {
            get
            {
                return DeclareBlockersResult.Empty;
            }
        }

        #endregion
    }
}