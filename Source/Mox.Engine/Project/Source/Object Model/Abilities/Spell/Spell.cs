using Mox.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    /// <summary>
    /// A spell on the stack
    /// </summary>
    public class Spell2 : GameObject
    {
        #region Variables

        private readonly SpellAbility2 m_ability = null;
        public static readonly Property<SpellAbility2> AbilityProperty = Property<SpellAbility2>.RegisterProperty<Spell2>("Ability", a => a.m_ability);

        private readonly Player m_controller = null;
        public static readonly Property<Player> ControllerProperty = Property<Player>.RegisterProperty<Spell2>("Controller", a => a.m_controller);

        #endregion

        #region Properties

        public SpellAbility2 Ability
        {
            get { return m_ability; }
        }

        public Player Controller
        {
            get { return m_controller; }
        }

        #endregion

        #region Methods

        public void Resolve(Part.Context context)
        {
            m_ability.Resolve(context, m_controller);
            Remove();
        }

        #endregion
    }
}
