using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class MockSpellAbility : SpellAbility
    {
        public bool? MockedIsManaAbility;
        public override bool IsManaAbility => MockedIsManaAbility ?? base.IsManaAbility;

        public bool? MockedUseStack;
        public override bool UseStack => MockedUseStack ?? base.UseStack;

        public bool? CanPlayResult;

        public override bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            return CanPlayResult ?? base.CanPlay(evaluationContext);
        }
    }
}
