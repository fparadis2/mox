using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mox.Events;
using Mox.Flow;

namespace Mox.Abilities
{
    public abstract class Trigger
    {
        public abstract IEnumerable<System.Type> EventTypes { get; }

        public abstract bool ShouldTrigger(TriggeredAbility2 ability, Event e);
    }

    public class ZoneChangedTrigger : Trigger
    {
        public override IEnumerable<System.Type> EventTypes => throw new NotImplementedException();

        public void HandleEvent(Game game, ZoneChangeEvent e)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldTrigger(TriggeredAbility2 ability, Event e)
        {
            throw new NotImplementedException();
        }
    }

    public class TriggeredAbility2 : SpellAbility, IEventHandler
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
        private void Trigger(object context)
        {
            if (CanTrigger(context))
            {
                Game.GlobalData.TriggerAbility(this, context);
            }
        }

        protected bool CanTrigger(object context)
        {
            return CanPlay(new AbilityEvaluationContext(Controller, AbilityEvaluationContextType.Triggered)
            {
                AbilityContext = context
            });
        }

        public override bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            if (Source.Zone.ZoneId != Zone.Id.Battlefield && !CanTriggerWhenSourceIsNotVisible)
            {
                return false;
            }

            return base.CanPlay(evaluationContext);
        }

        protected override void Init()
        {
            base.Init();

            var spellDefinition = SpellDefinition;
            if (spellDefinition != null)
            {
                if (spellDefinition.Trigger != null)
                {
                    foreach (var eventType in spellDefinition.Trigger.EventTypes)
                    {
                        Manager.Events.Register(eventType, this);
                    }
                }
            }
        }

        protected override void Uninit()
        {
            var spellDefinition = SpellDefinition;
            if (spellDefinition != null)
            {
                if (spellDefinition.Trigger != null)
                {
                    foreach (var eventType in spellDefinition.Trigger.EventTypes)
                    {
                        Manager.Events.Unregister(eventType, this);
                    }
                }
            }

            base.Uninit();
        }

        public void HandleEvent(Game game, Event e)
        {
            if (SpellDefinition.Trigger.ShouldTrigger(this, e))
            {
                Trigger(null);
            }
        }

        #endregion

        #region Inner Types

#warning todo spell_v2
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
