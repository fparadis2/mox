using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    internal class LobbyParametersUserSettings
    {
        public string LastSelectedGameFormat = null;
        public string LastSelectedDeckFormat = null;
    }

    public class LobbyParametersViewModel : PropertyChangedBase
    {
        #region Constructor

        public LobbyParametersViewModel()
        {
            var settings = Settings.Get<LobbyParametersUserSettings>();
            SelectedGameFormat = GameFormatViewModel.GetFormat(settings.LastSelectedGameFormat) ?? GameFormats.FirstOrDefault();
            SelectedDeckFormat = DeckFormatViewModel.GetFormat(settings.LastSelectedDeckFormat) ?? DeckFormats.FirstOrDefault();
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

        #region Deck Format

        public IEnumerable<DeckFormatViewModel> DeckFormats
        {
            get { return DeckFormatViewModel.AllFormats; }
        }

        private DeckFormatViewModel m_deckFormat;

        public DeckFormatViewModel SelectedDeckFormat
        {
            get { return m_deckFormat; }
            set
            {
                if (m_deckFormat != value)
                {
                    m_deckFormat = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        public LobbyParameters ToLobbyParameters()
        {
            return new LobbyParameters
            {
                GameFormat = m_gameFormat.Format,
                DeckFormat = m_deckFormat.Format,
                AssignNewPlayersToFreeSlots = true,
                AutoFillWithBots = true                
            };
        }

        public void SaveUserSettings()
        {
            var settings = Settings.Get<LobbyParametersUserSettings>();
            settings.LastSelectedGameFormat = m_gameFormat != null ? m_gameFormat.Name : null;
            settings.LastSelectedDeckFormat = m_deckFormat != null ? m_deckFormat.Name : null;
            Settings.Save<LobbyParametersUserSettings>();
        }

        #endregion
    }
}