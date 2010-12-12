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

        public string SetIdentifier
        {
            get { return Set.Identifier; }
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