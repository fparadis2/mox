using System;
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
        private int m_selectedDeckIndex;

        private string m_filterText;

        private DispatcherTimer m_timer;

        #endregion

        #region Constructor

        public DeckLibraryViewModel(DeckLibrary library)
        {
            Throw.IfNull(library, "library");

            m_library = library;

            var allDecks = library.Decks.Select(CreateViewModel).ToList();
            allDecks.Sort();

            m_decks = new ObservableCollection<DeckViewModel>(allDecks);
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

        public int SelectedDeckIndex
        {
            get { return m_selectedDeckIndex; }
            set
            {
                if (m_selectedDeckIndex != value)
                {
                    m_selectedDeckIndex = value;
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
                SaveAction = NewDeck,
                SaveText = "Create"
            };

            editViewModel.Show(this);
        }

        private bool NewDeck(DeckEditPageViewModel result)
        {
            string name = result.Name;
            string error;
            if (!m_library.ValidateDeckName(null, ref name, out error))
            {
                if (name == result.Name)
                {
                    MessageBox.Show(error, "Invalid Deck Name");
                    return false;
                }

                if (MessageBox.Show(string.Format("{0} Do you want to use this name instead: {1}?", error, name),
                    "Invalid Deck Name", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                {
                    return false;
                }
            }

            var deck = m_library.Save(null, name, result.Contents);

            var deckViewModel = CreateViewModel(deck);
            AddDeck(deckViewModel);
            SelectedDeck = deckViewModel;

            return true;
        }

        public void DeleteDeck(DeckViewModel deck)
        {
            if (MessageBox.Show("Do you really want to delete the deck?", "Delete Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) != MessageBoxResult.OK)
            {
                return;
            }

            m_library.Delete(deck.Model);
            RemoveDeck(deck);
        }

        private void AddDeck(DeckViewModel deck)
        {
            int index = m_decks.BinarySearchForInsertion(deck);
            m_decks.Insert(index, deck);
        }

        private void RemoveDeck(DeckViewModel deck)
        {
            int oldSelectedIndex = SelectedDeckIndex;
            m_decks.Remove(deck);

            oldSelectedIndex = Math.Min(oldSelectedIndex, m_decks.Count - 1);
            SelectedDeckIndex = oldSelectedIndex;
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

            library.Create("My First Deck", @"
// This is my first deck. I'm proud of it! I remember when I first created this deck, I was 3 years old. Fond memories... Those decks are the best!
// Also, this rocks!
3 Plains
4 Turned yogurt");

            library.Create("My Second Deck", @"
// This one is not so good.
3 Plains
1 Blood Moon
2 Werewolf");

            return library;
        }
    }
}
