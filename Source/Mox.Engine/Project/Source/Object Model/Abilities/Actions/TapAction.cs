using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class TapAction : Action
    {
        public TapAction(ObjectResolver cards)
        {
            Debug.Assert(cards != null);
            Cards = cards;
        }

        public ObjectResolver Cards { get; private set; }

        protected override void Resolve(Spell2 spell)
        {
            foreach (var card in Cards.Resolve<Card>(spell))
            {
                card.Tap();
            }
        }
    }
}
