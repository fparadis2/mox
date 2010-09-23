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
    }
}