using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Library
{
    public class CardPrintingViewModel : PropertyChangedBase
    {
        #region Variables
        
        private readonly CardInstanceInfo m_cardInstanceInfo;

        #endregion

        #region Constructor

        public CardPrintingViewModel(CardInstanceInfo cardInstanceInfo)
        {
            m_cardInstanceInfo = cardInstanceInfo;
        }

        #endregion

        #region Properties

        public ImageKey FullCardImage
        {
            get { return ImageKey.ForCardImage(m_cardInstanceInfo, false); }
        }

        private SetInfo Set
        {
            get { return m_cardInstanceInfo.Set; }
        }

        public int MultiverseId
        {
            get { return m_cardInstanceInfo.MultiverseId; }
        }

        public string SetIdentifier
        {
            get { return Set.Identifier; }
        }

        public string SetName
        {
            get { return Set.Name; }
        }

        public ImageKey SetImage
        {
            get { return ImageKey.ForSetSymbol(Set, Rarity); }
        }

        public string BlockName
        {
            get { return Set.Block; }
        }

        public int ReleaseYear
        {
            get { return Set.ReleaseDate.Year; }
        }

        public Rarity Rarity
        {
            get { return m_cardInstanceInfo.Rarity; }
        }

        public string ToolTip
        {
            get
            {
                return string.Format("{0} ({1})", SetName, Rarity.ToPrettyString());
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return string.Format("{0} ({1})", m_cardInstanceInfo.Card.Name, m_cardInstanceInfo.Set.Name);
        }

        #endregion
    }
}