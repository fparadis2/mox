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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class DeckViewModel : EditableViewModel
    {
        #region Constants

        private const DragDropKeyStates SmallIncrementKeys = DragDropKeyStates.AltKey | DragDropKeyStates.ControlKey | DragDropKeyStates.ShiftKey;

        #endregion

        #region Variables

        private Deck m_currentDeck;
        private Deck m_backupDeck;

        private readonly IDeckViewModelEditor m_editor;

        private readonly Dictionary<DeckCardGroup, DeckCardGroupViewModel> m_groups = new Dictionary<DeckCardGroup, DeckCardGroupViewModel>();

        private bool m_isSelected;
        private bool m_isMouseOver;

        private ObservableCollection<DeckCardViewModel> m_cards;
        private CollectionViewSource m_cardsViewSource;

        #endregion

        #region Constructor

        public DeckViewModel(Deck deck, IDeckViewModelEditor editor)
        {
            Throw.IfNull(deck, "deck");

            m_currentDeck = deck;
            m_editor = editor;
        }

        #endregion

        #region Properties

        internal Deck Deck
        {
            get
            {
                Debug.Assert(m_currentDeck != null);
                return m_currentDeck;
            }
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
            get { return Deck.Name; }
            set
            {
                if (Name != value)
                {
                    Throw.IfEmpty(value, "Name");
                    Modify(deck => deck.Name = value);
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        public string Author
        {
            get { return Deck.Author; }
            set
            {
                if (Author != value)
                {
                    Throw.IfEmpty(value, "Author");
                    Modify(deck => deck.Author = value);
                    NotifyOfPropertyChange(() => Author);
                }
            }
        }

        public string Description
        {
            get { return Deck.Description; }
            set
            {
                if (Description != value)
                {
                    Modify(deck => deck.Description = value);
                    NotifyOfPropertyChange(() => Description);
                }
            }
        }

        public DateTime LastModificationTime
        {
            get { return Deck.LastModificationTime; }
        }

        public string LastModificationTimeString
        {
            get { return new DateTimeOffset(DateTime.Now, LastModificationTime).ToString(); }
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
                    NotifyOfPropertyChange(() => IsSelected);
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
                    NotifyOfPropertyChange(() => IsMouseOver);
                }
            }
        }

        //public IDropTarget DropTarget
        //{
        //    get { return new DropTarget<CardIdentifier>((ci, ks) => Drop(ci, ks), null); }
        //}

        #endregion

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #region Editing

        public override void BeginEdit()
        {
            base.BeginEdit();

            m_backupDeck = m_currentDeck;
            m_currentDeck = m_currentDeck.Clone();
        }

        public override void EndEdit()
        {
            base.EndEdit();

            m_backupDeck = null;
        }

        public override void CancelEdit()
        {
            base.CancelEdit();

            m_currentDeck = m_backupDeck;
            m_backupDeck = null;

            m_cards.Clear();
            EnumerateCards().ForEach(m_cards.Add);
        }

        #endregion

#warning Move in EditableViewModel?

        internal void Modify(Action<Deck> action)
        {
            Throw.InvalidOperationIf(!IsEditing, "Must call BeginEdit before editing a view model");
            action(Deck);
            Editor.IsDirty = true;
        }

        private IEnumerable<DeckCardViewModel> EnumerateCards()
        {
            foreach (CardIdentifier cardIdentifier in Deck.Cards.Keys)
            {
                if (Editor.Database.Cards.ContainsKey(cardIdentifier.Card))
                {
                    yield return new DeckCardViewModel(this, cardIdentifier);
                }
            }
        }

        private DeckCardViewModel AddCard(CardIdentifier cardIdentifier)
        {
            if (Editor.Database.Cards.ContainsKey(cardIdentifier.Card))
            {
                var viewModel = new DeckCardViewModel(this, cardIdentifier);
                m_cards.Add(viewModel);
                RefreshGroup(viewModel);
                return viewModel;
            }

            return null;
        }

        private void RemoveCard(CardIdentifier cardIdentifier)
        {
            DeckCardViewModel existing = m_cards.FirstOrDefault(c => c.Name == cardIdentifier.Card);

            if (existing != null)
            {
                Remove(existing);
            }
        }

        internal void Remove(DeckCardViewModel deckCardViewModel)
        {
            m_cards.Remove(deckCardViewModel);
        }

        internal void Refresh(DeckCardViewModel deckCardViewModel)
        {
            RefreshGroup(deckCardViewModel);
        }

        private void RefreshGroup(DeckCardViewModel cardViewModel)
        {
            var groupType = DeckCardGroupViewModel.GetGroup(cardViewModel.Card);
            DeckCardGroupViewModel groupViewModel;
            if (m_groups.TryGetValue(groupType, out groupViewModel))
            {
                RefreshGroup(groupViewModel);
            }
        }

        private void RefreshGroup(DeckCardGroupViewModel groupViewModel)
        {
            var cardsFromGroup = m_cards.Where(c => DeckCardGroupViewModel.GetGroup(c.Card) == groupViewModel.Group);
            groupViewModel.Quantity = cardsFromGroup.Sum(c => c.Quantity);
        }

        private DeckCardViewModel RefreshCard(CardIdentifier cardIdentifier)
        {
            if (m_cards != null)
            {
                RemoveCard(cardIdentifier);
                return AddCard(cardIdentifier);
            }

            return null;
        }

        public DeckCardViewModel Drop(CardIdentifier card, Flags<DragDropKeyStates> modifiers)
        {
            int amountToAdd = 1;

            if (Deck.Cards[card] == 0 && !modifiers.ContainsAny(SmallIncrementKeys))
            {
                amountToAdd = 4;
            }

            Modify(deck => deck.Cards.Add(card, amountToAdd));

            var cardViewModel = RefreshCard(card);
            if (cardViewModel != null)
            {
                cardViewModel.IsSelected = true;
            }
            return cardViewModel;
        }

        internal DeckCardGroupViewModel GetOrCreateGroup(CardInfo cardInfo)
        {
            DeckCardGroup group = DeckCardGroupViewModel.GetGroup(cardInfo);
            DeckCardGroupViewModel groupModel;
            if (!m_groups.TryGetValue(group, out groupModel))
            {
                groupModel = new DeckCardGroupViewModel(group);
                RefreshGroup(groupModel);
                m_groups.Add(group, groupModel);
            }
            return groupModel;
        }

        #endregion
    }
}