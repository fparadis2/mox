using NUnit.Framework;

namespace Mox.AI
{
    [TestFixture]
    public class RecursiveMinMaxDriverTests : MinMaxDriverTestsBase
    {
        #region Overrides of MinMaxDriverTestsBase
        
        protected override MinMaxDriver CreateMinMaxDriver(AIEvaluationContext context, ICancellable cancellable)
        {
            return new RecursiveMinMaxDriver(context, cancellable);
        }

        #endregion
    }
}