using Mox.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class SpellAbility : Ability
    {
        #region Properties

        public virtual bool UseStack
        {
            get { return !IsManaAbility; }
        }

        public override bool IsManaAbility => SpellDefinition.IsManaAbility;

        #endregion

        #region Methods

        public override void FillManaOutcome(IManaAbilityOutcome outcome)
        {
            SpellDefinition.FillManaOutcome(outcome);
        }

        public override bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            if (!base.CanPlay(evaluationContext))
                return false;

            var spellContext = new SpellContext(this, evaluationContext.Player);
            return CanExecuteCosts(evaluationContext);
        }

        private bool CanExecuteCosts(AbilityEvaluationContext evaluationContext)
        {
            foreach (Cost cost in SpellDefinition.Costs)
            {
                if (!cost.CanExecute(this, evaluationContext))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void Push(Part.Context context, Spell2 spell)
        {
            if (UseStack)
            {
                context.Game.SpellStack2.Push(spell);

#warning todo spell_v2
                //context.Game.Events.Trigger(new Events.SpellPlayed(spell));
            }
            else
            {
                // Resolve immediately
                spell.Resolve(context);
            }
        }

        public virtual void Resolve(Part.Context context, Spell2 spell)
        {
            foreach (var action in SpellDefinition.Actions)
            {
                context.Schedule(action.ResolvePart(spell));
            }
        }

        #endregion
    }
}
