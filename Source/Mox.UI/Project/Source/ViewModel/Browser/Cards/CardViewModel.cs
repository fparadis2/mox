﻿// Copyright (c) François Paradis
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
using Mox.Database;

namespace Mox.UI.Browser
{
    public class CardViewModel : ViewModel
    {
        #region Variables

        private readonly CardInfo m_cardInfo;

        private List<CardEditionViewModel> m_editions;
        private CardEditionViewModel m_currentEdition;

        private bool m_isSelected;

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

        public IList<CardEditionViewModel> Editions
        {
            get
            {
                EnsureEditionsCreated();
                return m_editions;
            }
        }

        public bool HasMoreThanOneEdition
        {
            get { return Editions.Count > 1; }
        }

        public CardEditionViewModel CurrentEdition
        {
            get
            {
                EnsureEditionsCreated();
                return m_currentEdition;
            }
            set
            {
                if (m_currentEdition != value)
                {
                    Debug.Assert(Editions.Contains(value));
                    m_currentEdition = value;
                    OnPropertyChanged("CurrentEdition");
                }
            }
        }

        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public string Rules
        {
            get { return m_cardInfo.Abilities.Join(Environment.NewLine); }
        }

        public CardGroupViewModel Group
        {
            get { return new CardGroupViewModel(m_cardInfo.Type); }
        }

        #endregion

        #region Methods

        private void EnsureEditionsCreated()
        {
            if (m_editions == null)
            {
                m_editions = m_cardInfo.Instances.OrderByDescending(i => i.Set.ReleaseDate).Select(i => new CardEditionViewModel(i)).ToList();
                m_currentEdition = m_editions.First();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Inner Types

#warning [HIGH] test
        public class CardGroupViewModel
        {
            private readonly Type m_type;

            public CardGroupViewModel(Type type)
            {
                m_type = type;
            }

            public string DisplayName
            {
                get { return m_type.ToString(); }
            }

            public override bool Equals(object obj)
            {
                CardGroupViewModel other = obj as CardGroupViewModel;

                if (other == null)
                {
                    return false;
                }

                return m_type == other.m_type;
            }

            public override int GetHashCode()
            {
                return m_type.GetHashCode();
            }
        }

        #endregion
    }
}