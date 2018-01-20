﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class DealDamageAction : Action
    {
        private readonly ObjectResolver m_targets;
        private readonly AmountResolver m_damage;

        public DealDamageAction(ObjectResolver targets, AmountResolver damage)
        {
            Throw.IfNull(targets, "targets");

            m_targets = targets;
            m_damage = damage;
        }

        public ObjectResolver Targets => m_targets;
        public AmountResolver Damage => m_damage;

        protected override void Resolve(Game game, SpellContext context)
        {
            base.Resolve(game, context);

            int damage = m_damage.Resolve(game, context);
            Debug.Assert(damage >= 0, "Damage should be positive");

            foreach (var target in m_targets.Resolve(game, context))
            {
                target.DealDamage(damage);
            }
        }
    }
}
