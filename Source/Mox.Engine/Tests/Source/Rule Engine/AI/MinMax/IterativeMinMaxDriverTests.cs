using NUnit.Framework;

namespace Mox.AI
{
    [TestFixture]
    public class IterativeMinMaxDriverTests : MinMaxDriverTestsBase
    {
        #region Overrides of MinMaxDriverTestsBase
        
        protected override MinMaxDriver CreateMinMaxDriver(AIEvaluationContext context, ICancellable cancellable)
        {
            return new IterativeMinMaxDriver(context, cancellable);
        }

        #endregion
    }
}