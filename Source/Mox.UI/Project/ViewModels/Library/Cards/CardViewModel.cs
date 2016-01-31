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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Library
{
    public class CardViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly CardInfo m_cardInfo;

        private List<CardPrintingViewModel> m_printings;
        private CardPrintingViewModel m_currentPrinting;

        #endregion

        #region Constructor

        public CardViewModel(CardInfo cardInfo)
        {
            m_cardInfo = cardInfo;
        }

        #endregion

        #region Properties

        public CardInfo Card
        {
            get { return m_cardInfo; }
        }

        public string Name
        {
            get { return m_cardInfo.Name; }
        }

        public string TypeLine
        {
            get
            {
                string typeline = m_cardInfo.TypeLine;

                if (m_cardInfo.Type.Is(Type.Creature))
                {
                    typeline += string.Format(" ({0}/{1})", m_cardInfo.PowerString, m_cardInfo.ToughnessString);
                }

                return typeline;
            }
        }

        public string ManaCost
        {
            get { return m_cardInfo.ManaCost; }
        }

        public IList<CardPrintingViewModel> Printings
        {
            get
            {
                EnsurePrintingsAreCreated();
                return m_printings;
            }
        }

        public bool HasMoreThanOnePrinting
        {
            get { return Printings.Count > 1; }
        }

        public CardPrintingViewModel CurrentPrinting
        {
            get
            {
                EnsurePrintingsAreCreated();
                return m_currentPrinting;
            }
            set
            {
                if (m_currentPrinting != value)
                {
                    Debug.Assert(Printings.Contains(value));
                    m_currentPrinting = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string Rules
        {
            get { return m_cardInfo.Text; }
        }

        public CardIdentifier CardIdentifier
        {
            get
            {
                CardIdentifier identifier = new CardIdentifier { Card = m_cardInfo.Name };

                if (CurrentPrinting != Printings.FirstOrDefault())
                {
                    identifier.Set = CurrentPrinting.SetIdentifier;
                }

                return identifier;
            }
        }

        public string HeaderText
        {
            get
            {
                ManaCost cost = Mox.ManaCost.Parse(Card.ManaCost);
                string headerText = string.Format("{0} {1}\n{2}", Card.Name, cost.ToString(ManaSymbolNotation.Long), Card.TypeLine);

                if (Card.Type.Is(Type.Creature))
                {
                    headerText += string.Format(" ({0}/{1})", Card.PowerString, Card.ToughnessString);
                }

                return headerText;
            }
        }

        #endregion

        #region Methods

        private void EnsurePrintingsAreCreated()
        {
            if (m_printings == null)
            {
                m_printings = m_cardInfo.Instances
                    .OrderByDescending(i => i.Set.ReleaseDate)
                    .Select(i => new CardPrintingViewModel(i))
                    .ToList();
                m_currentPrinting = m_printings.First();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    internal class CardViewModel_DesignTime : CardViewModel
    {
        public CardViewModel_DesignTime()
            : base(DesignTimeCardDatabase.Instance.Cards[0])
        {
        }
    }
}