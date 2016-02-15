using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private readonly List<DeckViewModel> m_decks;
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
            m_decks = new List<DeckViewModel>(library.Decks.Select(CreateViewModel));
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
            var viewModel = new DeckViewModel(deck) { LastModificationTime = m_library.GetLastModificationTime(deck) };
            return viewModel;
        }

        #endregion

        #region Commands

        public ICommand NewDeckCommand
        {
            get { return new RelayCommand(o => NewDeck()); }
        }

        private void NewDeck()
        {
            DeckEditViewModel editViewModel = new DeckEditViewModel
            {
                CanEditName = true,
                Name = "My New Deck",
                Contents = @"// This is the description of the deck
4 Plains"
            };

            DialogViewModel dialog = new DialogViewModel { Title = "Create new deck", Content = editViewModel };

            dialog.AddCommand("Create", () => NewDeck(editViewModel), DialogCommandOptions.IsDefault);
            dialog.AddCancelCommand();

            dialog.Show();
        }

        private void NewDeck(DeckEditViewModel result)
        {

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
