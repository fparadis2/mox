using Mox.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class SpellAbility2 : Ability
    {
        #region Variables

        private SpellDefinition m_spellDefinition = SpellDefinition.Empty;
        public static readonly Property<SpellDefinition> SpellDefinitionProperty = Property<SpellDefinition>.RegisterProperty<SpellAbility2>("SpellDefinition", a => a.m_spellDefinition, PropertyFlags.Private);

        #endregion

        #region Properties

        public SpellDefinition SpellDefinition
        {
            get { return m_spellDefinition; }
            set
            {
                Throw.IfNull(value, "SpellDefinition");
                SetValue(SpellDefinitionProperty, value, ref m_spellDefinition);
            }
        }

        public virtual bool UseStack
        {
            get { return !IsManaAbility; }
        }

        #endregion

        #region Methods

        public override bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            if (!base.CanPlay(evaluationContext))
                return false;

            return CanExecuteCosts(evaluationContext);
        }

        private bool CanExecuteCosts(AbilityEvaluationContext evaluationContext)
        {
            foreach (Cost cost in m_spellDefinition.Costs)
            {
                if (!cost.CanExecute(Manager, evaluationContext))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void Push(Part.Context context, Player controller)
        {
            if (UseStack)
            {
                var spell = context.Game.CreateSpell(this, controller);
                context.Game.SpellStack2.Push(spell);

#warning todo spell_v2
                //context.Game.Events.Trigger(new Events.SpellPlayed(spell));
            }
            else
            {
                // Resolve immediately
                Resolve(context, controller);
            }
        }

        public virtual void Resolve(Part.Context context, Player controller)
        {
            SpellResolutionContext2 spellContext = new SpellResolutionContext2(this, controller);

            foreach (var action in m_spellDefinition.Actions)
            {
                context.Schedule(action.ResolvePart(spellContext));
            }
        }

        #endregion
    }

    /// <summary>
    /// Triggered abilities
    /// </summary>
    public class TriggeredAbility2 : SpellAbility2
    {
    }
}
