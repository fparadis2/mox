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
using System.Windows;

namespace Mox.UI
{
    /// <summary>
    /// Provides access to navigation to view models.
    /// </summary>
    public static class GameFlow
    {
        #region Variables

        private static IGameFlow m_gameFlow = new DefaultGameFlow();

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
    }

    internal class DefaultGameFlow : IGameFlow
    {
        #region Variables

        private readonly Stack<object> m_contentStack = new Stack<object>();

        #endregion

        #region Properties

        public bool CanGoBack
        {
            get { return m_contentStack.Count > 1; }
        }

        #endregion

        #region Methods

        public void GoToPage<TPage>()
            where TPage : new()
        {
            m_contentStack.Clear();
            PushPage<TPage>();
        }

        public void PushPage<TPage>()
            where TPage : new()
        {
            TPage page = new TPage();
            m_contentStack.Push(page);
            OnNavigated(new GameFlowNavigationEventArgs(page));
        }

        public void GoBack()
        {
            m_contentStack.Pop();
            OnNavigated(new GameFlowNavigationEventArgs(m_contentStack.Peek()));
        }

        #endregion

        #region Events

        public event EventHandler<GameFlowNavigationEventArgs> Navigated;

        private void OnNavigated(GameFlowNavigationEventArgs e)
        {
            Navigated.Raise(this, e);
        }

        #endregion
    }
}
