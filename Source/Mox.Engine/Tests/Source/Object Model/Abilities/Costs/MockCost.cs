using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Flow;

namespace Mox.Abilities
{
    class MockCost : Cost
    {
        public bool IsValid = true;
        public bool Executed;

        public bool ExecuteResult = true;
        public Func<bool> ExecuteCallback;

        public override bool CanExecute(Ability ability, AbilityEvaluationContext evaluationContext)
        {
            return IsValid;
        }

        public override void Execute(Part.Context context, Spell2 spell)
        {
            Assert.That(IsValid);
            Executed = true;

            var result = ExecuteResult;

            var callback = ExecuteCallback;
            if (callback != null)
            {
                result = callback();
            }

            PushResult(context, result);
        }
    }
}
