using System;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class CardEditionViewModel : ViewModel
    {
        #region Variables
        
        private readonly CardInstanceInfo m_cardInstanceInfo;

        #endregion

        #region Constructor

        public CardEditionViewModel(CardInstanceInfo cardInstanceInfo)
        {
            m_cardInstanceInfo = cardInstanceInfo;
        }

        #endregion

        #region Properties

        public IAsyncImage CardImage
        {
            get { return ImageService.GetCardImage(m_cardInstanceInfo); }
        }

        private SetInfo Set
        {
            get { return m_cardInstanceInfo.Set; }
        }

        public string SetName
        {
            get { return Set.Name; }
        }

        public IAsyncImage SetImage
        {
            get { return ImageService.GetSetSymbolImage(Set, Rarity, ImageSize.Small); }
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