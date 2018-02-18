using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mox.Events;
using Mox.Flow;

namespace Mox.Abilities
{
    public class Trigger
    {
    }

    public class ZoneChangedTrigger : Trigger
    {
        public void HandleEvent(Game game, ZoneChangeEvent e)
        {
            throw new NotImplementedException();
        }
    }

    public class TriggeredAbility2 : SpellAbility
    {
        #region Properties

        public override sealed AbilityType AbilityType
        {
            get { return AbilityType.Triggered; }
        }

        protected virtual bool CanTriggerWhenSourceIsNotVisible
        {
            get { return false; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Triggers the ability, with a <paramref name="context"/> that can be later used while playing the ability.
        /// </summary>
        /// <param name="context"></param>
        protected void Trigger(object context)
        {
            if (CanTrigger(context))
            {
#warning todo spell_v2
                //Game.GlobalData.TriggerAbility(this, context);
            }
        }

        protected bool CanTrigger(object context)
        {
            return CanPlay(new AbilityEvaluationContext(Controller, AbilityEvaluationContextType.Triggered)
            {
                AbilityContext = context
            });
        }

        public virtual bool CanPushOnStack(Game game, object abilityContext)
        {
            return true;
        }

        public override bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            if (Source.Zone.ZoneId != Zone.Id.Battlefield && !CanTriggerWhenSourceIsNotVisible)
            {
                return false;
            }

            return base.CanPlay(evaluationContext);
        }

        #region Management

        protected override void Init()
        {
            base.Init();

            //Manager.Events.RegisterAllHandlerTypes(this);
        }

        protected override void Uninit()
        {
            //Manager.Events.UnregisterAllHandlerTypes(this);

            base.Uninit();
        }

        #endregion

        #endregion

        #region Inner Types

        protected abstract class SpellEffectModalChoicePart : Part, IChoicePart, ISpellEffectPart
        {
            private readonly ModalChoiceContext m_modalChoiceContext;

            protected SpellEffectModalChoicePart(ModalChoiceContext modalChoiceContext)
            {
                Throw.IfNull(modalChoiceContext, "modalChoiceContext");
                m_modalChoiceContext = modalChoiceContext;
            }

            public Choice GetChoice(Sequencer sequencer)
            {
                var spell = this.PeekSpell(sequencer);
                return new ModalChoice(spell.Controller, m_modalChoiceContext);
            }

            public override sealed Part Execute(Context context)
            {
                var result = this.PopChoiceResult<ModalChoiceResult>(context);
                var spell = this.PopSpell(context);
                return Execute(context, result, spell);
            }

            protected abstract Part Execute(Context context, ModalChoiceResult result, Spell spell);
        }

        #endregion
    }
}
