using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var effect = CreateEffect(spell);

            foreach (var target in Targets.Resolve(spell))
            {
                spell.Ability.Game.CreateLocalEffect(target, effect, ScopeType);
            }
        }

        public abstract EffectBase CreateEffect(ISpellContext spellContext);
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

        public override EffectBase CreateEffect(ISpellContext spellContext)
        {
            return new ModifyPowerAndToughnessEffect(spellContext, Power, Toughness);
        }
    }
}
