﻿using System;
using Caliburn.Micro;

namespace Mox.UI.Lobby
{
    public class DeckChoiceViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly DeckListViewModel m_deckList;
        private DeckViewModel m_selectedDeck;
        private bool m_useRandomDeck;

        #endregion

        #region Constructor

        public DeckChoiceViewModel(DeckListViewModel deckList)
        {
            Throw.IfNull(deckList, "deckList");
            m_deckList = deckList;
        }

        #endregion

        #region Properties

        public DeckViewModel SelectedDeck
        {
            get { return m_selectedDeck; }
            set
            {
                if (m_selectedDeck != value)
                {
                    m_selectedDeck = value;
                    NotifyOfPropertyChange(() => SelectedDeck);
                    NotifyOfPropertyChange(() => Text);
                }
            }
        }

        public bool UseRandomDeck
        {
            get { return m_useRandomDeck; }
            set
            {
                if (m_useRandomDeck != value)
                {
                    m_useRandomDeck = value;
                    NotifyOfPropertyChange(() => UseRandomDeck);
                    NotifyOfPropertyChange(() => Text);
                }
            }
        }

        public string Text
        {
            get
            {
                if (UseRandomDeck)
                {
                    return "Random Deck";
                }

                if (SelectedDeck != null)
                {
                    return SelectedDeck.Name;
                }

                return "[No selected deck]";
            }
        }

        public DeckListViewModel DeckList
        {
            get 
            {
                return m_deckList;
            }
        }

        #endregion
    }
}
