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

        private static readonly Stack<FrameworkElement> m_contentStack = new Stack<FrameworkElement>();

        #endregion

        #region Properties

        public static bool CanGoBack
        {
            get { return m_contentStack.Count > 1; }
        }

        #endregion

        #region Methods

        public static void GoToPage<TPage>()
            where TPage : FrameworkElement, new()
        {
            m_contentStack.Clear();
            PushPage<TPage>();
        }

        public static void PushPage<TPage>()
            where TPage : FrameworkElement, new()
        {
            TPage page = new TPage();
            m_contentStack.Push(page);
            OnNavigated(new GameFlowNavigationEventArgs(page));
        }

        public static void GoBack()
        {
            m_contentStack.Pop();
            OnNavigated(new GameFlowNavigationEventArgs(m_contentStack.Peek()));
        }

        #endregion

        #region Events

        public static event EventHandler<GameFlowNavigationEventArgs> Navigated;

        private static void OnNavigated(GameFlowNavigationEventArgs e)
        {
            Navigated.Raise(null, e);
        }

        #endregion
    }

    public class GameFlowNavigationEventArgs : EventArgs
    {
        private readonly FrameworkElement m_content;

        public GameFlowNavigationEventArgs(FrameworkElement content)
        {
            Throw.IfNull(content, "content");
            m_content = content;
        }

        public FrameworkElement Content
        {
            get { return m_content; }
        }
    }
}
