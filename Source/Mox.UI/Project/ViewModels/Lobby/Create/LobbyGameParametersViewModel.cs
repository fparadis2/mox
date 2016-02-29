using System.Collections.Generic;
using Caliburn.Micro;

namespace Mox.UI.Lobby
{
    public class LobbyGameParametersViewModel : PropertyChangedBase
    {


        #region Game Format

        public IEnumerable<GameFormatViewModel> GameFormats
        {
            get { return GameFormatViewModel.AllFormats; }
        }

        private GameFormatViewModel m_gameFormat;

        public GameFormatViewModel SelectedGameFormat
        {
            get { return m_gameFormat; }
            set
            {
                if (m_gameFormat != value)
                {
                    m_gameFormat = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion
    }
}