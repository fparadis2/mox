using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mox.Abilities;
using Mox.Flow;

namespace Mox
{
    public class MockPlayerAction : PlayerAction
    {
        public Player ExpectedPlayer { get; set; }
        public bool IsValid { get; set; } = true;

        public override bool CanExecute(AbilityEvaluationContext evaluationContext)
        {
            if (ExpectedPlayer != null)
                Assert.AreEqual(ExpectedPlayer, evaluationContext.Player);

            return IsValid;
        }

        public override void Execute(Part.Context context, Player player)
        {
            Assert.That(IsValid);

            if (ExpectedPlayer != null)
                Assert.AreEqual(ExpectedPlayer, player);
        }
    }
}
