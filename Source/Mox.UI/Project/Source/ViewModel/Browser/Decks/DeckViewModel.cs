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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using Mox.Database;

namespace Mox.UI.Browser
{
    public interface IDeckViewModelEditor
    {
        CardDatabase Database { get; }
        DeckLibrary Library { get; }

        bool IsDirty { get; set; }
        bool IsEnabled { get; }

        string UserName { get; }
    }

    public class DeckViewModel : ViewModel
    {
        #region Constants

        private const int MaxDisplayDescriptionLength = 140;

        #endregion

        #region Variables

        private readonly IDeckViewModelEditor m_editor;
        private readonly Deck m_deck;

        private bool m_isSelected;
        private bool m_isMouseOver;

        private ObservableCollection<DeckCardViewModel> m_cards;
        private CollectionViewSource m_cardsViewSource;

        #endregion

        #region Constructor

        public DeckViewModel(IDeckViewModelEditor editor, Deck deck)
        {
            m_editor = editor;
            m_deck = deck;
        }

        #endregion

        #region Properties

        internal Deck Deck
        {
            get { return m_deck; }
        }

        public IDeckViewModelEditor Editor
        {
            get { return m_editor; }
        }

        public ICollection<DeckCardViewModel> Cards
        {
            get
            {
                if (m_cards == null)
                {
                    m_cards = new ObservableCollection<DeckCardViewModel>(EnumerateCards());
                    m_cardsViewSource = new CollectionViewSource { Source = m_cards };
                    m_cardsViewSource.GroupDescriptions.Add(new PropertyGroupDescription { PropertyName = "Group" });
                    m_cardsViewSource.SortDescriptions.Add(new SortDescription { PropertyName = "Group" });
                    m_cardsViewSource.SortDescriptions.Add(new SortDescription { PropertyName = "Name" });
                }

                return m_cards;
            }
        }

        public CollectionViewSource CardsViewSource
        {
            get
            {
                var cards = Cards; // Make sure cards are initialized.
                return m_cardsViewSource;
            }
        }

        public string Name
        {
            get { return m_deck.Name; }
            set
            {
                if (Name != value)
                {
                    Modify(deck => deck.Name = value);
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Author
        {
            get { return m_deck.Author; }
            set
            {
                if (Author != value)
                {
                    Modify(deck => deck.Author = value);
                    OnPropertyChanged("Author");
                }
            }
        }

        public string Description
        {
            get { return m_deck.Description; }
            set
            {
                if (Description != value)
                {
                    Modify(deck => deck.Description = value);
                    OnPropertyChanged("Description");
                    OnPropertyChanged("DisplayDescription");
                }
            }
        }

        public string DisplayDescription
        {
            get
            {
                string description = Description;
                if (string.IsNullOrEmpty(description))
                {
                    return "No description";
                }
                
                if (description.Length > MaxDisplayDescriptionLength)
                {
                    description = description.Remove(MaxDisplayDescriptionLength - 3) + "...";
                }

                return description;
            }
        }

        public DateTime LastModificationTime
        {
            get { return m_deck.LastModificationTime; }
        }

        public string LastModificationTimeString
        {
            get { return LastModificationTime.ToString("D"); }
        }

        public string LastModificationTimeToolTipString
        {
            get { return string.Format("Last modified {0}", LastModificationTimeString); }
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

        public bool IsMouseOver
        {
            get { return m_isMouseOver; }
            set
            {
                if (m_isMouseOver != value)
                {
                    m_isMouseOver = value;
                    OnPropertyChanged("IsMouseOver");
                }
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(o => true, o =>
                {
                    EditDeckPageViewModel viewModel = new EditDeckPageViewModel(m_editor, m_deck);
                    EditDeckPage page = new EditDeckPage { DataContext = viewModel };
                    GameFlow.Instance.PushPage(page);
                });
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        internal void Modify(Action<Deck> action)
        {
            Throw.InvalidOperationIf(!Editor.IsEnabled, "Cannot edit model when readonly");
            action(m_deck);
            m_editor.IsDirty = true;
        }

        private IEnumerable<DeckCardViewModel> EnumerateCards()
        {
            foreach (CardIdentifier cardIdentifier in m_deck.Cards.Keys)
            {
                if (Editor.Database.Cards.ContainsKey(cardIdentifier.Card))
                {
                    yield return new DeckCardViewModel(this, cardIdentifier);
                }
            }
        }

        #endregion
    }
}