namespace Mox.Flow
{
    [AI.ChoiceEnumerator(typeof(AI.ChoiceEnumerators.PayManaChoiceEnumerator))]
    public class PayManaChoice : Choice
    {
        #region Variables

        private readonly ManaCost m_manaCost;

        #endregion

        #region Constructor

        public PayManaChoice(Resolvable<Player> player, ManaCost cost)
            : base(player)
        {
            m_manaCost = cost;
        }

        #endregion

        #region Overrides of Choice

        public override object DefaultValue
        {
            get
            {
                return null;
            }
        }

        public ManaCost ManaCost 
        {
            get { return m_manaCost; }
        }

        #endregion
    }
}