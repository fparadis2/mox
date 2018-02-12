using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mox.Effects;

namespace Mox.Abilities
{
    public class ModifyPowerAndToughnessAction : Action
    {
        public ModifyPowerAndToughnessAction(ObjectResolver targets, System.Type scopeType, AmountResolver power, AmountResolver toughness)
        {
            Throw.IfNull(targets, "targets");
            Throw.IfNull(power, "power");
            Throw.IfNull(toughness, "toughness");

            Targets = targets;
            ScopeType = scopeType;
            Power = power;
            Toughness = toughness;
        }

        public ObjectResolver Targets { get; }
        public AmountResolver Power { get; }
        public AmountResolver Toughness { get; }
        public System.Type ScopeType { get; }

        protected override void Resolve(Spell2 spell)
        {
            base.Resolve(spell);

            int power = Power.Resolve(spell);
            int toughness = Toughness.Resolve(spell);

            var effect = new ModifyPowerAndToughnessEffect(power, toughness);

            foreach (var target in Targets.Resolve(spell))
            {
                spell.Manager.CreateLocalEffect(target, effect, ScopeType);
            }
        }
    }
}
