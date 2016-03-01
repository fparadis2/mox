using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Mox.UI.Lobby
{
    internal class LobbyUserSettings
    {
        public string LastSelectedGameFormat;
    }

    public class LobbyGameParametersViewModel : PropertyChangedBase
    {
        #region Constructor

        public LobbyGameParametersViewModel()
        {
            var settings = Settings.Get<LobbyUserSettings>();
            SelectedGameFormat = GameFormatViewModel.GetFormat(settings.LastSelectedGameFormat) ?? GameFormats.FirstOrDefault();
        }

        #endregion

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