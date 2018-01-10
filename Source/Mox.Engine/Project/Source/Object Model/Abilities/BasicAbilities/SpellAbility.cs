using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class SpellAbility2 : Ability
    {
        private SpellDefinition m_spellDefinition;
        public static readonly Property<SpellDefinition> SpellDefinitionProperty = Property<SpellDefinition>.RegisterProperty<SpellAbility2>("SpellDefinition", a => a.m_spellDefinition, PropertyFlags.Private);

        public SpellDefinition SpellDefinition
        {
            get { return m_spellDefinition; }
            set { SetValue(SpellDefinitionProperty, value, ref m_spellDefinition); }
        }

        public override bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            if (!base.CanPlay(evaluationContext))
                return false;

            return CanExecuteCosts(evaluationContext);
        }

        private bool CanExecuteCosts(AbilityEvaluationContext evaluationContext)
        {
            if (m_spellDefinition != null)
            {
                foreach (Cost cost in m_spellDefinition.Costs)
                {
                    if (!cost.CanExecute(Manager, evaluationContext))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Cast or activated abilities
    /// </summary>
    public class PlayAbility2 : SpellAbility2
    {
    }

    /// <summary>
    /// Triggered abilities
    /// </summary>
    public class TriggeredAbility2 : SpellAbility2
    {
    }
}
