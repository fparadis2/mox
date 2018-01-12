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
        public System.Func<bool> ExecuteCallback;

        public override bool CanExecute(Game game, AbilityEvaluationContext evaluationContext)
        {
            return IsValid;
        }

        public override void Execute(Part.Context context, Player activePlayer)
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
