using System;

namespace Mox.UI
{
    public class MockGameFlow : GameFlow.Default, IDisposable
    {
        #region Variables

        private IDisposable m_singletonHandle;

        #endregion

        #region Constructor

        public void Dispose()
        {
            DisposableHelper.SafeDispose(m_singletonHandle);
        }

        #endregion

        #region Properties

        public object Content
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        public static MockGameFlow Use()
        {
            var flow = new MockGameFlow();
            flow.m_singletonHandle = GameFlow.Use(flow);
            return flow;
        }

        public void Assert_Content_Is<T>()
        {
            Assert.IsInstanceOf<T>(Content);
        }

        protected override void OnNavigated(GameFlowNavigationEventArgs e)
        {
            Content = e.Content;
            base.OnNavigated(e);
        }

        #endregion
    }
}
