namespace Mox.Flow
{
    [AI.ChoiceEnumerator(typeof(AI.ChoiceEnumerators.ModalChoiceEnumerator))]
    public class ModalChoice : Choice
    {
        #region Variables

        private readonly ModalChoiceContext m_context;

        #endregion

        #region Constructor

        public ModalChoice(Resolvable<Player> player, ModalChoiceContext context)
            : base(player)
        {
            m_context = context;
        }

        #endregion

        #region Properties

        public ModalChoiceContext Context
        {
            get { return m_context; }
        }

        public override object DefaultValue
        {
            get { return m_context.DefaultChoice; }
        }

        #endregion
    }
}