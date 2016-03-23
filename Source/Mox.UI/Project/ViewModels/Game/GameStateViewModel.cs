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
using Caliburn.Micro;

namespace Mox.UI.Game
{
    public class GameStateViewModel : PropertyChangedBase
    {
        #region Variables

        private StepViewModel m_currentStep = new StepViewModel(Phases.PrecombatMain);
        private readonly List<StepViewModel> m_allSteps = new List<StepViewModel>();
        private PlayerViewModel m_activePlayer;

        #endregion

        #region Constructor

        public GameStateViewModel()
        {
            m_allSteps.Add(new StepViewModel(Steps.Untap));
            m_allSteps.Add(new StepViewModel(Steps.Upkeep));
            m_allSteps.Add(new StepViewModel(Steps.Draw));

            m_allSteps.Add(new StepViewModel(Phases.PrecombatMain));

            m_allSteps.Add(new StepViewModel(Steps.BeginningOfCombat));
            m_allSteps.Add(new StepViewModel(Steps.DeclareAttackers));
            m_allSteps.Add(new StepViewModel(Steps.DeclareBlockers));
            m_allSteps.Add(new StepViewModel(Steps.CombatDamage));
            m_allSteps.Add(new StepViewModel(Steps.EndOfCombat));

            m_allSteps.Add(new StepViewModel(Phases.PostcombatMain));

            m_allSteps.Add(new StepViewModel(Steps.End));
            m_allSteps.Add(new StepViewModel(Steps.Cleanup));
        }

        #endregion

        #region Properties

        #region Synchronization

        public Steps CurrentStep
        {
            set { Step = new StepViewModel(value); }
        }

        public Phases CurrentPhase
        {
            set
            {
                switch (value)
                {
                    case Phases.PrecombatMain:
                    case Phases.PostcombatMain:
                        Step = new StepViewModel(value);
                        break;
                }
            }
        }

        #endregion

        public StepViewModel Step
        {
            get { return m_currentStep; }
            set
            {
                m_currentStep = value;
                NotifyOfPropertyChange();
            }
        }

        public IList<StepViewModel> AllSteps
        {
            get { return m_allSteps; }
        }

        public PlayerViewModel ActivePlayer
        {
            get { return m_activePlayer; }
            set
            {
                if (m_activePlayer != value)
                {
                    m_activePlayer = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion
    }
}
