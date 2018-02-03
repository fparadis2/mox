using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class DrawCardsAction : Action
    {
        private readonly ObjectResolver m_targets;
        private readonly AmountResolver m_cardAmount;

        public DrawCardsAction(ObjectResolver targets, AmountResolver cardAmount)
        {
            Throw.IfNull(targets, "targets");

            m_targets = targets;
            m_cardAmount = cardAmount;
        }

        public ObjectResolver Targets => m_targets;
        public AmountResolver Amount => m_cardAmount;

        protected override void Resolve(Spell2 spell)
        {
            base.Resolve(spell);

            int amount = m_cardAmount.Resolve(spell);
            Debug.Assert(amount >= 0, "Life should be positive");

            foreach (var player in m_targets.Resolve<Player>(spell))
            {
                player.DrawCards(amount);
            }
        }
    }
}
