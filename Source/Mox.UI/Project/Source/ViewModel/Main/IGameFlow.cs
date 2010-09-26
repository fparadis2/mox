using System;
using System.Windows;

namespace Mox.UI
{
    public interface IGameFlow
    {
        #region Properties

        bool CanGoBack { get; }

        #endregion

        #region Methods

        void GoToPage<TPage>() where TPage : new();
        void PushPage<TPage>() where TPage : new();
        void GoBack();

        #endregion

        #region Events

        event EventHandler<GameFlowNavigationEventArgs> Navigated;

        #endregion
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