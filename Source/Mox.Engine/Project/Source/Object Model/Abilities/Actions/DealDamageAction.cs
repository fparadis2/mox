using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class DealDamageAction : Action
    {
        private readonly ObjectResolver m_targets;
        private readonly int m_damage;

        public DealDamageAction(ObjectResolver targets, int damage)
        {
            Throw.IfNull(targets, "targets");
            Throw.ArgumentOutOfRangeIf(damage <= 0, "Damage must be positive", "damage");

            m_targets = targets;
            m_damage = damage;
        }

        protected override void Resolve(Game game, SpellContext context)
        {
            base.Resolve(game, context);

            foreach (var target in m_targets.Resolve(game, context))
            {
                target.DealDamage(m_damage);
            }
        }
    }
}
