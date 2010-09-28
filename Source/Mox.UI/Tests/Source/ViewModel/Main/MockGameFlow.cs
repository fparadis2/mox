// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
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
