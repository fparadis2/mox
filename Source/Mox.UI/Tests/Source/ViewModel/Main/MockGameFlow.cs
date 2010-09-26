using System;

namespace Mox.UI
{
    public class MockGameFlow : GameFlow.Default
    {
        #region Properties

        public object Content
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        protected override void OnNavigated(GameFlowNavigationEventArgs e)
        {
            Content = e.Content;
            base.OnNavigated(e);
        }

        #endregion
    }
}
