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
using System.Diagnostics;
using System.Windows.Input;
using Mox.Database;

namespace Mox.UI
{
    public class CardViewModel : GameElementViewModel
    {
        #region Variables

        private bool m_canBeChosen;

        private bool m_tapped;
        private PowerAndToughness m_powerAndToughness;

        private IAsyncImage m_image;

        #endregion

        #region Constructor

        public CardViewModel(GameViewModel gameViewModel)
            : base(gameViewModel)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Identifier of the card.
        /// </summary>
        public int Identifier { get; internal set; }

        /// <summary>
        /// Original card.
        /// </summary>
        public Card Source { get; internal set; }

        /// <summary>
        /// Name of the card.
        /// </summary>
        public string Name
        {
            get { return Source.Name; }
            private set { }
        }

        /// <summary>
        /// Image of the card.
        /// </summary>
        public IAsyncImage Image
        {
            get 
            {
                if (m_image == null)
                {
                    var cardInfo = MasterCardDatabase.Instance.Cards[Name];
                    m_image = ImageService.GetCardImage(cardInfo);
                    Debug.Assert(m_image != null);
                }

                return m_image;
            }
        }

        /// <summary>
        /// Whether the card can be choosed for the current interaction.
        /// </summary>
        public bool CanBeChosen
        {
            get { return m_canBeChosen; }
            set 
            {
                if (m_canBeChosen != value)
                {
                    m_canBeChosen = value;
                    OnPropertyChanged("CanBeChosen");
                }
            }
        }

        /// <summary>
        /// Whether the card is tapped.
        /// </summary>
        public bool Tapped
        {
            get { return m_tapped; }
            set 
            {
                if (m_tapped != value)
                {
                    m_tapped = value;
                    OnPropertyChanged("Tapped");
                }
            }
        }

        public PowerAndToughness PowerAndToughness
        {
            get { return m_powerAndToughness; }
            set 
            {
                if (!Equals(m_powerAndToughness, value))
                {
                    m_powerAndToughness = value;
                    OnPropertyChanged("PowerAndToughness");
                    OnPropertyChanged("Power");
                    OnPropertyChanged("Toughness");
                }
            }
        }

        /// <summary>
        /// Power of the card.
        /// </summary>
        public int Power
        {
            get { return PowerAndToughness.Power; }
        }

        /// <summary>
        /// Toughness of the card.
        /// </summary>
        public int Toughness
        {
            get { return PowerAndToughness.Toughness; }
        }

        public bool ShowPowerAndToughness
        {
            get 
            { 
                return Source.Is(Type.Creature) && Source.Zone == Source.Manager.Zones.Battlefield; 
            }
        }

        #endregion

        #region Methods

        public void ResetInteraction()
        {
            CanBeChosen = false;
        }

        public void Choose()
        {
            if (CanBeChosen)
            {
                GameViewModel.Interaction.OnCardChosen(new CardChosenEventArgs(this));
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to choose a card.
        /// </summary>
        public ICommand ChooseCommand
        {
            get { return new RelayCommand(p => CanBeChosen, p => Choose()); }
        }

        #endregion
    }
}
