using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class GainLifeAction : Action
    {
        private readonly ObjectResolver m_targets;
        private readonly AmountResolver m_life;

        public GainLifeAction(ObjectResolver targets, AmountResolver life)
        {
            Throw.IfNull(targets, "targets");

            m_targets = targets;
            m_life = life;
        }

        public ObjectResolver Targets => m_targets;
        public AmountResolver Life => m_life;

        protected override void Resolve(Spell2 spell)
        {
            base.Resolve(spell);

            int life = m_life.Resolve(spell);
            Debug.Assert(life >= 0, "Life should be positive");

            foreach (var player in m_targets.Resolve<Player>(spell))
            {
                player.GainLife(life);
            }
        }
    }
}
