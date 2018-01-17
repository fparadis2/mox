using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class ActivatedAbility : SpellAbility
    {
        public override bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            if (Source.Zone != Source.Manager.Zones.Battlefield)
                return false;

            return base.CanPlay(evaluationContext);
        }
    }
}
