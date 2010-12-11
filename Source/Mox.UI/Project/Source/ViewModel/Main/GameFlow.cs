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
using System.Collections.Generic;

namespace Mox.UI
{
    /// <summary>
    /// Provides access to navigation to view models.
    /// </summary>
    public static class GameFlow
    {
        #region Variables

        private static IGameFlow m_gameFlow = new Default();

        #endregion

        #region Properties

        public static IGameFlow Instance
        {
            get { return m_gameFlow; }
        }

        #endregion

        #region Methods

        public static IDisposable Use(IGameFlow gameFlow)
        {
            var oldFlow = m_gameFlow;
            m_gameFlow = gameFlow;

            return new DisposableHelper(() =>
            {
                m_gameFlow = oldFlow;
            });
        }

        #endregion

        #region Inner Types

        public class Default : IGameFlow
        {
            #region Variables

            private readonly Stack<PageHandle> m_contentStack = new Stack<PageHandle>();

            #endregion

            #region Properties

            public bool CanGoBack
            {
                get { return m_contentStack.Count > 1; }
            }

            #endregion

            #region Methods

            public IGameFlowPageHandle GoToPage<TPage>()
                where TPage : new()
            {
                return GoToPage(new TPage());
            }

            public IGameFlowPageHandle GoToPage(object page)
            {
                Clear();
                return PushPage(page);
            }

            public IGameFlowPageHandle PushPage<TPage>()
                where TPage : new()
            {
                return PushPage(new TPage());
            }

            public IGameFlowPageHandle PushPage(object page)
            {
                var pageHandle = new PageHandle(page);
                m_contentStack.Push(pageHandle);
                OnNavigated(new GameFlowNavigationEventArgs(pageHandle.Content));
                return pageHandle;
            }

            public void GoBack()
            {
                Pop();
                OnNavigated(new GameFlowNavigationEventArgs(m_contentStack.Peek().Content));
            }

            private void Clear()
            {
                m_contentStack.ForEach(handle => handle.Dispose());
                m_contentStack.Clear();
            }

            private void Pop()
            {
                var pageHandle = m_contentStack.Pop();
                pageHandle.Dispose();
            }

            #endregion

            #region Events

            public event EventHandler<GameFlowNavigationEventArgs> Navigated;

            protected virtual void OnNavigated(GameFlowNavigationEventArgs e)
            {
                Navigated.Raise(this, e);
            }

            #endregion

            #region Inner Types

            private class PageHandle : IGameFlowPageHandle, IDisposable
            {
                #region Variables

                private readonly object m_content;

                #endregion

                #region Constructor

                public PageHandle(object content)
                {
                    m_content = content;
                }

                public void Dispose()
                {
                    OnClosed(EventArgs.Empty);
                }

                #endregion

                #region Properties

                public object Content
                {
                    get { return m_content; }
                }

                #endregion

                #region Events

                public event EventHandler Closed;

                private void OnClosed(EventArgs e)
                {
                    Closed.Raise(m_content, e);
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}
