using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Mox.UI.Game
{
    public class GameViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly InteractionModel m_interactionModel;
        private readonly ObservableCollection<PlayerViewModel> m_players = new ObservableCollection<PlayerViewModel>();

        private readonly ObservableCollection<CardViewModel> m_allCards = new ObservableCollection<CardViewModel>();
        private readonly SpellStackViewModel m_spellStack = new SpellStackViewModel();
        private readonly GameStateViewModel m_state = new GameStateViewModel();

        private PlayerViewModel m_mainPlayer;

        #endregion

        #region Constructor

        public GameViewModel()
        {
            m_interactionModel = new InteractionModel(this);

            m_state.PropertyChanged += m_state_PropertyChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Main player of the game (player the user is playing)
        /// </summary>
        public PlayerViewModel MainPlayer
        {
            get { return m_mainPlayer; }
            set
            {
                if (m_mainPlayer != value)
                {
                    m_mainPlayer = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public PlayerViewModel OpponentPlayer
        {
            get { return Players[1]; }
        }

        /// <summary>
        /// Whether the <see cref="MainPlayer"/> is active.
        /// </summary>
        public bool IsActivePlayer
        {
            get { return State.ActivePlayer == null || MainPlayer == State.ActivePlayer; }
        }

        /// <summary>
        /// Whether the <see cref="MainPlayer"/> is active.
        /// </summary>
        public bool IsNotActivePlayer
        {
            get { return !IsActivePlayer; }
        }

        /// <summary>
        /// Players in the game.
        /// </summary>
        public ObservableCollection<PlayerViewModel> Players
        {
            get { return m_players; }
        }

        /// <summary>
        /// Interaction model.
        /// </summary>
        public InteractionModel Interaction
        {
            get { return m_interactionModel; }
        }

        /// <summary>
        /// All cards currently in the model.
        /// </summary>
        public ICollection<CardViewModel> AllCards
        {
            get { return m_allCards; }
        }

        /// <summary>
        /// Source.
        /// </summary>
        public Mox.Game Source
        {
            get;
            internal set;
        }

        public SpellStackViewModel SpellStack
        {
            get { return m_spellStack; }
        }

        /// <summary>
        /// The current game state.
        /// </summary>
        public GameStateViewModel State
        {
            get { return m_state; }
        }

        #endregion

        #region Methods

        public void ResetInteraction()
        {
            Interaction.UserChoiceInteraction = null;

            AllCards.ForEach(card => card.ResetInteraction());
            Players.ForEach(player => player.ResetInteraction());
        }

        #endregion

        #region Event Handlers

        void m_state_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ActivePlayer":
                    NotifyOfPropertyChange(() => IsActivePlayer);
                    NotifyOfPropertyChange(() => IsNotActivePlayer);
                    break;
            }
        }

        #endregion
    }
}
