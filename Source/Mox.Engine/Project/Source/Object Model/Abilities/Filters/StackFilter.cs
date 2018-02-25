using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public abstract class StackFilter : Filter
    {
        public override sealed FilterType FilterType => FilterType.Stack;

        public static readonly AnySpellOnStackFilter AnySpell = new AnySpellOnStackFilter();
    }

    public class AnySpellOnStackFilter : StackFilter
    {
        public override bool Accept(GameObject o, Player controller)
        {
#warning todo spell_v2 - type spells on stack!
            Spell2 spell = (Spell2)o;
            return spell.Ability is PlayCardAbility;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return false;
        }
    }
}
