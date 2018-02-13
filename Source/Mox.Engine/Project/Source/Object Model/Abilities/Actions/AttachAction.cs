using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class AttachAction : Action
    {
        public AttachAction(ObjectResolver target)
        {
            Throw.IfNull(target, "target");
            Target = target;
        }

        public ObjectResolver Target { get; }

        protected override void Resolve(Spell2 spell)
        {
            base.Resolve(spell);

            var targets = Target.Resolve(spell).ToArray();
            Debug.Assert(targets.Length == 1);
            Debug.Assert(targets[0] is Card, "Todo enchant player");
            spell.Source.Attach((Card)targets[0]);
        }
    }
}
