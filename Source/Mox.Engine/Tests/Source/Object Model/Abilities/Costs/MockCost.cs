using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Flow;

namespace Mox
{
    class MockCost : Cost
    {
        public bool IsValid { get; set; }

        public override bool CanExecute(Game game, ExecutionEvaluationContext evaluationContext)
        {
            return IsValid;
        }

        public override void Execute(Part.Context context, Player activePlayer)
        {
            Assert.That(IsValid);
        }
    }
}
