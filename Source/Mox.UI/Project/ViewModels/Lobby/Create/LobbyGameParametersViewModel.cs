using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    internal class LobbyUserSettings
    {
        public string LastSelectedGameFormat = null;
        public string LastSelectedDeckFormat = null;
    }

    public class LobbyGameParametersViewModel : PropertyChangedBase
    {
        #region Constructor

        public LobbyGameParametersViewModel()
        {
            var settings = Settings.Get<LobbyUserSettings>();
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
                DeckFormat = m_deckFormat.Format
            };
        }

        #endregion
    }
}