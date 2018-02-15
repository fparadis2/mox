﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mox.Effects;

namespace Mox.Abilities
{
    public abstract class ApplyEffectAction : Action
    {
        public ApplyEffectAction(ObjectResolver targets, System.Type scopeType)
        {
            Throw.IfNull(targets, "targets");

            Targets = targets;
            ScopeType = scopeType;
        }

        public ObjectResolver Targets { get; }
        public System.Type ScopeType { get; }

        protected override void Resolve(Spell2 spell)
        {
            base.Resolve(spell);

            foreach (var instance in CreateEffectInstances(spell))
            { } // Loop has side effect
        }

        public IEnumerable<EffectInstance> CreateEffectInstances(ISpellContext spellContext)
        {
            var effect = CreateEffect(spellContext);

            foreach (var target in Targets.Resolve(spellContext))
            {
                yield return spellContext.Ability.Game.CreateLocalEffect(target, effect, ScopeType);
            }
        }

        protected abstract EffectBase CreateEffect(ISpellContext spellContext);
    }

    public class ModifyPowerAndToughnessAction : ApplyEffectAction
    {
        public ModifyPowerAndToughnessAction(ObjectResolver targets, System.Type scopeType, AmountResolver power, AmountResolver toughness)
            : base(targets, scopeType)
        {
            Throw.IfNull(power, "power");
            Throw.IfNull(toughness, "toughness");

            Power = power;
            Toughness = toughness;
        }
        
        public AmountResolver Power { get; }
        public AmountResolver Toughness { get; }

        protected override EffectBase CreateEffect(ISpellContext spellContext)
        {
            int power = Power.Resolve(spellContext);
            int toughness = Toughness.Resolve(spellContext);

            return new ModifyPowerAndToughnessEffect(power, toughness);
        }
    }
}
