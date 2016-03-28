using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;
using Caliburn.Micro;
using Mox.Database;
using Mox.Lobby;
using Mox.UI.Library;

namespace Mox.UI.Lobby
{
    public class LobbyPlayerSlotViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly IRandom m_random = Random.New();

        private readonly LobbyViewModel m_lobbyViewModel;
        private readonly int m_index;

        #endregion

        #region Constructor

        public LobbyPlayerSlotViewModel(LobbyViewModel lobbyViewModel, int index)
        {
            Throw.IfNull(lobbyViewModel, "lobbyViewModel");
            m_lobbyViewModel = lobbyViewModel;
            m_index = index;
        }

        #endregion

        #region Properties

        public string SlotName
        {
            get { return string.Format("Slot {0}", m_index + 1); }
        }

        public bool CanChangeSlot
        {
            get { return IsBot || m_lobbyViewModel.LocalUser == Player; }
        }

        public string PlayerName
        {
            get { return Player == null ? "Unassigned" : Player.Name; }
        }

        private LobbyPlayerViewModel m_player;
        public LobbyPlayerViewModel Player
        {
            get { return m_player; }
            set
            {
                if (m_player != value)
                {
                    m_player = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(() => CanChangeSlot);
                    NotifyOfPropertyChange(() => PlayerName);
                }
            }
        }

        public bool IsLocalPlayer
        {
            get { return m_player == m_lobbyViewModel.LocalUser; }
        }

        public bool IsBot
        {
            get { return m_player == null || m_player.IsBot; }
        }

        private DeckChoiceViewModel m_deck = DeckChoiceViewModel.None;
        public DeckChoiceViewModel Deck
        {
            get { return m_deck; }
            set
            {
                Throw.IfNull(value, "Deck");

                if (m_deck != value)
                {
                    DeckChoiceViewModel newValue = value;

                    switch (value.Type)
                    {
                        case DeckChoiceType.None:
                        case DeckChoiceType.Deck:
                            break;

                        case DeckChoiceType.Browse:
                            BrowseDeck();
                            return;

                        case DeckChoiceType.Random:
                            SelectRandomDeck();
                            return;

                        default:
                            throw new NotImplementedException();
                    }

                    m_deck = newValue;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(() => DeckChoices);

                    if (!m_lobbyViewModel.IsSyncingFromModel)
                    {
                        SetDeckInModel();
                    }
                }
            }
        }

        public IEnumerable<DeckChoiceViewModel> DeckChoices
        {
            get
            {
                yield return m_deck;
                yield return DeckChoiceViewModel.Random;
                yield return DeckChoiceViewModel.Browse;
            }
        }

        private bool m_isReady;
        public bool IsReady
        {
            get { return m_isReady; }
            private set
            {
                if (m_isReady != value)
                {
                    m_isReady = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private bool m_isValid;
        public bool IsValid
        {
            get { return m_isValid; }
            private set
            {
                if (m_isValid != value)
                {
                    m_isValid = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        public void SyncFromModel(PlayerSlotData slot)
        {
            LobbyPlayerViewModel player;
            m_lobbyViewModel.TryGetPlayerViewModel(slot.PlayerId, out player);

            if (player == null)
                player = GetBotIdentity(m_index);

            Player = player;

            Deck = new DeckChoiceViewModel(slot.CreateDeck());
            IsValid = slot.IsValid;
            IsReady = slot.IsReady;
        }

        private void BrowseDeck()
        {
            DeckBrowsePageViewModel browsePage = new DeckBrowsePageViewModel(MasterDeckLibrary.Instance)
            {
                AcceptAction = page =>
                {
                    Deck = new DeckChoiceViewModel(page.SelectedDeck.Model);
                    return true;
                }
            };

            browsePage.Show(m_lobbyViewModel);
        }

        private void SelectRandomDeck()
        {
            DeckBrowsePageViewModel browsePage = new DeckBrowsePageViewModel(MasterDeckLibrary.Instance);

            var decks = browsePage.Library.AvailableDecks;
            if (decks.Count == 0)
                return;

            var deck = m_random.Choose(decks);
            var deckChoice = new DeckChoiceViewModel(deck.Model);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Action(() => Deck = deckChoice));
        }

        private async void SetDeckInModel()
        {
            var lobby = m_lobbyViewModel.Lobby;
            if (lobby != null)
            {
                var data = lobby.Slots[m_index];

                data.FromDeck(m_deck.Deck);

                var result = await lobby.SetPlayerSlotData(m_index, data);

                if (result != SetPlayerSlotDataResult.Success)
                {
                    SyncFromModel(lobby.Slots[m_index]);
                }
            }
        }

        #endregion

        #region Nested

        public enum DeckChoiceType
        {
            None,
            Deck,
            Random,
            Browse
        }

        public class DeckChoiceViewModel
        {
            public static readonly DeckChoiceViewModel None = new DeckChoiceViewModel(DeckChoiceType.None);
            public static readonly DeckChoiceViewModel Random = new DeckChoiceViewModel(DeckChoiceType.Random);
            public static readonly DeckChoiceViewModel Browse = new DeckChoiceViewModel(DeckChoiceType.Browse);

            private readonly DeckChoiceType m_type;
            private readonly IDeck m_deck;

            public DeckChoiceViewModel(DeckChoiceType type)
            {
                Throw.InvalidArgumentIf(type == DeckChoiceType.Deck, "Use the other constructor", "type");
                m_type = type;
                m_deck = null;
            }

            public DeckChoiceViewModel(IDeck deck)
            {
                m_type = deck == null ? DeckChoiceType.None : DeckChoiceType.Deck;
                m_deck = deck;
            }

            public DeckChoiceType Type
            {
                get { return m_type; }
            }

            public IDeck Deck
            {
                get { return m_deck; }
            }

            public string Name
            {
                get
                {
                    switch (m_type)
                    {
                        case DeckChoiceType.Deck:
                            return m_deck.Name;
                        case DeckChoiceType.None:
                            return "None";
                        case DeckChoiceType.Random:
                            return "Random";
                        case DeckChoiceType.Browse:
                            return "Browse...";
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        #endregion

        #region Bot Identities

        private static readonly List<LobbyPlayerViewModel> ms_botPlayers = new List<LobbyPlayerViewModel>();

        private static readonly string[] ms_botNames =
        {
            "Marvin",
            "Jarvis",
            "Hal",
        };

        private static LobbyPlayerViewModel GetBotIdentity(int slot)
        {
            while (ms_botPlayers.Count <= slot)
            {
                int i = ms_botPlayers.Count;
                string name = ms_botNames[i % ms_botNames.Length];
                var botPlayer = new LobbyPlayerViewModel(new PlayerData { Name = name }, true);
                FillBotIdentity(botPlayer);

                ms_botPlayers.Add(botPlayer);
            }

            return ms_botPlayers[slot];
        }

        private static async void FillBotIdentity(LobbyPlayerViewModel bot)
        {
            bot.Image = await LobbyPlayerViewModel.GetImageSource(new PlayerIdentity { Name = bot.Name });
        }

        #endregion
    }

    public class LobbyPlayerSlotViewModel_DesignTime : LobbyPlayerSlotViewModel
    {
        public LobbyPlayerSlotViewModel_DesignTime() 
            : base(new LobbyViewModel(), 0)
        {
        }
    }
}
