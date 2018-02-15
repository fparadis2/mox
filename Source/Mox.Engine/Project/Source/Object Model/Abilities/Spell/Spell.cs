using Mox.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public interface ISpellContext
    {
        Spell2 Spell { get; }
        Ability Ability { get; }
        Player Controller { get; }
    }

    /// <summary>
    /// A spell on the stack
    /// </summary>
    public class Spell2 : GameObject, ISpellContext
    {
        #region Variables

        private readonly SpellAbility m_ability = null;
        public static readonly Property<SpellAbility> AbilityProperty = Property<SpellAbility>.RegisterProperty<Spell2>("Ability", a => a.m_ability);

        private readonly Player m_controller = null;
        public static readonly Property<Player> ControllerProperty = Property<Player>.RegisterProperty<Spell2>("Controller", a => a.m_controller);

        private SpellData m_spellData = null;
        internal static readonly Property<SpellData> SpellDataProperty = Property<SpellData>.RegisterProperty<Spell2>("SpellData", c => c.m_spellData);

        #endregion

        #region Properties

        public SpellAbility Ability
        {
            get { return m_ability; }
        }

        public Player Controller
        {
            get { return m_controller; }
        }

        public Card Source
        {
            get { return m_ability.Source; }
        }

        private SpellData SpellData
        {
            get { return m_spellData ?? new SpellData(); }
            set { SetValue(SpellDataProperty, value, ref m_spellData); }
        }

        Spell2 ISpellContext.Spell => this;
        Ability ISpellContext.Ability => m_ability;

        #endregion

        #region Methods

        #region Lifetime

        public void Push(Part.Context context)
        {
            m_ability.Push(context, this);
        }

        public void Resolve(Part.Context context)
        {
            m_ability.Resolve(context, this);
            context.Schedule(new Flow.Parts.RemoveSpell(this));
        }

        #endregion

        #region Accessors

        public object GetCostResult(Cost cost)
        {
            return SpellData.GetCostResult(cost);
        }

        public void SetCostResult(Cost cost, object result)
        {
            var spellData = SpellData;
            spellData.SetCostResult(cost, result);
            SpellData = spellData;
        }

        #endregion

        #endregion
    }
}
