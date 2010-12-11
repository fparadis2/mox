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
    public interface IGameFlow
    {
        #region Properties

        bool CanGoBack { get; }

        #endregion

        #region Methods

        IGameFlowPageHandle GoToPage<TPage>() where TPage : new();
        IGameFlowPageHandle GoToPage(object page);

        IGameFlowPageHandle PushPage<TPage>() where TPage : new();
        IGameFlowPageHandle PushPage(object page);

        void GoBack();

        #endregion

        #region Events

        event EventHandler<GameFlowNavigationEventArgs> Navigated;

        #endregion
    }

    public interface IGameFlowPageHandle
    {
        /// <summary>
        /// Triggered when the page is closed/popped.
        /// </summary>
        event EventHandler Closed;
    }

    public class GameFlowNavigationEventArgs : EventArgs
    {
        private readonly object m_content;

        public GameFlowNavigationEventArgs(object content)
        {
            Throw.IfNull(content, "content");
            m_content = content;
        }

        public object Content
        {
            get { return m_content; }
        }
    }
}