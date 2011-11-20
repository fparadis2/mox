// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;

using Mox.Flow;

namespace Mox
{
    /// <summary>
    /// A triggered ability.
    /// </summary>
    public abstract class TriggeredAbility : Ability
    {
        #region Properties

        public override sealed AbilityType AbilityType
        {
            get
            {
                return AbilityType.Triggered;
            }
        }

        protected virtual Zone.Id TriggerZone
        {
            get { return Zone.Id.Battlefield; }
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
                Game.GlobalData.TriggerAbility(this, context);
            }
        }

        protected bool CanTrigger(object context)
        {
            return CanPlay(Controller, new ExecutionEvaluationContext 
            { 
                Type = EvaluationContextType.Triggered,
                AbilityContext = context
            });
        }

        public virtual bool CanPushOnStack(Game game, object zoneChangeContext)
        {
            return true;
        }

        public override bool CanPlay(Player player, ExecutionEvaluationContext evaluationContext)
        {
            if (Source.Zone.ZoneId != Zone.Id.Battlefield && !CanTriggerWhenSourceIsNotVisible)
            {
                return false;
            }

            return base.CanPlay(player, evaluationContext);
        }

        #region Management

        protected override void Init()
        {
            base.Init();

            Manager.Events.RegisterAllHandlerTypes(this);
        }

        protected override void Uninit()
        {
            Manager.Events.UnregisterAllHandlerTypes(this);

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
