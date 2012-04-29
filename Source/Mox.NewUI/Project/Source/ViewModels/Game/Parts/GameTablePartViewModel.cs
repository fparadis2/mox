using System;

namespace Mox.UI.Game
{
    public class GameTablePartViewModel
    {
        #region Variables

        private readonly GameViewModel m_gameViewModel;

        #endregion

        #region Constructor

        public GameTablePartViewModel(GameViewModel gameViewModel)
        {
            m_gameViewModel = gameViewModel;
        }

        #endregion

        #region Properties

        public GameViewModel Game
        {
            get { return m_gameViewModel; }
        }

        #endregion
    }
}
