using System;

namespace Mox.Flow
{
    [Serializable, AI.ChoiceEnumerator(typeof(AI.ChoiceEnumerators.DeclareAttackersChoiceEnumerator))]
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

        #region Properties

        public DeclareAttackersContext AttackContext
        {
            get { return m_context; }
        }

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