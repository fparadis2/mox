using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Library
{
    public class DeckLibraryViewModel : Screen
    {
        #region Variables

        private readonly DeckLibrary m_library;

        private readonly ObservableCollection<DeckViewModel> m_decks;
        private readonly ICollectionView m_decksView;

        private DeckViewModel m_selectedDeck;

        private string m_filterText;

        private DispatcherTimer m_timer;

        #endregion

        #region Constructor

        public DeckLibraryViewModel(DeckLibrary library)
        {
            Throw.IfNull(library, "library");

            m_library = library;
            m_decks = new ObservableCollection<DeckViewModel>(library.Decks.Select(CreateViewModel));
            m_decksView = CollectionViewSource.GetDefaultView(m_decks);
            m_decksView.Filter = FilterDeck;

            m_selectedDeck = m_decks.FirstOrDefault();

            DisplayName = "Decks";
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            m_timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(10) };
            m_timer.Tick += WhenTimerTick;
            m_timer.Start();
        }

        protected override void OnDeactivate(bool close)
        {
            m_timer.Stop();
            m_timer.Tick -= WhenTimerTick;

            base.OnDeactivate(close);
        }

        #endregion

        #region Properties

        public ICollectionView Decks
        {
            get { return m_decksView; }
        }

        public DeckViewModel SelectedDeck
        {
            get { return m_selectedDeck; }
            set
            {
                if (m_selectedDeck != value)
                {
                    m_selectedDeck = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string FilterText
        {
            get { return m_filterText; }
            set
            {
                if (m_filterText != value)
                {
                    m_filterText = value;

                    RefreshFilter();
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        private void RefreshFilter()
        {
            m_decksView.Refresh();
        }

        private bool FilterDeck(object o)
        {
            if (string.IsNullOrEmpty(FilterText))
                return true;

            DeckViewModel deckModel = (DeckViewModel)o;
            return deckModel.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private DeckViewModel CreateViewModel(IDeck deck)
        {
            return new DeckViewModel(deck) { LastModificationTime = m_library.GetLastModificationTime(deck) };
        }

        #endregion

        #region Commands

        public ICommand NewDeckCommand
        {
            get { return new RelayCommand(NewDeck); }
        }

        private void NewDeck()
        {
            DeckEditPageViewModel editViewModel = new DeckEditPageViewModel
            {
                DisplayName = "Create a new deck",
                CanEditName = true,
                Name = "My New Deck",
                Contents = @"// This is the description of the deck
4 Plains",
                CreateAction = NewDeck
            };

            editViewModel.Show(this);
        }

        private bool NewDeck(DeckEditPageViewModel result)
        {
            if (m_library.GetDeck(result.Name) != null)
            {
                MessageBox.Show(string.Format("There is already a deck named {0}.", result.Name), "Invalid name");
                return false;
            }

            if (!m_library.IsValidName(result.Name))
            {
                MessageBox.Show(string.Format("{0} is not a valid deck name.", result.Name), "Invalid name");
                return false;
            }

            IDeck deck = new Deck(result.Name);
            deck = m_library.Save(deck, result.Contents);
            m_decks.Add(CreateViewModel(deck));
            return true;
        }

        #endregion

        #region Event Handlers

        private void WhenTimerTick(object sender, EventArgs e)
        {
            foreach (var deck in m_decks)
            {
                deck.InvalidateTimingBasedProperties();
            }
        }

        #endregion
    }

    public class DeckLibraryViewModel_DesignTime : DeckLibraryViewModel
    {
        public DeckLibraryViewModel_DesignTime()
            : base(CreateLibrary())
        { }

        private static DeckLibrary CreateLibrary()
        {
            DeckLibrary library = new DeckLibrary();

            library.Save(new Deck("My First Deck"), @"
// This is my first deck. I'm proud of it! I remember when I first created this deck, I was 3 years old. Fond memories... Those decks are the best!
// Also, this rocks!
3 Plains
4 Turned yogurt");

            library.Save(new Deck("My Second Deck"), @"
// This one is not so good.
3 Plains
1 Blood Moon
2 Werewolf");

            return library;
        }
    }
}
